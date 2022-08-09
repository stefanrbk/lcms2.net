﻿using System.ComponentModel.DataAnnotations;

using lcms2.io;
using lcms2.plugins;
using lcms2.state;

namespace lcms2.types.type_handlers;
public class ParametricCurveHandler : ITagTypeHandler
{
    public Signature Signature { get; }
    public Context? Context { get; }
    public uint ICCVersion => 0;

    public object? Duplicate(ITagTypeHandler handler, object value, int num) =>
        (value as ToneCurve)?.Clone();

    public void Free(ITagTypeHandler handler, object value) =>
        (value as ToneCurve)?.Dispose();

    public object? Read(ITagTypeHandler handler, Stream io, int sizeOfTag, out int numItems)
    {
        var @params = new double[10];

        numItems = 0;

        if (!io.ReadUInt16Number(out var type)) return null;
        if (!io.ReadUInt16Number(out _)) return null; // Reserved

        if (type > 4)
        {
            Context.SignalError(Context, ErrorCode.UnknownExtension, "Unknown parametric curve type '{0}'", type);
            return null;
        }
        var n = readParamsByType[type];

        for (var i = 0; i < n; i++)
            if (!io.Read15Fixed16Number(out @params[i])) return null;

        var newGamma = ToneCurve.BuildParametric(Context, type + 1, @params);

        numItems = 1;
        return newGamma;
    }

    public bool Write(ITagTypeHandler handler, Stream io, object value, int numItems)
    {
        var curve = (ToneCurve)value;

        var typeN = curve.Segments[0].Type;

        if (curve.NumSegments > 1 || typeN < 1)
        {
            Context.SignalError(Context, ErrorCode.UnknownExtension, "Multisegment or Inverted parametric curves cannot be written");
            return false;
        }

        if (typeN > 5)
        {
            Context.SignalError(Context, ErrorCode.UnknownExtension, "Unsupported parametric curve");
            return false;
        }

        var numParams = writeParamsByType[typeN];

        if (!io.Write((ushort)(curve.Segments[0].Type - 1))) return false;
        if (!io.Write((ushort)0)) return false; // Reserved

        for (var i = 0; i < numParams; i++)
            if (!io.Write(curve.Segments[0].Params![i])) return false;

        return true;
    }

    private static readonly int[] readParamsByType = new int[] { 1, 3, 4, 5, 7 };
    private static readonly int[] writeParamsByType = new int[] { 0, 1, 3, 4, 5, 7 };
}
