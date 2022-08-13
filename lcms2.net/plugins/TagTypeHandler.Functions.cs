using lcms2.io;
using lcms2.state;
using lcms2.types.type_handlers;
using lcms2.types;

using static lcms2.Helpers;

namespace lcms2.plugins;
public abstract partial class TagTypeHandler
{
    internal bool Read8bitTables(Stream io, ref Pipeline lut, uint numChannels)
    {
        var tables = new ToneCurve[Lcms2.MaxChannels];

        if (numChannels is > Lcms2.MaxChannels or <= 0) return false;

        var temp = new byte[256];

        for (var i = 0; i < numChannels; i++) {
            var tab = ToneCurve.BuildTabulated16(Context, 256, null);
            if (tab is null) goto Error;
            tables[i] = tab;
        }

        for (var i = 0; i < numChannels; i++) {
            if (io.Read(temp) != 256) goto Error;

            for (var j = 0; j < 256; j++)
                tables[i].Table16[j] = From8to16(temp[j]);
        }
        temp = null;

        if (!lut.InsertStage(StageLoc.AtEnd, Stage.AllocToneCurves(Context, numChannels, tables)))
            goto Error;

        for (var i = 0; i < numChannels; i++)
            tables[i].Dispose();

        return true;

    Error:
        return false;
    }

    internal bool Write8bitTables(Stream io, uint num, ref Stage.ToneCurveData tables)
    {
        for (var i = 0; i < num; i++) {
            // Usual case of identity curves
            if ((tables.TheCurves[i].NumEntries == 2) &&
                (tables.TheCurves[i].Table16[0] == 0) &&
                (tables.TheCurves[i].Table16[1] == 65535)) {
                for (var j = 0; j < 256; j++)
                    if (!io.Write((byte)j)) return false;
            } else {
                if (tables.TheCurves[i].NumEntries != 256) {
                    Context.SignalError(Context, ErrorCode.Range, "LUT8 needs 256 entries on prelinearization");
                    return false;
                } else
                    for (var j = 0; j < 256; j++) {
                        var val = From16to8(tables.TheCurves[i].Table16[j]);

                        if (!io.Write(val)) return false;
                    }
            }
        }

        return true;
    }

    internal bool Read16bitTables(Stream io, ref Pipeline lut, uint numChannels, uint numEntries)
    {
        var tables = new ToneCurve[Lcms2.MaxChannels];

        // Maybe an empty table? (this is a lcms extension)
        if (numEntries == 0) return true;

        // Check for malicious profiles
        if (numChannels is > Lcms2.MaxChannels || numEntries < 2) return false;

        for (var i = 0; i < numChannels; i++) {
            var tab = ToneCurve.BuildTabulated16(Context, numEntries, null);
            if (tab is null) goto Error;
            tables[i] = tab;

            if (!io.ReadUInt16Array((int)numEntries, out tables[i].Table16)) goto Error;
        }

        // Add the table (which may certainly be an identity, but this is up to the optimizer, not the reading code)
        if (!lut.InsertStage(StageLoc.AtEnd, Stage.AllocToneCurves(Context, numChannels, tables)))
            goto Error;

        for (var i = 0; i < numChannels; i++)
            tables[i].Dispose();

        return true;

    Error:
        for (var i = 0; i < numChannels; i++)
            if (tables[i] is not null)
                tables[i].Dispose();

        return false;
    }

    internal bool Write16bitTables(Stream io, ref Stage.ToneCurveData tables)
    {
        var numEntries = tables.TheCurves[0].NumEntries;

        for (var i = 0; i < tables.NumCurves; i++) {
            for (var j = 0; j < numEntries; j++) {
                var val = tables.TheCurves[i].Table16[j];
                if (!io.Write(val)) return false;
            }
        }

        return true;
    }

    internal Stage? ReadMatrix(Stream io, uint offset)
    {
        var dMat = new double[9];
        var dOff = new double[3];

        // Go to address
        if (io.Seek(offset, SeekOrigin.Begin) != offset) return null;

        // Read the matrix
        for (var i = 0; i < 9; i++)
            if (!io.Read15Fixed16Number(out dMat[i])) return null;

        return Stage.AllocMatrix(Context, 3, 3, in dMat, dOff);
    }

    internal Stage? ReadClut(Stream io, uint offset, uint inputChannels, uint outputChannels)
    {
        var gridPoints8 = new byte[Lcms2.MaxChannels]; // Number of grid points in each dimension.
        var gridPoints = new uint[Lcms2.MaxChannels];
        ushort[]? nullTab = null;

        if (io.Seek(offset, SeekOrigin.Begin) != offset) return null;
        if (io.Read(gridPoints8, 0, Lcms2.MaxChannels) != Lcms2.MaxChannels) return null;

        for (var i = 0; i < Lcms2.MaxChannels; i++) {
            if (gridPoints[i] == 1) return null; // Impossible value, 0 for not CLUT and at least 2 for anything else
            gridPoints[i] = gridPoints8[i];
        }

        if (!io.ReadUInt8Number(out var precision)) return null;

        if (!io.ReadUInt8Number(out _)) return null;
        if (!io.ReadUInt8Number(out _)) return null;
        if (!io.ReadUInt8Number(out _)) return null;

        var clut = Stage.AllocCLut16bitGranular(Context, gridPoints, inputChannels, outputChannels, in nullTab);
        if (clut is null || clut.Data is null) return null;

        var data = (Stage.CLutData)clut.Data;

        // Precision can be 1 or 2 bytes
        switch (precision) {
            case 1:
                for (var i = 0; i < data.NumEntries; i++) {
                    if (!io.ReadUInt8Number(out var v)) {
                        clut.Dispose();
                        return null;
                    }
                    data.Table.T[i] = From8to16(v);
                }
                break;
            case 2:
                if (!io.ReadUInt16Array(data.NumEntries, out data.Table.T)) {
                    clut.Dispose();
                    return null;
                }
                break;
            default:
                clut.Dispose();
                Context.SignalError(Context, ErrorCode.UnknownExtension, "Unknown precision of '{0}'", precision);
                return null;
        }

        return clut;
    }

    internal ToneCurve? ReadEmbeddedCurve(Stream io)
    {
        var baseType = io.ReadTypeBase();
        if (baseType.Signature == Signature.TagType.Curve) {
            var h = new CurveHandler();
            return (ToneCurve?)h.Read(io, 0, out _);
        } else if (baseType.Signature == Signature.TagType.ParametricCurve) {
            var h = new ParametricCurveHandler();
            return (ToneCurve?)h.Read(io, 0, out _);
        } else
            Context.SignalError(Context, ErrorCode.UnknownExtension, "Unknown curve type '{0}'", baseType.Signature);
        return null;
    }

    internal Stage? ReadSetOfCurves(Stream io, uint offset, uint numCurves)
    {
        Stage? lin = null;
        var curves = new ToneCurve?[Lcms2.MaxChannels];

        if (numCurves > Lcms2.MaxChannels) return null;

        if (io.Seek(offset, SeekOrigin.Begin) != offset) return null;

        for (var i = 0; i < numCurves; i++) {
            curves[i] = ReadEmbeddedCurve(io);
            if (curves[i] is null) goto Error;
            if (!io.ReadAlignment()) goto Error;
        }

        lin = Stage.AllocToneCurves(Context, numCurves, curves);

    Error:
        for (var i = 0; i < numCurves; i++)
            curves[i]?.Dispose();
        return lin;
    }

    internal bool WriteMatrix(Stream io, Stage mpe)
    {
        var m = (Stage.MatrixData)mpe.Data;

        var num = mpe.InputChannels * mpe.OutputChannels;

        // Write the matrix
        for (var i = 0; i < num; i++)
            if (!io.Write(m.Double[i])) return false;

        for (var i = 0; i < mpe.OutputChannels; i++)
            if (!io.Write(m.Offset?[i] ?? 0)) return false;

        return true;
    }

    internal bool WriteSetOfCurves(Stream io, Signature type, Stage mpe)
    {
        var num = mpe.OutputChannels;
        var curves = mpe.CurveSet;

        for (var i = 0; i < num; i++) {
            // If this is a table-based curve, use curve type even on V4
            var currentType = type;

            if ((curves[i].NumSegments == 0) ||
                ((curves[i].NumSegments == 2) &&
                (curves[i].Segments[i].Type == 0)) ||
                (curves[i].Segments[0].Type < 0))
                currentType = Signature.TagType.Curve;

            if (!io.Write(new TagBase(currentType))) return false;

            if (currentType == Signature.TagType.Curve) {
                var h = new CurveHandler(Context);
                if (!h.Write(io, curves[i], 1)) return false;
            } else if (currentType == Signature.TagType.ParametricCurve) {
                var h = new ParametricCurveHandler(Context);
                if (!h.Write(io, curves[i], 1)) return false;
            } else {
                Context.SignalError(Context, ErrorCode.UnknownExtension, "Unknown curve type '{0}'", type);
                return false;
            }

            if (!io.WriteAlignment()) return false;
        }

        return true;
    }

    internal bool WriteClut(Stream io, byte precision, Stage mpe)
    {
        var gridPoints = new byte[Lcms2.MaxChannels]; // Number of grid points in each dimension.
        var clut = (Stage.CLutData)mpe.Data;

        if (clut.HasFloatValues) {
            Context.SignalError(Context, ErrorCode.NotSuitable, "Cannot save floating point data, CLUT are 8 or 16 bit only");
            return false;
        }

        for (var i = 0; i < clut.Params[0].NumInputs; i++)
            gridPoints[i] = (byte)clut.Params[0].NumSamples[i];

        io.Write(gridPoints, 0, Lcms2.MaxChannels * sizeof(byte));

        if (!io.Write(precision)) return false;
        if (!io.Write((byte)0)) return false;
        if (!io.Write((byte)0)) return false;
        if (!io.Write((byte)0)) return false;

        // Precision can be 1 or 2 bytes
        switch (precision) {
            case 1:

                for (var i = 0; i < clut.NumEntries; i++)
                    if (!io.Write(From16to8(clut.Table.T[i]))) return false;

                break;

            case 2:

                if (!io.Write(clut.NumEntries, clut.Table.T)) return false;

                break;

            default:

                Context.SignalError(Context, ErrorCode.UnknownExtension, "Unknown precision of '{0}'", precision);
                return false;
        }

        return io.WriteAlignment();
    }

    internal bool ReadEmbeddedText(Stream io, ref Mlu mlu, int sizeOfTag)
    {
        var baseType = io.ReadTypeBase();

        TagTypeHandler h;

        if (baseType.Signature == Signature.TagType.Text)
            h = new TextHandler(Context);
        else if (baseType.Signature == Signature.TagType.TextDescription)
            h = new TextDescriptionHandler(Context);
        else if (baseType.Signature == Signature.TagType.MultiLocalizedUnicode)
            h = new MluHandler(Context);
        else
            return false;

        if (mlu is not null) mlu.Dispose();
        var temp = (Mlu?)h.Read(io, sizeOfTag, out _);
        if (temp is not null)
            mlu = temp;
        else
            return false;

        return true;
    }

    internal bool SaveDescription(Stream io, Mlu text)
    {
        TagTypeHandler h;
        TagBase tb;

        if (ICCVersion < 0x40000000)
            (h, tb) = (new TextDescriptionHandler(Context), new TagBase(Signature.TagType.TextDescription));
        else
            (h, tb) = (new MluHandler(Context), new TagBase(Signature.TagType.MultiLocalizedUnicode));

        if (!io.Write(tb)) return false;

        return h.Write(io, text, 1);
    }
}