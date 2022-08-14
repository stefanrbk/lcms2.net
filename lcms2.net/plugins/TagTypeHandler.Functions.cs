using lcms2.io;
using lcms2.state;
using lcms2.types.type_handlers;
using lcms2.types;

using static lcms2.Helpers;

namespace lcms2.plugins;
public abstract partial class TagTypeHandler
{
    /// <summary>
    ///     Reads a position table as described in ICC spec 4.3.<br />
    ///     A table of n elements is read, where first comes n records containing offsets and sizes and
    ///     then a block containing the data itself. This allows to reuse same data in more than one entry.
    /// </summary>
    /// <param name="io">
    ///     <see cref="Stream"/> to read from</param>
    /// <returns>
    ///     Whether the read operation was successful.</returns>
    public bool ReadPositionTable(Stream io, int count, uint baseOffset, ref object cargo, PositionTableEntryFn elementFn)
    {
        var currentPos = io.Tell();

        // Verify there is enough space left to read at least two int items for count items.
        if (((io.Length - currentPos) / (2 * sizeof(uint))) < count)
            return false;

        // Let's take the offsets to each element
        var offsets = new uint[count];
        var sizes = new uint[count];

        for (var i = 0; i < count; i++) {
            if (!io.ReadUInt32Number(out var offset)) return false;
            if (!io.ReadUInt32Number(out var size)) return false;

            offsets[i] = offset + baseOffset;
            sizes[i] = size;
        }

        // Seek to each element and read it
        for (var i = 0; i < count; i++) {
            if (io.Seek(offsets[i], SeekOrigin.Begin) != offsets[i])
                return false;

            // This is the reader callback
            if (!elementFn(this, io, ref cargo, i, (int)sizes[i])) return false;
        }

        return true;
    }

    /// <summary>
    ///     Writes a position table as described in ICC spec 4.3.<br />
    ///     A table of n elements is read, where first comes n records containing offsets and sizes and
    ///     then a block containing the data itself. This allows to reuse same data in more than one entry.
    /// </summary>
    /// <param name="io">
    ///     <see cref="Stream"/> to write to</param>
    /// <returns>
    ///     Whether the read operation was successful.</returns>
    public bool WritePositionTable(Stream io, int sizeOfTag, int count, uint baseOffset, ref object cargo, PositionTableEntryFn elementFn)
    {
        // Create table
        var offsets = new uint[count];
        var sizes = new uint[count];

        // Keep starting position of curve offsets
        var dirPos = io.Tell();

        // Write a fake directory to be filled later on
        for (var i = 0; i < count; i++) {
            if (!io.Write((uint)0)) return false; // Offset
            if (!io.Write((uint)0)) return false; // Size
        }

        // Write each element. Keep track of the size as well.
        for (var i = 0; i < count; i++) {
            var before = (uint)io.Tell();
            offsets[i] = before - baseOffset;

            // Callback to write...
            if (!elementFn(this, io, ref cargo, i, sizeOfTag)) return false;

            // Now the size
            sizes[i] = (uint)io.Tell() - before;
        }

        // Write the directory
        var curPos = io.Tell();
        if (io.Seek(dirPos, SeekOrigin.Begin) != dirPos) return false;

        for (var i = 0; i < count; i++) {
            if (!io.Write(offsets[i])) return false;
            if (!io.Write(sizes[i])) return false;
        }

        return io.Seek(curPos, SeekOrigin.Begin) == curPos; // Make sure we end up at the end of the table
    }

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

    internal static bool Write16bitTables(Stream io, ref Stage.ToneCurveData tables)
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

    internal static bool WriteMatrix(Stream io, Stage mpe)
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

    internal static unsafe bool ReadSequenceId(TagTypeHandler self, Stream io, ref object cargo, int index, int sizeOfTag)
    {
        var outSeq = (Sequence)cargo;
        var seq = outSeq.Seq[index];
        var b = new byte[16];
        if (io.Read(b) != 16) return false;
        if (!self.ReadEmbeddedText(io, ref seq.Description, sizeOfTag)) return false;

        fixed (byte* profileId = seq.ProfileID.ID8, buf = &b[0]) {

            Buffer.MemoryCopy(buf, profileId, 16, 16);
        }

        return true;
    }

    internal static unsafe bool WriteSequenceId(TagTypeHandler self, Stream io, ref object cargo, int index, int sizeOfTag)
    {
        var seq = (Sequence)cargo;
        var buffer = new byte[16];

        fixed (byte* profileId = seq.Seq[index].ProfileID.ID8, buf = &buffer[0]) {

            Buffer.MemoryCopy(profileId, buf, 16, 16);
        }

        io.Write(buffer);

        // Store the MLU here
        return self.SaveDescription(io, seq.Seq[index].Description);
    }

    internal static bool ReadCountAndString(Stream io, Mlu mlu, ref int sizeOfTag, string section)
    {
        if (sizeOfTag < sizeof(uint)) return false;

        if (!io.ReadUInt32Number(out var count)) return false;

        if (count > UInt32.MaxValue - sizeof(uint)) return false;
        if (sizeOfTag < count + sizeof(uint)) return false;

        var text = new byte[count];

        if (io.Read(text) != count) return false;

        if (!mlu.SetAscii("PS", section, text)) return false;

        sizeOfTag -= (int)count + sizeof(uint);
        return true;
    }

    internal static bool WriteCountAndString(Stream io, Mlu mlu, string section)
    {
        var textSize = mlu.GetAscii("PS", section);
        var text = new byte[textSize];

        if (!io.Write(textSize)) return false;

        if (mlu.GetAscii("PS", section, ref text) == 0) return false;

        io.Write(text);

        return true;
    }

    internal ToneCurve? ReadSegmentedCurve(Stream io)
    {
        var paramsByType = new uint[] { 4, 5, 5 };
        var prevBreak = MinusInf;

        // Take signature and channels for each element.
        if (!io.ReadUInt32Number(out var rawElementSig)) return null;
        var elementSig = new Signature(rawElementSig);

        // That should be a segmented curve
        if (elementSig == Signature.CurveSegment.Segmented) return null;

        if (!io.ReadUInt32Number(out _)) return null;
        if (!io.ReadUInt16Number(out var numSegments)) return null;
        if (!io.ReadUInt16Number(out _)) return null;

        if (numSegments < 1) return null;
        var segments = new CurveSegment[numSegments];

        // Read breakpoints
        for (var i = 0; i < numSegments - 1; i++) {

            segments[i] = new();
            segments[i].X0 = prevBreak;
            if (!io.ReadFloat32Number(out segments[i].X1)) return null;
            prevBreak = segments[i].X1;
        }

        segments[numSegments - 1].X0 = prevBreak;
        segments[numSegments - 1].X1 = PlusInf;

        // Read segments
        for (var i = 0; i < numSegments; i++) {

            if (!io.ReadUInt32Number(out rawElementSig)) return null;
            elementSig = new Signature(rawElementSig);
            if (!io.ReadUInt32Number(out _)) return null;

            if (elementSig == Signature.CurveSegment.Formula) {

                if (!io.ReadUInt16Number(out var type)) return null;
                if (!io.ReadUInt16Number(out _)) return null;

                segments[i].Type = type + 6;
                if (type > 2) return null;

                for (var j = 0; j < paramsByType[type]; j++) {

                    if (!io.ReadFloat32Number(out var f)) return null;
                    segments[i].Params[j] = f;
                }
            } else if (elementSig == Signature.CurveSegment.Sampled) {

                if (!io.ReadUInt32Number(out var count)) return null;

                // The first point is implicit in the last stage, we allocate an extra note to be populated later on
                count++;
                var sp = new float[count];

                sp[0] = 0;
                for (var j = 1; j < count; j++)
                    if (!io.ReadFloat32Number(out sp[j])) return null;
                segments[i].SampledPoints = sp;
            } else {

                Context.SignalError(Context, ErrorCode.UnknownExtension, "Unknown curve element type '{0}' found.", elementSig);
                return null;
            }
        }

        ToneCurve? curve = ToneCurve.BuildSegmented(Context, segments);
        if (curve is null) return null;

        // Explore for missing implicit points
        for (var i = 0; i < numSegments; i++)
            // If sampled curve, fix it
            if (curve.Segments[i].Type == 0)
                curve.Segments[i].SampledPoints![0] = curve.Eval(curve.Segments[i].X0);

        return curve;
    }

    internal static bool WriteSegmentedCurve(Stream io, ToneCurve g)
    {
        var paramsByType = new uint[] { 4, 5, 5 };
        ref var segments = ref g.Segments;
        var numSegments = g.NumSegments;

        if (!io.Write((uint)Signature.CurveSegment.Segmented)) return false;
        if (!io.Write((uint)0)) return false;
        if (!io.Write((ushort)numSegments)) return false;
        if (!io.Write((ushort)0)) return false;

        // Write the break-points
        for (var i = 0; i < numSegments - 1; i++)
            if (!io.Write(segments[i].X1)) return false;

        // Write the segments
        for (var i = 0; i < numSegments; i++) {

            ref var actualSeg = ref segments[i];

            if (actualSeg.Type == 0) {

                // This is a sampled curve. First point is implicit in the ICC format, but not in our representation
                if (!io.Write((uint)Signature.CurveSegment.Sampled)) return false;
                if (!io.Write((uint)0)) return false;
                if (!io.Write(actualSeg.NumGridPoints - 1)) return false;

                for (var j = 1; j < actualSeg.NumGridPoints; j++)
                    if (!io.Write(actualSeg.SampledPoints[j])) return false;
            } else {

                // This is a formula-based curve.
                if (!io.Write((uint)Signature.CurveSegment.Formula)) return false;
                if (!io.Write((uint)0)) return false;

                // We only allow 1, 2 and 3 as types
                var type = actualSeg.Type - 6;
                if (type is > 2 or < 0) return false;

                if (!io.Write((ushort)type)) return false;
                if (!io.Write((ushort)0)) return false;

                for (var j = 0; j < paramsByType[type]; j++)
                    if (!io.Write((float)actualSeg.Params[j])) return false;
            }

            // It seems there is no need to align. Code is here, and for safety commented out
            if (!io.WriteAlignment()) return false;
        }

        return true;
    }

    internal static bool ReadMpeCurve(TagTypeHandler self, Stream io, ref object cargo, int index, int sizeOfTag)
    {
        var gammaTables = (ToneCurve[])cargo;

        var table = self.ReadSegmentedCurve(io);
        if (table is null) return false;
        gammaTables[index] = table;

        return true;
    }

    internal static bool WriteMpeCurve(TagTypeHandler self, Stream io, ref object cargo, int index, int sizeOfTag) =>
        WriteSegmentedCurve(io, ((Stage.ToneCurveData)cargo).TheCurves[index]);

    internal static bool ReadMpeElem(TagTypeHandler self, Stream io, ref object cargo, int index, int sizeOfTag)
    {
        var newLut = (Pipeline)cargo;
        var mpeChunk = Context.GetMultiProcessElementPlugin(self.Context);

        // Take signature and channels for each element.
        if (!io.ReadUInt32Number(out var rawSig)) return false;
        var elementSig = new Signature(rawSig);

        // The reserved placeholder
        if (!io.ReadUInt32Number(out _)) return false;

        // Read diverse MPE types
        var typeHandler = GetHandler(elementSig, mpeChunk.tagTypes);
        if (typeHandler is null) {
            Context.SignalError(self.Context, ErrorCode.UnknownExtension, "Unknown MPE type '{0}' found.", elementSig);
            return false;
        }

        if (typeHandler is not MpeStubHandler &&
            !newLut.InsertStage(StageLoc.AtEnd, (Stage?)typeHandler.Read(io, sizeOfTag, out _)))

            return false;

        return true;
    }

    protected static TagTypeHandler? GetHandler(Signature sig, TagTypeLinkedList? pluginList)
    {
        if (pluginList is null) return null;

        for (var pt = pluginList; pt is not null; pt = pt.Next)
            if (sig == pt.Handler.Signature) return pt.Handler;

        for (var pt = TagTypePluginChunk.SupportedTagTypes; pt is not null; pt = pt.Next)
            if (sig == pt.Handler.Signature) return pt.Handler;

        for (var pt = TagTypePluginChunk.SupportedMpeTypes; pt is not null; pt = pt.Next)
            if (sig == pt.Handler.Signature) return pt.Handler;

        return null;
    }

    public static TagTypeHandler? GetHandler(Context? context, Signature sig) =>
        GetHandler(sig, Context.GetTagTypePlugin(context).tagTypes);
}