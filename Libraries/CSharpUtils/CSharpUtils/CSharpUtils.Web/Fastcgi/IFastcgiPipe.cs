namespace CSharpUtils.Fastcgi
{
    public interface IFastcgiPipe
    {
        void Write(byte[] Data, int Offset, int Length);
        int Read(byte[] Data, int Offset, int Length);
        void Close();
    }
}
