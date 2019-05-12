using System;

namespace CSPspEmuLLETest
{
    public static class TransferUtils
    {
        public static void Transfer<T>(Dma.Direction direction, ref T deviceValue, ref uint memoryValue)
        {
            if (direction == Dma.Direction.Read)
            {
                memoryValue = (uint) (object) deviceValue;
            }
            else
            {
                deviceValue = (T) (object) memoryValue;
            }
        }

        public static void TransferToArray(Dma.Direction direction, byte[] array, int offset, int size,
            ref uint memoryValue)
        {
            if (direction == Dma.Direction.Read)
            {
                switch (size)
                {
                    case 4:
                        memoryValue = BitConverter.ToUInt32(array, offset);
                        break;
                    case 2:
                        memoryValue = BitConverter.ToUInt16(array, offset);
                        break;
                    case 1:
                        memoryValue = array[offset];
                        break;
                    default: throw new NotImplementedException();
                }
            }
            else
            {
                byte[] bytes;
                switch (size)
                {
                    case 4:
                        bytes = BitConverter.GetBytes(memoryValue);
                        break;
                    case 2:
                        bytes = BitConverter.GetBytes((ushort) memoryValue);
                        break;
                    case 1:
                        bytes = BitConverter.GetBytes((byte) memoryValue);
                        break;
                    default: throw new NotImplementedException();
                }
                Buffer.BlockCopy(bytes, 0, array, offset, size);
            }
        }
    }
}