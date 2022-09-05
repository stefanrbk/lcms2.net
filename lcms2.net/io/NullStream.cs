namespace lcms2.io;

public class NullStream: Stream
{
    private long _length;

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => true;
    public override long Length => _length;
    public override long Position { get; set; }

    public override void Flush()
    { }

    public override int Read(byte[] buffer, int offset, int count)
    {
        _length = count;
        Position += _length;
        return count;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                Position = offset;
                break;

            case SeekOrigin.Current:
                Position += offset;
                break;

            case SeekOrigin.End:
                Position = _length;
                Position -= offset;
                break;
        }
        return Position;
    }

    public override void SetLength(long value) =>
        _length = value;

    public override void Write(byte[] buffer, int offset, int count)
    {
        Position += count;
        if (Position > _length)
            _length = Position;
    }
}
