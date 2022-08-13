using lcms2.state;

namespace lcms2.types;
public class Sequence : ICloneable, IDisposable
{
    public Context? Context;
    public ProfileSequenceDescription[] Seq;
    private bool disposed;

    public int SeqCount =>
        Seq.Length;

    public Sequence(Context? context, int count)
    {
        Context = context;
        Seq = new ProfileSequenceDescription[count];

        disposed = false;
    }

    public void Dispose()
    {
        if (!disposed) {
            foreach (var seq in Seq)
                seq?.Dispose();

            disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    public object Clone()
    {
        Sequence result = new(Context, SeqCount);

        for (var i = 0; i < SeqCount; i++)
            result.Seq[i] = (ProfileSequenceDescription)Seq[i].Clone();

        return result;
    }
}
