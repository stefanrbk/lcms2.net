using lcms2.state;

namespace lcms2.types;

public class Sequence: ICloneable, IDisposable
{
    public object? StateContainer;
    public ProfileSequenceDescription[] Seq;

    private bool _disposed;

    public Sequence(object? state, int count)
    {
        StateContainer = state;
        Seq = new ProfileSequenceDescription[count];

        _disposed = false;
    }

    public int SeqCount =>
        Seq.Length;

    public object Clone()
    {
        Sequence result = new(StateContainer, SeqCount);

        for (var i = 0; i < SeqCount; i++)
            result.Seq[i] = (ProfileSequenceDescription)Seq[i].Clone();

        return result;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var seq in Seq)
                seq?.Dispose();

            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
