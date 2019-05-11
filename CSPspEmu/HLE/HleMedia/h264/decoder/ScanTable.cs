namespace cscodec.h264.decoder
{
    public class ScanTable
    {
        /**
         * Scantable.
         */
        public byte[] scantable;

        public byte[] permutated = new byte[64];
        public byte[] raster_end = new byte[64];
    }
}