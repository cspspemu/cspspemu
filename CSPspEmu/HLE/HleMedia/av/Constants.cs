namespace cscodec.av
{
    public class Constants
    {
        /* picture type */
        public const int PICT_TOP_FIELD = 1;

        public const int PICT_BOTTOM_FIELD = 2;
        public const int PICT_FRAME = 3;

        public const long AV_NOPTS_VALUE = unchecked((long) 0x8000000000000000L);
    }
}