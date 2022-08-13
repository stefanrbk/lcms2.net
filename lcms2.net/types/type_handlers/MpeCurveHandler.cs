﻿using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class MpeCurveHandler : TagTypeHandler
{
    public MpeCurveHandler(Context? context = null)
        : base(default, context, 0) { }

    public override object? Duplicate(object value, int num) =>
        (value as Stage)?.Clone();

    public override void Free(object value) =>
        (value as Stage)?.Dispose();

    public unsafe override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        // Get actual position as a basis for element offsets
        var baseOffset = (uint)(io.Tell() - sizeof(TagBase));

        if (!io.ReadUInt16Number(out var inputChans)) return null;
        if (!io.ReadUInt16Number(out var outputChans)) return null;

        if (inputChans != outputChans) return null;

        object gammaTables = new ToneCurve[inputChans];
        var mpe = ReadPositionTable(io, inputChans, baseOffset, ref gammaTables, ReadMpeCurve)
            ? Stage.AllocToneCurves(Context, inputChans, (ToneCurve[])gammaTables)
            : null;

        for (var i = 0; i < inputChans; i++)
            ((ToneCurve[])gammaTables)[i]?.Dispose();

        numItems = mpe is not null ? 1 : 0;
        return mpe;
    }

    public unsafe override bool Write(Stream io, object value, int numItems)
    {
        var mpe = (Stage)value;

        var baseOffset = (uint)(io.Tell() - sizeof(TagBase));

        // Write the header. Since those are curves, input and output channels are the same
        if (!io.Write((ushort)mpe.InputChannels)) return false;
        if (!io.Write((ushort)mpe.InputChannels)) return false;

        if (!WritePositionTable(io, 0, mpe.InputChannels, baseOffset, ref mpe.Data, WriteMpeCurve)) return false;

        return true;
    }
}