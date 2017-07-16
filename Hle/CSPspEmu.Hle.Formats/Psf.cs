using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        public Dictionary<String, object> EntryDictionary;

        public Psf()
        {
        }

        public Psf(Stream Stream)
        {
            this.Load(Stream);
        }

        public Psf Load(Stream Stream)
        {
            EntryDictionary = new Dictionary<string, object>();
            Header = Stream.ReadStruct<HeaderStruct>();
            Entries = Stream.ReadStructVector<EntryStruct>(Header.NumberOfPairs);
            KeysStream = Stream.SliceWithLength(Header.KeyTable);
            ValuesStream = Stream.SliceWithLength(Header.ValueTable);
            foreach (var Entry in Entries)
            {
                var Key = KeysStream.ReadStringzAt(Entry.KeyOffset);
                var ValueStream = ValuesStream.SliceWithLength(Entry.ValueOffset, Entry.ValueSize);
                ;
                switch (Entry.DataType)
                {
                    case DataType.Binary:
                        EntryDictionary[Key] = ValueStream.ReadAll();
                        break;
                    case DataType.Int:
                        EntryDictionary[Key] = ValueStream.ReadStruct<int>();
                        break;
                    case DataType.Text:
                        EntryDictionary[Key] = ValueStream.ReadStringz(-1, Encoding.UTF8);
                        break;
                    default: throw(new NotImplementedException());
                }
            }
            return this;
        }
    }
}