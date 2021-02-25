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
		/// <remarks>this type hook can not hook a hooked address.</remarks>
		SimpleJumpToRet,
		/// <summary>Specifies that the hook is just a jump to hook.</summary>
		/// <remarks>this type hook can not hook a hooked address.</remarks>
		DirectJumpToHook,
		/// <summary>Specifies that the hook is to write bytes to address.</summary>
		/// <remarks>this type hook can not hook a hooked address.</remarks>
		WriteBytesHook
	};

	/// <summary>Controls the hook behavior and data.</summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Delegate,
		Inherited = false, AllowMultiple = true)]
	public sealed class HookAttribute : Attribute
	{
		/// <summary>The hook behavior.</summary>
		public HookType Type { get; }
		/// <summary>The address to hook (write a jump).</summary>
		public int Address { get; set; }
		/// <summary>The number of bytes to store and overwrite</summary>
		public int Size { get; set; }

		// not necessary
		// public string Name { get; set; }

		/// <summary> Initializes a new instance of the DynamicPatcher.HookAttribute class with the specified HookType.</summary>
		public HookAttribute(HookType type)
		{
			Type = type;
		}
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
					var function = GetDelegate(GetHookFuntionType());
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

		public Type GetHookFuntionType()
		{
			HookAttribute hook = GetHookAttribute();

            return hook.Type switch
            {
                HookType.AresHook => typeof(AresHookFunction),
                HookType.SimpleJumpToRet => typeof(Func<int>),
                HookType.DirectJumpToHook => typeof(Action),
				HookType.WriteBytesHook => typeof(Func<byte[]>),
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
				dlg = Delegate.Combine(dlg, info.GetDelegate(GetHookFuntionType()));
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
								Logger.Log("hook exception caught!");
								Helpers.PrintException(e);

								Logger.Log("TransferStation unhook to run on origin code");
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

			return Marshal.GetFunctionPointerForDelegate(GetCallableDlg());
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
