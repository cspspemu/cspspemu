using System;
using System.Collections.Generic;
using System.Linq;

namespace CSPspEmu.Hle.Managers
{
    public enum RegKeyTypes
    {
        /// <summary>
        /// Key is a directory
        /// </summary>
        Directory = 1,

        /// <summary>
        /// Key is an integer (4 bytes)
        /// </summary>
        Integer = 2,

        /// <summary>
        /// Key is a string
        /// </summary>
        String = 3,

        /// <summary>
        /// Key is a binary string
        /// </summary>
        Binary = 4,
    }

    public enum RegHandle : uint
    {
    }

    public enum RegCategoryHandle : uint
    {
    }

    public enum RegKeyHandle : uint
    {
    }

    public class HleRegistryNode : IDisposable
    {
        string Name;

        public HleRegistryNode(string Name)
        {
            this.Name = Name;
        }

        public void Flush()
        {
        }

        public void Dispose()
        {
        }
    }

    public unsafe class HleRegistryKeyNode
    {
        public RegKeyHandle Id;
        public string Name;
        public RegKeyTypes Type;
        protected object _Value;

        public object Value
        {
            set
            {
                _Value = value;
                if (value is byte[])
                {
                    Type = RegKeyTypes.Binary;
                }
                else if (value is string)
                {
                    Type = RegKeyTypes.String;
                }
                else if (value is int || value is uint)
                {
                    Type = RegKeyTypes.Integer;
                }
                else
                {
                    throw new NotImplementedException("Unknown type '" + value.GetType() + "'");
                }
            }
            get => _Value;
        }

        public uint Size
        {
            get
            {
                switch (Type)
                {
                    case RegKeyTypes.Binary: throw new NotImplementedException();
                    case RegKeyTypes.Directory: return (uint) ((string) Value).Length;
                    case RegKeyTypes.Integer: return sizeof(uint);
                    case RegKeyTypes.String: return (uint) ((string) Value).Length;
                    default: return 0;
                }
            }
        }

        public void Write(void* Buffer, uint Size)
        {
            switch (Type)
            {
                case RegKeyTypes.Binary: throw new NotImplementedException();
                case RegKeyTypes.Directory: throw new NotImplementedException();
                case RegKeyTypes.Integer:
                    *(uint*) Buffer = (uint) Value;
                    break;
                case RegKeyTypes.String: throw new NotImplementedException();
                default: throw new NotImplementedException();
            }
        }
    }

    public class HleRegistryCategoryNode : IDisposable
    {
        HleRegistryNode HleRegistryNode;
        string Name;

        Dictionary<RegKeyHandle, HleRegistryKeyNode> HleRegistryKeyNodeList =
            new Dictionary<RegKeyHandle, HleRegistryKeyNode>();

        public HleRegistryCategoryNode(HleConfig HleConfig, HleRegistryNode HleRegistryNode, string Name)
        {
            this.HleRegistryNode = HleRegistryNode;
            this.Name = Name;

            AddKey("language", (uint) HleConfig.Language);
            AddKey("button_assign", (uint) 0);
        }

        public void AddKey(string Name, object Value)
        {
            var Key = new HleRegistryKeyNode()
            {
                Id = (RegKeyHandle) HleRegistryKeyNodeList.Count,
                Name = Name,
                Value = Value,
                Type = RegKeyTypes.Integer,
            };
            HleRegistryKeyNodeList[Key.Id] = Key;
        }

        public HleRegistryKeyNode GetKeyByName(string Name)
        {
            try
            {
                return HleRegistryKeyNodeList.Single(Item => Item.Value.Name == Name).Value;
            }
            catch (InvalidOperationException)
            {
                throw new KeyNotFoundException("Can't find key '" + Name + "'");
            }
        }

        public HleRegistryKeyNode GetKeyNodeById(RegKeyHandle Id)
        {
            return HleRegistryKeyNodeList[Id];
        }

        public void Flush()
        {
        }

        public void Dispose()
        {
        }
    }

    public class HleRegistryManager
    {
        public HleUidPool<HleRegistryNode> RegHandles = new HleUidPool<HleRegistryNode>();
        public HleUidPool<HleRegistryCategoryNode> RegCategoryHandles = new HleUidPool<HleRegistryCategoryNode>();
    }
}