using DynamicPatcher;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace PatcherSample
{
    enum InitState
    {
        Blank = 0x0, // CTOR'd
        Constanted = 0x1, // values that can be set without looking at Rules (i.e. country default loadscreen)
        Ruled = 0x2, // Rules has been loaded and props set (i.e. country powerplants taken from [General])
        Inited = 0x3, // values that need the object's state (i.e. is object a secretlab? -> load default boons)
        Completed = 0x4 // INI has been read and values set
    };

    [Serializable]
    public class Extension<T>
    {
        public Pointer<T> OwnerObject { get; set; }
        InitState Initialized;
        public Extension(Pointer<T> ownerObject)
        {
            OwnerObject = ownerObject;
            Initialized = InitState.Blank;
        }

        ~Extension() { }

        public void EnsureConstanted()
        {
            if (Initialized < InitState.Constanted)
            {
                InitializeConstants();
                Initialized = InitState.Constanted;
            }
        }

        public void LoadFromINI(Pointer<CCINIClass> pINI)
        {
            if (pINI == Pointer<CCINIClass>.Zero)
            {
                return;
            }

            switch (Initialized)
            {
                case InitState.Blank:
                    EnsureConstanted();
                    goto case InitState.Constanted;
                case InitState.Constanted:
                    InitializeRuled();
                    Initialized = InitState.Ruled;
                    goto case InitState.Ruled;
                case InitState.Ruled:
                    Initialize();
                    Initialized = InitState.Inited;
                    goto case InitState.Inited;
                case InitState.Inited:
                case InitState.Completed:
                    if (pINI == CCINIClass.INI_Rules)
                    {
                        LoadFromRulesFile(pINI);
                    }
                    LoadFromINIFile(pINI);
                    Initialized = InitState.Completed;
                    break;
            }
        }

        // right after construction. only basic initialization tasks possible;
        // owner object is only partially constructed! do not use global state!
        protected virtual void InitializeConstants() { }

        protected virtual void InitializeRuled() { }

        // called before the first ini file is read
        protected virtual void Initialize() { }

        // for things that only logically work in rules - countries, sides, etc
        protected virtual void LoadFromRulesFile(Pointer<CCINIClass> pINI) { }

        // load any ini file: rules, game mode, scenario or map
        protected virtual void LoadFromINIFile(Pointer<CCINIClass> pINI) { }
    }

    public class Container<TExt, TBase> where TExt : Extension<TBase>
    {
        Dictionary<Pointer<TBase>, TExt> Items;

        Pointer<TBase> SavingObject;
        IStream SavingStream;
        string Name;

        public Container(string name)
        {
            Items = new Dictionary<Pointer<TBase>, TExt>();
            SavingObject = Pointer<TBase>.Zero;
            SavingStream = null;
            Name = name;
        }

        ~Container() { }


        public TExt FindOrAllocate(Pointer<TBase> key)
        {
            if (key.IsNull)
            {
                Logger.Log("CTOR of {0} attempted for a NULL pointer! WTF!\n", Name);
                return null;
            }

            TExt val = Find(key);
            if (val == null)
            {
                val = Activator.CreateInstance(typeof(TExt), key) as TExt;
                val.EnsureConstanted();
                Items.Add(key, val);
            }
            return val;
        }

        public TExt Find(Pointer<TBase> key)
        {
            if (Items.ContainsKey(key))
            {
                return Items[key];
            }
            return null;
        }

        public void Remove(Pointer<TBase> key)
        {
            Items.Remove(key);
        }

        public void Clear()
        {
            if (Items.Count > 0)
            {
                Logger.Log("Cleared {0} items from {1}.\n", Items.Count, Name);
                Items.Clear();
            }
        }

        public void LoadAllFromINI(Pointer<CCINIClass> pINI)
        {
            foreach (var i in Items)
            {
                i.Value.LoadFromINI(pINI);
            }
        }

        public void LoadFromINI(Pointer<TBase> key, Pointer<CCINIClass> pINI)
        {
            TExt val = Find(key);
            if (val != null)
            {
                val.LoadFromINI(pINI);
            }
        }

        public void PrepareStream(Pointer<TBase> key, IStream pStm)
        {
            Logger.Log("[PrepareStream] Next is {0:X} of type '{1}'\n", key, Name);

            SavingObject = key;
            SavingStream = pStm;
        }

        public void SaveStatic()
        {
            if (SavingObject.IsNull == false && SavingStream != null)
            {
                Logger.Log("[SaveStatic] Saving object {0:X} as '{1}'\n", (int)SavingObject, Name);

                if (!Save(SavingObject, SavingStream))
                {
                    Logger.Log("[SaveStatic] Saving failed!\n");
                }
            }
            else
            {
                Logger.Log("[SaveStatic] Object or Stream not set for '{0:X}': {1}, {2}\n", Name, (int)SavingObject, SavingStream);
            }

            SavingObject = Pointer<TBase>.Zero;
            SavingStream = null;
        }

        public void LoadStatic()
        {
            if (SavingObject.IsNull == false && SavingStream != null)
            {
                Logger.Log("[LoadStatic] Loading object {0:X} as '%s'\n", (int)SavingObject, Name);

                if (!Load(SavingObject, SavingStream))
                {
                    Logger.Log("[LoadStatic] Loading failed!\n");
                }
            }
            else
            {
                Logger.Log("[LoadStatic] Object or Stream not set for '{0:X}': {1}, {2}\n", Name, (int)SavingObject, SavingStream);
            }

            SavingObject = Pointer<TBase>.Zero;
            SavingStream = null;
        }

        // specialize this method to do type-specific stuff
        protected bool Save(Pointer<TBase> key, IStream pStm)
        {
            return SaveKey(key, pStm) != null;
        }

        // specialize this method to do type-specific stuff
        protected bool Load(Pointer<TBase> key, IStream pStm)
        {
            return LoadKey(key, pStm) != null;
        }

        protected TExt SaveKey(Pointer<TBase> key, IStream pStm)
        {
            ulong written = 0;

            if (key.IsNull)
            {
                return null;
            }
            var val = Find(key);
            //Logger.Log("\tKey maps to {0:X}\n", val);
            if (val != null)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream memory = new MemoryStream();
                formatter.Serialize(memory, val);

                byte[] buffer = memory.ToArray();
                pStm.Write(BitConverter.GetBytes(buffer.Length), sizeof(int), Pointer<ulong>.AsPointer(ref written));
                pStm.Write(buffer, buffer.Length, Pointer<ulong>.AsPointer(ref written));
                //			Debug::Log("Save used up 0x%X bytes (HRESULT 0x%X)\n", out, res);
            }

            //Debug::Log("\n\n");
            return val;
        }

        protected TExt LoadKey(Pointer<TBase> key, IStream pStm)
        {
            ulong written = 0;

            if (key.IsNull)
            {
                Logger.Log("Load attempted for a NULL pointer! WTF!\n");
                return null;
            }
            var val = FindOrAllocate(key);

            byte[] buffer = new byte[4];
            pStm.Read(buffer, sizeof(int), Pointer<ulong>.AsPointer(ref written));
            buffer = new byte[BitConverter.ToInt32(buffer, 0)];
            pStm.Read(buffer, buffer.Length, Pointer<ulong>.AsPointer(ref written));

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memory = new MemoryStream(buffer);
            formatter.Deserialize(memory);

            return val;
        }
    }

}
