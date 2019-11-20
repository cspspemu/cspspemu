using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Formats
{
    public class Psf
    {
        public enum DataType : byte
        {
            Binary = 0,
            Text = 2,
            Int = 4,
        }

        public struct HeaderStruct
        {
            public uint Magic;
            public uint Version;
            public uint KeyTable;
            public uint ValueTable;
            public uint NumberOfPairs;
        }

        public struct EntryStruct
        {
            public ushort KeyOffset;
            public byte Unknown;
            public DataType DataType;
            public uint ValueSize;
            public uint ValueSizePad;
            public uint ValueOffset;
        }

        public Stream KeysStream;
        public Stream ValuesStream;
        public HeaderStruct Header;
        public EntryStruct[] Entries;
        public Dictionary<string, object> EntryDictionary;

        public Psf()
        {
        }

        public Psf(Stream stream)
        {
            Load(stream);
        }

        public Psf Load(Stream stream)
        {
            EntryDictionary = new Dictionary<string, object>();
            Header = stream.ReadStruct<HeaderStruct>();
            Entries = stream.ReadStructVector<EntryStruct>(Header.NumberOfPairs);
            KeysStream = stream.SliceWithLength(Header.KeyTable);
            ValuesStream = stream.SliceWithLength(Header.ValueTable);
            foreach (var entry in Entries)
            {
                var key = KeysStream.ReadStringzAt(entry.KeyOffset);
                var valueStream = ValuesStream.SliceWithLength(entry.ValueOffset, entry.ValueSize);
                switch (entry.DataType)
                {
                    case DataType.Binary:
                        EntryDictionary[key] = valueStream.ReadAll();
                        break;
                    case DataType.Int:
                        EntryDictionary[key] = valueStream.ReadStruct<int>();
                        break;
                    case DataType.Text:
                        EntryDictionary[key] = valueStream.ReadStringz(-1, Encoding.UTF8);
                        break;
                    default: throw new NotImplementedException();
                }
            }
            return this;
        }
    }
}