using System.Security.Cryptography;

namespace CSharpUtils.Ext
{
    /// <summary>
    /// 
    /// </summary>
    public class Crc32 : HashAlgorithm
    {
        /// <summary>
        /// 
        /// </summary>
        public const uint DefaultPolynomial = 0xedb88320;

        /// <summary>
        /// 
        /// </summary>
        public const uint DefaultSeed = 0xffffffff;

        private uint _hash;
        private uint seed;
        private uint[] table;
        private static uint[] _defaultTable;

        /// <summary>
        /// 
        /// </summary>
        public Crc32()
        {
            table = InitializeTable(DefaultPolynomial);
            seed = DefaultSeed;
            Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polynomial"></param>
        /// <param name="seed"></param>
        public Crc32(uint polynomial, uint seed)
        {
            table = InitializeTable(polynomial);
            this.seed = seed;
            Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed override void Initialize()
        {
            _hash = seed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        protected override void HashCore(byte[] buffer, int start, int length)
        {
            _hash = CalculateHash(table, _hash, buffer, start, length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override byte[] HashFinal()
        {
            var hashBuffer = UintToBigEndianBytes(~_hash);
            HashValue = hashBuffer;
            return hashBuffer;
        }

        /// <summary>
        /// 
        /// </summary>
        public override int HashSize => 32;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static uint Compute(byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static uint Compute(uint seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polynomial"></param>
        /// <param name="seed"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static uint Compute(uint polynomial, uint seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        private static uint[] InitializeTable(uint polynomial)
        {
            if (polynomial == DefaultPolynomial && _defaultTable != null)
                return _defaultTable;

            var createTable = new uint[256];
            for (var i = 0; i < 256; i++)
            {
                var entry = (uint) i;
                for (var j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                _defaultTable = createTable;

            return createTable;
        }

        private static uint CalculateHash(uint[] table, uint seed, byte[] buffer, int start, int size)
        {
            var crc = seed;
            for (int i = start; i < size; i++)
                unchecked
                {
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                }
            return crc;
        }

        private static byte[] UintToBigEndianBytes(uint x)
        {
            return new[]
            {
                (byte) ((x >> 24) & 0xff),
                (byte) ((x >> 16) & 0xff),
                (byte) ((x >> 8) & 0xff),
                (byte) (x & 0xff)
            };
        }
    }
}