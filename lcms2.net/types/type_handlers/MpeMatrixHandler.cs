using lcms2.io;
using lcms2.plugins;
using lcms2.state;

using static lcms2.Lcms2;

namespace lcms2.types.type_handlers;
public class MpeMatrixHandler : TagTypeHandler
{
    public MpeMatrixHandler(Context? context = null)
        : base(default, context, 0) { }

    public override object? Duplicate(object value, int num) =>
        (value as Stage)?.Clone();

    public override void Free(object value) =>
        (value as Stage)?.Dispose();

    public unsafe override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;

        if (!io.ReadUInt16Number(out var inputChans)) return null;
        if (!io.ReadUInt16Number(out var outputChans)) return null;

        // Input and output channels may be ANY (up to 0xFFFF),
        // but we choose to limit to 16 channels for now
        if (inputChans >= MaxChannels || outputChans >= MaxChannels) return null;

        var numElements = (uint)inputChans * outputChans;

        var matrix = new double[numElements];
        var offsets = new double[outputChans];

        for (var i = 0; i < numElements; i++) {

            if (!io.ReadFloat32Number(out var v)) return null;
            matrix[i] = v;
        }

        for (var i = 0; i < outputChans; i++) {

            if (!io.ReadFloat32Number(out var v)) return null;
            offsets[i] = v;
        }

        var mpe = Stage.AllocMatrix(Context, outputChans, inputChans, matrix, offsets);

        numItems = 1;
        return mpe;
    }

    public unsafe override bool Write(Stream io, object value, int numItems)
    {
        var mpe = (Stage)value;
        var matrix = (Stage.MatrixData)mpe.Data;

        if (!io.Write((ushort)mpe.InputChannels)) return false;
        if (!io.Write((ushort)mpe.OutputChannels)) return false;

        var numElements = mpe.InputChannels * mpe.OutputChannels;

        for (var i = 0; i < numElements; i++)
            if (!io.Write((float)matrix.Double[i])) return false;

        for (var i = 0; i < mpe.OutputChannels; i++) {
            if (!io.Write((float)(matrix.Offset?[i] ?? 0.0f))) return false;
        }

        return true;
    }
}
