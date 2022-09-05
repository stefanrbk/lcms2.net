using lcms2.io;
using lcms2.plugins;

namespace lcms2.types.type_handlers;

public class ProfileSequenceIdHandler: TagTypeHandler
{
    public ProfileSequenceIdHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public ProfileSequenceIdHandler(object? state = null)
        : this(default, state) { }

    public override object? Duplicate(object value, int num) =>
        (value as Sequence)?.Clone();

    public override void Free(object value) =>
        (value as Sequence)?.Dispose();

    public override unsafe object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        // Get actual position as a basis for element offsets
        var baseOffset = io.Tell() - sizeof(TagBase);

        // Get table count
        if (!io.ReadUInt32Number(out var count)) return null;
        sizeOfTag -= sizeof(uint);

        // Allocate an empty structure
        object outSeq = new Sequence(StateContainer, (int)count);

        // Read the position table
        if (!ReadPositionTable(io, (int)count, (uint)baseOffset, ref outSeq, ReadSequenceId))
        {
            ((IDisposable)outSeq)?.Dispose();
            return null;
        }

        // Success
        numItems = 1;
        return outSeq;
    }

    public override unsafe bool Write(Stream io, object value, int numItems)
    {
        var seq = (Sequence)value;

        // Keep the base offset
        var baseOffset = io.Tell() - sizeof(TagBase);

        // This is the table count
        if (!io.Write(seq.SeqCount)) return false;

        // This is the position table and content
        return WritePositionTable(io, 0, (uint)seq.SeqCount, (uint)baseOffset, value, WriteSequenceId);
    }
}
