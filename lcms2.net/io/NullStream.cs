namespace lcms2.io;
public class NullStream : Stream
{
    private long pointer;
    private long length;

    public override bool CanRead { get => true; }
    public override bool CanSeek { get => true; }
    public override bool CanWrite { get => true; }
    public override long Length { get => length; }
    public override long Position { get => pointer; set => pointer = value; }

    public override void Flush()
    { }
    public override int Read(byte[] buffer, int offset, int count)
    {
        length = count;
        pointer += length;
        return count;
    }
    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin) {
            case SeekOrigin.Begin:
                pointer = offset;
                break;

            case SeekOrigin.Current:
                pointer += offset;
                break;

            case SeekOrigin.End:
                pointer = length;
                pointer -= offset;
                break;
        }
        return pointer;
    }
    public override void SetLength(long value) =>
        length = value;
    public override void Write(byte[] buffer, int offset, int count)
    {
        pointer += count;
        if (pointer > length)
            length = pointer;
    }
}
