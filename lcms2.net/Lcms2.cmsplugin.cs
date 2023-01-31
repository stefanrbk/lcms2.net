//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022-2023 Stefan Kewatt
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//---------------------------------------------------------------------------------
//

using lcms2.io;
using lcms2.plugins;
using lcms2.state;
using lcms2.types;

using System.Runtime.CompilerServices;
using System.Text;

using S15Fixed16Number = System.Int32;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private static readonly object contextPoolHeadMutex = new();
    private static Context? contextPoolHead;

    private static readonly Context globalContext = new()
    {
        chunks = new object?[(int)Chunks.Max]
        {
            null,
            globalLogErrorChunk,
            globalAlarmCodesChunk,
            globalAdaptationStateChunk,
            globalInterpPluginChunk,
            globalCurvePluginChunk,
            globalFormattersPluginChunk,
            globalTagTypePluginChunk,
            globalTagPluginChunk,
            globalIntentsPluginChunk,
            globalMPETypePluginChunk,
            globalOptimizationPluginChunk,
            TransformPluginChunk.global,
            globalMutexPluginChunk,
        }
    };

    internal static ushort _cmsAdjustEndianess16(ushort Word)
    {
        var pByte = (byte*)&Word;

        var tmp = *pByte;
        *pByte = *++pByte;
        *pByte = tmp;

        return Word;
    }

    internal static uint _cmsAdjustEndianess32(uint DWord)
    {
        var pByte = (byte*)&DWord;

        var temp1 = *pByte++;
        var temp2 = *pByte++;
        *(pByte - 1) = *pByte;
        *pByte++ = temp2;
        *(pByte - 3) = *pByte;
        *pByte++ = temp1;

        return DWord;
    }

    internal static void _cmsAdjustEndianess64(ulong* Result, ulong QWord)
    {
        var pIn = (byte*)&QWord;
        var pOut = (byte*)Result;

        pOut[7] = pIn[0];
        pOut[6] = pIn[1];
        pOut[5] = pIn[2];
        pOut[4] = pIn[3];
        pOut[3] = pIn[4];
        pOut[2] = pIn[5];
        pOut[1] = pIn[6];
        pOut[0] = pIn[7];
    }

    internal static bool _cmsReadUInt8Number(IOHandler io, byte* n)
    {
        byte tmp;

        if (io.Read(io, &tmp, sizeof(byte), 1) != 1)
            return false;

        if (n is not null) *n = tmp;
        return true;
    }

    internal static bool _cmsReadUInt16Number(IOHandler io, ushort* n)
    {
        ushort tmp;

        if (io.Read(io, &tmp, sizeof(ushort), 1) != 1)
            return false;

        if (n is not null) *n = _cmsAdjustEndianess16(tmp);
        return true;
    }

    internal static bool _cmsReadUInt16Array(IOHandler io, uint n, ushort* array)
    {
        for (var i = 0; i < n; i++)
        {
            if (array is not null)
            {
                if (!_cmsReadUInt16Number(io, array + i))
                    return false;
            }
            else
            {
                if (!_cmsReadUInt16Number(io, null))
                    return false;
            }
        }
        return true;
    }

    internal static bool _cmsReadUInt32Number(IOHandler io, uint* n)
    {
        uint tmp;

        if (io.Read(io, &tmp, sizeof(uint), 1) != 1)
            return false;

        if (n is not null) *n = _cmsAdjustEndianess32(tmp);
        return true;
    }

    internal static bool _cmsReadFloat32Number(IOHandler io, float* n)
    {
        uint tmp;

        if (io.Read(io, &tmp, sizeof(uint), 1) != 1)
            return false;

        if (n is not null)
        {
            tmp = _cmsAdjustEndianess32(tmp);
            *n = *(float*)(void*)&tmp;

            // Safeguard which covers against absurd values
            if (*n is > 1E+20f or < -1E+20f)
                return false;

            // I guess we don't deal with subnormal values!
            return Single.IsNormal(*n) || *n is 0;
        }

        return true;
    }

    internal static bool _cmsReadUInt64Number(IOHandler io, ulong* n)
    {
        ulong tmp;

        if (io.Read(io, &tmp, sizeof(ulong), 1) != 1)
            return false;

        if (n is not null) _cmsAdjustEndianess64(n, tmp);
        return true;
    }

    internal static bool _cmsRead15Fixed16Number(IOHandler io, double* n)
    {
        uint tmp;

        if (io.Read(io, &tmp, sizeof(uint), 1) != 1)
            return false;

        if (n is not null)
            *n = _cms15Fixed16toDouble((S15Fixed16Number)_cmsAdjustEndianess32(tmp));

        return true;
    }

    internal static bool _cmsReadXYZNumber(IOHandler io, CIEXYZ* XYZ)
    {
        EncodedXYZNumber xyz;

        if (io.Read(io, &xyz, (uint)sizeof(EncodedXYZNumber), 1) != 1)
            return false;

        if (XYZ is not null)
        {
            XYZ->X = _cms15Fixed16toDouble((S15Fixed16Number)_cmsAdjustEndianess32((uint)xyz.X));
            XYZ->Y = _cms15Fixed16toDouble((S15Fixed16Number)_cmsAdjustEndianess32((uint)xyz.Y));
            XYZ->Z = _cms15Fixed16toDouble((S15Fixed16Number)_cmsAdjustEndianess32((uint)xyz.Z));
        }

        return true;
    }

    internal static bool _cmsWriteUInt8Number(IOHandler io, byte n) =>
        io.Write(io, sizeof(byte), &n);

    internal static bool _cmsWriteUInt16Number(IOHandler io, ushort n)
    {
        var tmp = _cmsAdjustEndianess16(n);

        return io.Write(io, sizeof(ushort), &tmp);
    }

    internal static bool _cmsWriteUInt16Array(IOHandler io, uint n, in ushort* array)
    {
        for (var i = 0; i < n; i++)
        {
            if (!_cmsWriteUInt16Number(io, array[i])) return false;
        }

        return true;
    }

    internal static bool _cmsWriteUInt32Number(IOHandler io, uint n)
    {
        var tmp = _cmsAdjustEndianess32(n);

        return io.Write(io, sizeof(uint), &tmp);
    }

    internal static bool _cmsWriteFloat32Number(IOHandler io, float n)
    {
        var tmp = *(uint*)(void*)&n;
        tmp = _cmsAdjustEndianess32(tmp);

        return io.Write(io, sizeof(uint), &tmp);
    }

    internal static bool _cmsWriteUInt64Number(IOHandler io, ulong n)
    {
        ulong tmp;
        _cmsAdjustEndianess64(&tmp, n);

        return io.Write(io, sizeof(ulong), &tmp);
    }

    internal static bool _cmsWrite15Fixed16Number(IOHandler io, uint n)
    {
        var tmp = _cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(n));

        return io.Write(io, sizeof(uint), &tmp);
    }

    internal static bool _cmsWriteXYZNumber(IOHandler io, CIEXYZ XYZ)
    {
        EncodedXYZNumber xyz;

        xyz.X = (S15Fixed16Number)_cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(XYZ.X));
        xyz.Y = (S15Fixed16Number)_cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(XYZ.Y));
        xyz.Z = (S15Fixed16Number)_cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(XYZ.Z));

        return io.Write(io, (uint)sizeof(EncodedXYZNumber), &xyz);
    }

    internal static double _cms8Fixed8toDouble(ushort fixed8)
    {
        var lsb = (byte)(fixed8 & 0xff);
        var msb = (byte)((fixed8 >> 8) & 0xff);

        return msb + (lsb / 255.0);
    }

    internal static ushort _cmsDoubleTo8Fixed8(double val)
    {
        var tmp = _cmsDoubleTo15Fixed16(val);
        return (ushort)((tmp >> 8) & 0xffff);
    }

    internal static double _cms15Fixed16toDouble(S15Fixed16Number fix32)
    {
        var sign = fix32 < 0 ? -1 : 1;
        fix32 = Math.Abs(fix32);

        var whole = (ushort)((fix32 >> 16) & 0xffff);
        var fracPart = (ushort)(fix32 & 0xffff);

        var mid = fracPart / 65536.0;
        var floater = whole + mid;

        return sign * floater;
    }

    internal static S15Fixed16Number _cmsDoubleTo15Fixed16(double v) =>
        (S15Fixed16Number)Math.Floor((v * 65536.0) + 0.5);

    internal static DateTime _cmsDecodeDateTimeNumber(DateTimeNumber source)
    {
        var sec = _cmsAdjustEndianess16(source.Seconds);
        var min = _cmsAdjustEndianess16(source.Minutes);
        var hour = _cmsAdjustEndianess16(source.Hours);
        var day = _cmsAdjustEndianess16(source.Day);
        var mon = _cmsAdjustEndianess16(source.Month);
        var year = _cmsAdjustEndianess16(source.Year);

        return new(year, mon, day, hour, min, sec);
    }

    internal static void _cmsEncodeDateTimeNumber(DateTimeNumber* dest, DateTime source)
    {
        dest->Seconds = _cmsAdjustEndianess16((ushort)source.Second);
        dest->Minutes = _cmsAdjustEndianess16((ushort)source.Minute);
        dest->Hours = _cmsAdjustEndianess16((ushort)source.Hour);
        dest->Day = _cmsAdjustEndianess16((ushort)source.Day);
        dest->Month = _cmsAdjustEndianess16((ushort)source.Month);
        dest->Year = _cmsAdjustEndianess16((ushort)source.Year);
    }

    internal static Signature _cmsReadTypeBase(IOHandler io)
    {
        TagBase Base;

        if (io.Read(io, &Base, (uint)sizeof(TagBase), 1) != 1)
            return default;

        return new(_cmsAdjustEndianess32(Base.Signature));
    }

    internal static bool _cmsWriteTypeBase(IOHandler io, Signature sig)
    {
        TagBase Base;

        Base.Signature = new(_cmsAdjustEndianess32(sig));
        return io.Write(io, (uint)sizeof(TagBase), &Base);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint _cmsALIGNLONG(uint x) =>
        (x + ((uint)sizeof(uint) - 1)) & ~((uint)sizeof(uint) - 1);

    internal static bool _cmsReadAlignment(IOHandler io)
    {
        var Buffer = stackalloc byte[4];

        var At = io.Tell(io);
        var NextAligned = _cmsALIGNLONG(At);
        var BytesToNextAlignedPos = NextAligned - At;
        if (BytesToNextAlignedPos is 0) return true;
        if (BytesToNextAlignedPos > 4) return false;

        return io.Read(io, Buffer, BytesToNextAlignedPos, 1) == 1;
    }

    internal static bool _cmsWriteAlignment(IOHandler io)
    {
        var Buffer = stackalloc byte[4];

        var At = io.Tell(io);
        var NextAligned = _cmsALIGNLONG(At);
        var BytesToNextAlignedPos = NextAligned - At;
        if (BytesToNextAlignedPos is 0) return true;
        if (BytesToNextAlignedPos > 4) return false;

        return io.Write(io, BytesToNextAlignedPos, Buffer);
    }

    internal static bool _cmsIOPrintf(IOHandler io, string frm, params object[] args)
    {
        var str = new StringBuilder(String.Format(frm, args));
        str.Replace(',', '.');
        if (str.Length > 2047) return false;
        var buffer = Encoding.UTF8.GetBytes(str.ToString());

        fixed (byte* ptr = buffer)
        {
            return io.Write(io, (uint)str.Length, ptr);
        }
    }

    public static bool cmsPlugin(PluginBase plugin) =>
        cmsPluginTHR(null, plugin);

    public static bool cmsPluginTHR(Context? id, PluginBase? Plugin)
    {
        for (
            ;
            Plugin is not null;
            Plugin = Plugin.Next)
        {
            if (Plugin.Magic != Signature.Plugin.MagicNumber)
            {
                cmsSignalError(id, ErrorCode.UnknownExtension, "Unrecognized plugin");
                return false;
            }

            if (Plugin.ExpectedVersion > Version)
            {
                cmsSignalError(id, ErrorCode.UnknownExtension, $"plugin needs Little CMS {Plugin.ExpectedVersion}, current version is {Version}");
                return false;
            }

            switch (Plugin)
            {
                case PluginInterpolation:
                    if (Plugin.Type != Signature.Plugin.Interpolation || !_cmsRegisterInterpPlugin(id, Plugin))
                        return false;
                    break;

                case PluginTagType:
                    if (Plugin.Type == Signature.Plugin.TagType)
                    {
                        return _cmsRegisterTagTypePlugin(id, Plugin);
                    }
                    else if (Plugin.Type == Signature.Plugin.MultiProcessElement)
                    {
                        return _cmsRegisterMultiProcessElementPlugin(id, Plugin);
                    }
                    return false;

                case PluginTag:
                    if (Plugin.Type != Signature.Plugin.Tag || !_cmsRegisterTagPlugin(id, Plugin))
                        return false;
                    break;

                case PluginFormatters:
                    if (Plugin.Type != Signature.Plugin.Formatters || !_cmsRegisterFormattersPlugin(id, Plugin))
                        return false;
                    break;

                case PluginRenderingIntent:
                    return
                        Plugin.Type == Signature.Plugin.RenderingIntent &&
                        _cmsRegisterRenderingIntentPlugin(id, Plugin);

                case PluginParametricCurves:
                    if (Plugin.Type != Signature.Plugin.ParametricCurve || !_cmsRegisterParametricCurvesPlugin(id, Plugin))
                        return false;
                    break;

                case PluginOptimization:
                    return
                        Plugin.Type == Signature.Plugin.Optimization &&
                        _cmsRegisterOptimizationPlugin(id, Plugin);

                case PluginMutex:
                    if (Plugin.Type != Signature.Plugin.Mutex || !_cmsRegisterMutexPlugin(id, Plugin))
                        return false;
                    break;

                default:
                    cmsSignalError(id, ErrorCode.UnknownExtension, $"Unrecognized plugin type '{Plugin.Type}'");
                    return false;
            }
        }

        return true;
    }

    internal static Context _cmsGetContext(Context? id)
    {
        // On null, use global settings
        if (id is null)
            return globalContext;

        // Search
        lock (contextPoolHeadMutex)
        {
            for (var ctx = contextPoolHead; ctx is not null; ctx = ctx.next)
            {
                // Found it?
                if (id == ctx)
                    return ctx;
            }
        }

        return globalContext;
    }

    internal static ref object? _cmsContextGetClientChunk(Context? id, Chunks mc)
    {
        if (mc is < 0 or >= Chunks.Max)
        {
            cmsSignalError(id, ErrorCode.Internal, "Bad context client -- possible corruption");

            return ref globalContext.chunks[(int)Chunks.UserPtr];
        }

        var ctx = _cmsGetContext(id);
        ref var ptr = ref ctx.chunks[(int)mc];

        if (ptr is not null) return ref ptr;

        // A null ptr means no special settings for that context, and this reverts to globals
        return ref globalContext.chunks[(int)mc];
    }

    /// <summary>
    ///     This function returns the given context its default, pristene state, as if no
    ///     plug-ins were declared.
    /// </summary>
    /// <remarks>
    ///     There is no way to unregister a single plug-in, as a single call to
    ///     <see cref="cmsPluginTHR"/> may register many different plug-ins
    ///     simultaneously, then there is no way to identify which plug-in to unregister.
    /// </remarks>
    public static void cmsUnregisterPlugins() =>
        cmsUnregisterPluginsTHR(null);

    /// <summary>
    ///     This function returns the given context its default, pristene state, as if no
    ///     plug-ins were declared.
    /// </summary>
    /// <remarks>
    ///     There is no way to unregister a single plug-in, as a single call to
    ///     <see cref="cmsPluginTHR"/> may register many different plug-ins
    ///     simultaneously, then there is no way to identify which plug-in to unregister.
    /// </remarks>
    public static void cmsUnregisterPluginsTHR(Context? context)
    {
        _cmsRegisterInterpPlugin(context, null);
        _cmsRegisterTagTypePlugin(context, null);
        _cmsRegisterTagPlugin(context, null);
        _cmsRegisterFormattersPlugin(context, null);
        _cmsRegisterRenderingIntentPlugin(context, null);
        _cmsRegisterParametricCurvesPlugin(context, null);
        _cmsRegisterMultiProcessElementPlugin(context, null);
        _cmsRegisterOptimizationPlugin(context, null);

        _cmsRegisterMutexPlugin(context, null);
    }

    private static void AllocChunks(Context ctx, Context? src)
    {
        _cmsAllocLogErrorChunk(ctx, src);
        _cmsAllocAlarmCodesChunk(ctx, src);
        _cmsAllocAdaptationStateChunk(ctx, src);
        _cmsAllocInterpPluginChunk(ctx, src);
        _cmsAllocCurvesPluginChunk(ctx, src);
        _cmsAllocFormattersPluginChunk(ctx, src);
        _cmsAllocTagTypePluginChunk(ctx, src);
        _cmsAllocMPETypePluginChunk(ctx, src);
        _cmsAllocTagPluginChunk(ctx, src);
        _cmsAllocIntentsPluginChunk(ctx, src);
        _cmsAllocOptimizationPluginChunk(ctx, src);

        _cmsAllocMutexPluginChunk(ctx, src);
    }

    /// <summary>
    ///     Creates a new context with optional associated plug-ins.
    /// </summary>
    /// <param name="UserData">
    ///     An optional pointer to user-defined data that will be forwarded to plug-ins and logger
    /// </param>
    public static Context? cmsCreateContext(PluginBase? Plugin, object? UserData)
    {
        // Create the context object.
        var ctx = new Context();

        // Maintain the linked list (with proper locking)
        lock (contextPoolHeadMutex)
        {
            ctx.next = contextPoolHead;
            contextPoolHead = ctx;
        }

        ctx.chunks[(int)Chunks.UserPtr] = UserData;

        AllocChunks(ctx, null);

        // Setup the plug-ins
        if (!cmsPluginTHR(ctx, Plugin))
        {
            cmsDeleteContext(ctx);
            return null;
        }

        return ctx;
    }

    /// <summary>
    ///     Duplicates a context with all associated plug-ins.
    /// </summary>
    /// <param name="NewUserData">
    ///     An optional pointer to user-defined data that will be forwarded to plug-ins and logger.<br/>
    ///     If <see langword="null"/>, the pointer to user-defined data of the original will be used.
    /// </param>
    public static Context? cmsDupContext(Context? context, object? NewUserData)
    {
        var ctx = new Context();
        var src = _cmsGetContext(context);

        var userData = NewUserData ?? src.chunks[(int)Chunks.UserPtr];

        // Maintain the linked list
        lock (contextPoolHeadMutex)
        {
            ctx.next = contextPoolHead;
            contextPoolHead = ctx;
        }

        ctx.chunks[(int)Chunks.UserPtr] = userData;

        AllocChunks(ctx, src);

        // Make sure no one failed
        for (var i = (int)Chunks.Logger; i < (int)Chunks.Max; i++)
        {
            if (ctx.chunks[i] is null)
            {
                cmsDeleteContext(ctx);
                return null;
            }
        }

        return ctx;
    }

    /// <summary>
    ///     Frees any resources associated with the given <see cref="Context"/>,
    ///     and destroys the placeholder.
    /// </summary>
    /// <remarks>
    ///     <paramref name="context"/> can no longer be used in any THR operation.
    /// </remarks>
    public static void cmsDeleteContext(Context? context)
    {
        if (context is null)
            return;

        var ctx = context;

        // Get rid of plugins
        cmsUnregisterPluginsTHR(ctx);

        // Maintain list
        lock (contextPoolHeadMutex)
        {
            if (contextPoolHead == ctx)
            {
                contextPoolHead = ctx.next;
            }
            else
            {
                // Search for previous
                for (
                    var prev = contextPoolHead;
                    prev is not null;
                    prev = prev.next)
                {
                    if (prev.next == ctx)
                    {
                        prev.next = ctx.next;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Returns a reference to the user data associated to the given <paramref name="context"/>,
    ///     or <see langword="null"/> if no user data was attached on context creation.
    /// </summary>
    /// <remarks>
    ///     This can be used to change the user data if needed, but probably not thread safe!
    /// </remarks>
    public static ref object? cmsGetContextUserData(Context? context) =>
        ref _cmsContextGetClientChunk(context, Chunks.UserPtr);

    /// <summary>
    ///     Provides thread-safe time
    /// </summary>
    /// <remarks>
    ///     <see cref="DateTime.UtcNow"/> is already thread-safe.
    ///     Providing for completeness.
    /// </remarks>
    internal static bool _cmsGetTime(out DateTime ptr_time)
    {
        ptr_time = DateTime.UtcNow;
        return true;
    }
}
