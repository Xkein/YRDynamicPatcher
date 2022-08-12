using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using dword = System.UInt32;
using word = System.UInt16;

namespace DynamicPatcher
{
	/// <summary>Specifies the hook behavior.</summary>
	public enum HookType
	{
		/// <summary>Specifies that the hook is a ares style hook.</summary>
		AresHook,
		/// <summary>Specifies that the hook is just a jump to destination.</summary>
		/// <remarks>This type hook can not hook a hooked address.</remarks>
		SimpleJumpToRet,
		/// <summary>Specifies that the hook is just a jump to hook.</summary>
		/// <remarks>This type hook can not hook a hooked address.</remarks>
		DirectJumpToHook,
		/// <summary>Specifies that the hook is to write bytes to address.</summary>
		/// <remarks>This type hook can not hook a hooked address.</remarks>
		WriteBytesHook,

		/// <summary>Specifies that the hook is to overwrite exported target reference address.</summary>
		ExportTableHook,
		/// <summary>Specifies that the hook is to overwrite imported target reference address.</summary>
		ImportTableHook
	};

	/// <summary>Controls the hook behavior and data.</summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Delegate,
		Inherited = false, AllowMultiple = true)]
	public sealed class HookAttribute : Attribute
	{
		/// <summary>The hook behavior.</summary>
		public HookType Type { get; }
		/// <summary>The absolute address to hook (write a jump).</summary>
		/// <remarks>If Module was set, set RelativeAddress is safer.</remarks>
		public int Address
		{
			get
			{
				switch (Type)
				{
					case HookType.ExportTableHook:
						return Helpers.GetEATAddress(Module, TargetName);
					case HookType.ImportTableHook:
						return Helpers.GetIATAddress(Module, TargetName);
				}

				if (string.IsNullOrEmpty(Module))
				{
					return _address;
				}

                return (int)Helpers.GetProcessModule(Module).BaseAddress + RelativeAddress;
			}
			set
			{
				switch (Type)
				{
					case HookType.ExportTableHook:
					case HookType.ImportTableHook:
						throw new InvalidOperationException($"you can not set address for {Type}.");
				}

				if (string.IsNullOrEmpty(Module))
				{
					_address = value;
					return;
				}

				// set RelativeAddress
				_address = value - (int)Helpers.GetProcessModule(Module).BaseAddress;
            }
		}
		/// <summary>The relative address to hook.</summary>
		/// <remarks>If Module was not set, set Address is safer.</remarks>
		public int RelativeAddress
		{
			get
			{
				switch (Type)
				{
					case HookType.ExportTableHook:
					case HookType.ImportTableHook:
						return Address - (int)Helpers.GetProcessModule(Module).BaseAddress;
				}

				if (string.IsNullOrEmpty(Module))
				{
					return Address - (int)Helpers.GetProcessModule().BaseAddress;
				}

				return _address;
			}
			set
			{
				switch (Type)
				{
					case HookType.ExportTableHook:
					case HookType.ImportTableHook:
						throw new InvalidOperationException($"you can not set address for {Type}.");
				}

				if (string.IsNullOrEmpty(Module))
				{
					// set Address
					_address = (int)Helpers.GetProcessModule().BaseAddress + value;
					return;
				}

				// set RelativeAddress
				_address = value;
			}
		}
		/// <summary>The number of bytes to store and overwrite.</summary>
		public int Size
		{
			get
			{
				switch (Type)
				{
					case HookType.ExportTableHook:
					case HookType.ImportTableHook:
						return sizeof(uint);
				}

				return _size;
			}
			set
			{
				switch (Type)
				{
					case HookType.ExportTableHook:
					case HookType.ImportTableHook:
						throw new InvalidOperationException($"you can not set size for {Type}.");
				}

				_size = value;
			}
		}
		/// <summary>The module to hook.</summary>
		public string Module { get; }
		/// <summary>The export or import name of target.</summary>
		public string TargetName { get; set; }

		// not necessary
		// public string Name { get; set; }

		/// <summary> Initializes a new instance of the DynamicPatcher.HookAttribute class with the specified HookType.</summary>
		public HookAttribute(HookType type, string module = null)
		{
			Type = type;
			Module = module;
		}

		private int _address;
		private int _size;
	}
	class HookInfo : IComparable
	{
		public static bool TryCatchCallable { get; set; } = false;

		public MemberInfo Member { get; private set; }
		public HookAttribute HookAttribute { get; private set; }

		public FieldInfo Field { get => Member as FieldInfo; }
		public EventInfo Event { get => Member as EventInfo; }
		public MethodInfo Method { get => Member as MethodInfo; }
		public PropertyInfo Property { get => Member as PropertyInfo; }

		public HookTransferStation TransferStation { get; set; } = null;
		public HookInfo(MemberInfo member, HookAttribute hookAttribute)
		{
			Member = member;
			HookAttribute = hookAttribute;
		}

		public HookAttribute GetHookAttribute()
		{
			return HookAttribute;
		}

		static public HookAttribute[] GetHookAttributes(MemberInfo member)
        {
			return member.GetCustomAttributes(typeof(HookAttribute), false) as HookAttribute[];
        }

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate dword AresHookFunction(REGISTERS* R);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate dword AresHookDelegate(IntPtr R);

		public object GetReturnValue()
		{
			switch (Member.MemberType)
			{
				case MemberTypes.Method:
				case MemberTypes.Event:
				case MemberTypes.Field when typeof(Delegate).IsAssignableFrom(Field.FieldType):
				case MemberTypes.Property when typeof(Delegate).IsAssignableFrom(Property.PropertyType):
					var function = GetDelegate();
					return function.DynamicInvoke();

				case MemberTypes.Field:
					return Field.GetValue(null);
				case MemberTypes.Property:
					return Property.GetValue(null);
			}

			return null;
		}
		public Delegate GetDelegate(Type dlgType)
		{
            return Member.MemberType switch
            {
                MemberTypes.Method => Method.CreateDelegate(dlgType),
                MemberTypes.Event => Event.RaiseMethod.CreateDelegate(dlgType),
                MemberTypes.Field when typeof(Delegate).IsAssignableFrom(Field.FieldType) => Field.GetValue(null) as Delegate,
                MemberTypes.Property when typeof(Delegate).IsAssignableFrom(Property.PropertyType) => Property.GetValue(null) as Delegate,
                _ => null,
            };
		}
		public Delegate GetDelegate()
		{
            return Member.MemberType switch
            {
                MemberTypes.Method or MemberTypes.Event => GetDelegate(GetHookFuntionType()),
                _ => GetDelegate(null),
            };
        }

		public Type GetHookFuntionType()
		{
			HookAttribute hook = GetHookAttribute();

            return hook.Type switch
            {
                HookType.AresHook => typeof(AresHookFunction),
                HookType.SimpleJumpToRet => typeof(Func<int>),
				HookType.WriteBytesHook => typeof(Func<byte[]>),
				HookType.DirectJumpToHook or
				HookType.ExportTableHook or
				HookType.ImportTableHook => Helpers.GetMethodDelegateType(Method ?? Event?.RaiseMethod),
				_ => null,
            };
        }

		// avoid GC collect
		public Delegate CallableDlg { get; set; }
		public Delegate GetCallableDlg()
        {
			if(CallableDlg != null)
            {
				//return CallableDlg;
			}

			Delegate dlg = null;

			foreach (HookInfo info in TransferStation.HookInfos)
			{
				dlg = Delegate.Combine(dlg, info.GetDelegate());
			}

			if (TryCatchCallable)
			{
				HookAttribute hook = GetHookAttribute();

				switch (hook.Type)
				{
					case HookType.AresHook:
						var aresHook = dlg as AresHookFunction;
						AresHookDelegate tryAresCallable = (IntPtr R) =>
						{
							try
							{
								unsafe
								{
									return aresHook.Invoke((REGISTERS*)R);
								}
							}
							catch (Exception e)
							{
								Logger.LogError("hook exception caught!");
								Logger.PrintException(e);

								Logger.LogWarning("TransferStation unhook to run on origin code");
								TransferStation.UnHook();
								return (uint)hook.Address;
							}
						};
						dlg = tryAresCallable;
						break;
				}
			}

			CallableDlg = dlg;

			return dlg;
		}

		public IntPtr GetCallable()
		{
			switch (Member.MemberType)
			{
				case MemberTypes.Method:
				case MemberTypes.Event:
				case MemberTypes.Field when typeof(Delegate).IsAssignableFrom(Field.FieldType):
				case MemberTypes.Property when typeof(Delegate).IsAssignableFrom(Property.PropertyType):
					break;
				case MemberTypes.Field:
					return (IntPtr)Field.GetValue(null);
				case MemberTypes.Property:
					return (IntPtr)Property.GetValue(null);
			}

			var callableDlg = GetCallableDlg();
			return Marshal.GetFunctionPointerForDelegate(callableDlg);
		}

        public int CompareTo(object obj)
        {
			return CompareTo((HookInfo)obj);
		}
		public int CompareTo(HookInfo obj)
		{
			return this.GetHookAttribute().Size.CompareTo(obj.GetHookAttribute().Size);
		}
	}
}
