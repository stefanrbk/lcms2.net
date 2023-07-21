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
using lcms2.state;
using lcms2.types;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

using S15Fixed16Number = System.Int32;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private static readonly object contextPoolHeadMutex = new();
    private static Context? contextPoolHead;

    private static readonly Context globalContext;

    [DebuggerStepThrough]
    internal static ushort _cmsAdjustEndianess16(ushort Word)
    {
        var pByte = (byte*)&Word;

        var tmp = *pByte;
        *pByte = *++pByte;
        *pByte = tmp;

        return Word;
    }

    [DebuggerStepThrough]
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

    [DebuggerStepThrough]
    internal static void _cmsAdjustEndianess64(ulong* Result, ulong* QWord)
    {
        var pIn = (byte*)QWord;
        var pOut = (byte*)Result;

        _cmsAssert(Result);

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

        _cmsAssert(io);

        if (io.Read(io, &tmp, _sizeof<byte>(), 1) != 1)
            return false;

        if (n is not null) *n = tmp;
        return true;
    }

    internal static bool _cmsReadUInt16Number(IOHandler io, ushort* n)
    {
        ushort tmp;

        _cmsAssert(io);

        if (io.Read(io, &tmp, _sizeof<ushort>(), 1) != 1)
            return false;

        if (n is not null) *n = _cmsAdjustEndianess16(tmp);
        return true;
    }

    internal static bool _cmsReadUInt16Array(IOHandler io, uint n, ushort* array)
    {
        _cmsAssert(io);

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

        _cmsAssert(io);

        if (io.Read(io, &tmp, _sizeof<uint>(), 1) != 1)
            return false;

        if (n is not null) *n = _cmsAdjustEndianess32(tmp);
        return true;
    }

    internal static bool _cmsReadFloat32Number(IOHandler io, float* n)
    {
        uint tmp;

        _cmsAssert(io);

        if (io.Read(io, &tmp, _sizeof<uint>(), 1) != 1)
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

        _cmsAssert(io);

        if (io.Read(io, &tmp, _sizeof<ulong>(), 1) != 1)
            return false;

        if (n is not null) _cmsAdjustEndianess64(n, &tmp);
        return true;
    }

    internal static bool _cmsRead15Fixed16Number(IOHandler io, out double n)
    {
        n = default;
        uint tmp;

        _cmsAssert(io);

        if (io.Read(io, &tmp, _sizeof<uint>(), 1) != 1)
            return false;

        n = _cms15Fixed16toDouble((S15Fixed16Number)_cmsAdjustEndianess32(tmp));

        return true;
    }

    internal static bool _cmsReadXYZNumber(IOHandler io, CIEXYZ* XYZ)
    {
        EncodedXYZNumber xyz;

        _cmsAssert(io);

        if (io.Read(io, &xyz, _sizeof<EncodedXYZNumber>(), 1) != 1)
            return false;

        if (XYZ is not null)
        {
            XYZ->X = _cms15Fixed16toDouble((S15Fixed16Number)_cmsAdjustEndianess32((uint)xyz.X));
            XYZ->Y = _cms15Fixed16toDouble((S15Fixed16Number)_cmsAdjustEndianess32((uint)xyz.Y));
            XYZ->Z = _cms15Fixed16toDouble((S15Fixed16Number)_cmsAdjustEndianess32((uint)xyz.Z));
        }

        return true;
    }

    internal static bool _cmsWriteUInt8Number(IOHandler io, byte n)
    {
        _cmsAssert(io);

        return io.Write(io, _sizeof<byte>(), &n);
    }

    internal static bool _cmsWriteUInt16Number(IOHandler io, ushort n)
    {
        _cmsAssert(io);

        var tmp = _cmsAdjustEndianess16(n);

        return io.Write(io, _sizeof<ushort>(), &tmp);
    }

    internal static bool _cmsWriteUInt16Array(IOHandler io, uint n, in ushort* array)
    {
        _cmsAssert(io);
        _cmsAssert(array);

        for (var i = 0; i < n; i++)
        {
            if (!_cmsWriteUInt16Number(io, array[i])) return false;
        }

        return true;
    }

    internal static bool _cmsWriteUInt32Number(IOHandler io, uint n)
    {
        _cmsAssert(io);

        var tmp = _cmsAdjustEndianess32(n);

        return io.Write(io, _sizeof<uint>(), &tmp);
    }

    internal static bool _cmsWriteFloat32Number(IOHandler io, float n)
    {
        _cmsAssert(io);

        var tmp = *(uint*)(void*)&n;
        tmp = _cmsAdjustEndianess32(tmp);

        return io.Write(io, _sizeof<uint>(), &tmp);
    }

    internal static bool _cmsWriteUInt64Number(IOHandler io, ulong n)
    {
        ulong tmp;

        _cmsAssert(io);

        _cmsAdjustEndianess64(&tmp, &n);

        return io.Write(io, _sizeof<ulong>(), &tmp);
    }

    internal static bool _cmsWrite15Fixed16Number(IOHandler io, double n)
    {
        _cmsAssert(io);

        var tmp = _cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(n));

        return io.Write(io, _sizeof<uint>(), &tmp);
    }

    internal static bool _cmsWriteXYZNumber(IOHandler io, CIEXYZ* XYZ)
    {
        EncodedXYZNumber xyz;

        _cmsAssert(io);
        _cmsAssert(XYZ);

        xyz.X = (S15Fixed16Number)_cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(XYZ->X));
        xyz.Y = (S15Fixed16Number)_cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(XYZ->Y));
        xyz.Z = (S15Fixed16Number)_cmsAdjustEndianess32((uint)_cmsDoubleTo15Fixed16(XYZ->Z));

        return io.Write(io, _sizeof<EncodedXYZNumber>(), &xyz);
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

    internal static void _cmsDecodeDateTimeNumber(DateTimeNumber* Source, DateTime* Dest)
    {
        _cmsAssert(Dest);
        _cmsAssert(Source);

        var sec = _cmsAdjustEndianess16(Source->Seconds);
        var min = _cmsAdjustEndianess16(Source->Minutes);
        var hour = _cmsAdjustEndianess16(Source->Hours);
        var day = _cmsAdjustEndianess16(Source->Day);
        var mon = _cmsAdjustEndianess16(Source->Month);
        var year = _cmsAdjustEndianess16(Source->Year);

        *Dest = new(year, mon, day, hour, min, sec);
    }

    internal static void _cmsEncodeDateTimeNumber(DateTimeNumber* dest, DateTime* source)
    {
        _cmsAssert(dest);
        _cmsAssert(source);

        dest->Seconds = _cmsAdjustEndianess16((ushort)source->Second);
        dest->Minutes = _cmsAdjustEndianess16((ushort)source->Minute);
        dest->Hours = _cmsAdjustEndianess16((ushort)source->Hour);
        dest->Day = _cmsAdjustEndianess16((ushort)source->Day);
        dest->Month = _cmsAdjustEndianess16((ushort)source->Month);
        dest->Year = _cmsAdjustEndianess16((ushort)source->Year);
    }

    internal static Signature _cmsReadTypeBase(IOHandler io)
    {
        TagBase Base;

        _cmsAssert(io);

        if (io.Read(io, &Base, _sizeof<TagBase>(), 1) != 1)
            return default;

        return new(_cmsAdjustEndianess32(Base.Signature));
    }

    internal static bool _cmsWriteTypeBase(IOHandler io, Signature sig)
    {
        TagBase Base;

        _cmsAssert(io);

        Base.Signature = new(_cmsAdjustEndianess32(sig));
        return io.Write(io, _sizeof<TagBase>(), &Base);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint _cmsALIGNLONG(uint x) =>
        (x + (_sizeof<uint>() - 1u)) & ~(_sizeof<uint>() - 1u);

    internal static bool _cmsReadAlignment(IOHandler io)
    {
        var Buffer = stackalloc byte[4];

        _cmsAssert(io);

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

        _cmsAssert(io);

        var At = io.Tell(io);
        var NextAligned = _cmsALIGNLONG(At);
        var BytesToNextAlignedPos = NextAligned - At;
        if (BytesToNextAlignedPos is 0) return true;
        if (BytesToNextAlignedPos > 4) return false;

        return io.Write(io, BytesToNextAlignedPos, Buffer);
    }

    internal static bool _cmsIOPrintf(IOHandler io, string frm, params object[] args)
    {
        _cmsAssert(io);
        _cmsAssert(frm);

        var str = new StringBuilder(String.Format(frm, args));
        str.Replace(',', '.');
        if (str.Length > 2047) return false;
        var buffer = Encoding.UTF8.GetBytes(str.ToString());

        fixed (byte* ptr = buffer)
        {
            return io.Write(io, (uint)str.Length, ptr);
        }
    }

    //internal static T* _cmsPluginMalloc<T>(Context? ContextID) where T : struct =>
    //    (T*)_cmsPluginMalloc(ContextID, _sizeof<T>());

    //internal static void* _cmsPluginMalloc(Context? ContextID, uint size)
    //{
    //    var ctx = _cmsGetContext(ContextID);

    //    if (ctx.MemPool is null)
    //    {
    //        if (ContextID is null)
    //        {
    //            ctx.MemPool = _cmsCreateSubAlloc(null, 2 * 1024);
    //            if (ctx.MemPool is null) return null;
    //        }
    //        else
    //        {
    //            cmsSignalError(ContextID, ErrorCode.CorruptionDetected, "NULL memory pool on context");
    //            return null;
    //        }
    //    }

    //    return _cmsSubAlloc(ctx.MemPool, size);
    //}

    public static bool cmsPlugin(PluginBase plugin) =>
        cmsPluginTHR(null, plugin);

    public static bool cmsPluginTHR(Context? id, PluginBase Plug_in)
    {
        for (var Plugin = Plug_in;
                Plugin is not null;
                Plugin = Plugin.Next)
        {
            if (Plugin.Magic != cmsPluginMagicNumber)
            {
                cmsSignalError(id, ErrorCode.UnknownExtension, "Unrecognized plugin");
                return false;
            }

            if (Plugin.ExpectedVersion > LCMS_VERSION)
            {
                cmsSignalError(id, ErrorCode.UnknownExtension, $"plugin needs Little CMS {Plugin.ExpectedVersion}, current version is {LCMS_VERSION}");
                return false;
            }

            switch ((uint)Plugin.Type)
            {
                case cmsPluginMemHandlerSig:
                    if (!_cmsRegisterMemHandlerPlugin(id, Plugin)) return false;
                    break;

                case cmsPluginInterpolationSig:
                    if (!_cmsRegisterInterpPlugin(id, Plugin)) return false;
                    break;

                case cmsPluginTagTypeSig:
                    if (!_cmsRegisterTagTypePlugin(id, Plugin)) return false;
                    break;

                case cmsPluginTagSig:
                    if (!_cmsRegisterTagPlugin(id, Plugin)) return false;
                    break;

                case cmsPluginFormattersSig:
                    if (!_cmsRegisterFormattersPlugin(id, Plugin)) return false;
                    break;

                case cmsPluginRenderingIntentSig:
                    if (!_cmsRegisterRenderingIntentPlugin(id, Plugin)) return false;
                    break;

                case cmsPluginParametricCurveSig:
                    if (!_cmsRegisterParametricCurvesPlugin(id, Plugin)) return false;
                    break;

                case cmsPluginMultiProcessElementSig:
                    if (!_cmsRegisterMultiProcessElementPlugin(id, Plugin)) return false;
                    break;

                case cmsPluginOptimizationSig:
                    if (!_cmsRegisterOptimizationPlugin(id, Plugin)) return false;
                    break;

                case cmsPluginTransformSig:
                    if (!_cmsRegisterTransformPlugin(id, Plugin)) return false;
                    break;

                case cmsPluginMutexSig:
                    if (!_cmsRegisterMutexPlugin(id, Plugin)) return false;
                    break;

                default:
                    cmsSignalError(id, ErrorCode.UnknownExtension, $"Unrecognized plugin type '{Plugin.Type}'");
                    return false;
            }
        }

        // Keep a reference to the plug-in
        return true;
    }

    [DebuggerStepThrough]
    internal static Context _cmsGetContext(Context? ContextID)
    {
        Context? id = ContextID;

        // On null, use global settings
        if (id is null)
            return globalContext;

        // Search
        lock (contextPoolHeadMutex)
        {
            for (var ctx = contextPoolHead; ctx is not null; ctx = ctx.Next)
            {
                // Found it?
                if (id == ctx)
                    return ctx;
            }
        }

        return globalContext;
    }

    //[DebuggerStepThrough]
    //internal static void* _cmsContextGetClientChunk(Context ContextID, Chunks mc)
    //{
    //    if (mc is < 0 or >= Chunks.Max)
    //    {
    //        cmsSignalError(ContextID, ErrorCode.Internal, "Bad context client -- possible corruption");
    //        Debug.Fail("Bad context client -- possible corruption");

    //        return globalContext->chunks[Chunks.UserPtr];
    //    }

    //    var ctx = _cmsGetContext(ContextID);
    //    var ptr = ctx->chunks[mc];

    //    if (ptr is not null) return ptr;

    //    // A null ptr means no special settings for that context, and this reverts to globals
    //    return globalContext->chunks[mc];
    //}

    //[DebuggerStepThrough]
    //internal static T* _cmsContextGetClientChunk<T>(Context ContextID, Chunks mc) where T : struct =>
    //    (T*)_cmsContextGetClientChunk(ContextID, mc);

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
        _cmsRegisterMemHandlerPlugin(context, null);
        _cmsRegisterInterpPlugin(context, null);
        _cmsRegisterTagTypePlugin(context, null);
        _cmsRegisterTagPlugin(context, null);
        _cmsRegisterFormattersPlugin(context, null);
        _cmsRegisterRenderingIntentPlugin(context, null);
        _cmsRegisterParametricCurvesPlugin(context, null);
        _cmsRegisterMultiProcessElementPlugin(context, null);
        _cmsRegisterOptimizationPlugin(context, null);
        _cmsRegisterTransformPlugin(context, null);
        _cmsRegisterMutexPlugin(context, null);
    }

    internal static PluginMemHandler? _cmsFindMemoryPlugin(PluginBase? PluginBundle)
    {
        for (var Plugin = PluginBundle;
            Plugin is not null;
            Plugin = Plugin.Next)
        {
            if ((uint)Plugin.Magic is cmsPluginMagicNumber &&
                Plugin.ExpectedVersion <= LCMS_VERSION &&
                (uint)Plugin.Type is cmsPluginMemHandlerSig)
            {
                // Found!
                return (PluginMemHandler)Plugin;
            }
        }

        // Nope, revert to defaults
        return null;
    }

    private static void AllocChunks(Context ctx, Context? src, bool trace = false)
    {
        _cmsAllocLogErrorChunk(ctx, src);
        _cmsAllocAlarmCodesChunk(ctx, src);
        _cmsAllocAdaptationStateChunk(ctx, src);
        _cmsAllocMemPluginChunk(ctx, src);
        _cmsAllocInterpPluginChunk(ctx, src);
        _cmsAllocCurvesPluginChunk(ctx, src);
        _cmsAllocFormattersPluginChunk(ctx, src);
        _cmsAllocTagTypePluginChunk(ctx, src);
        _cmsAllocMPETypePluginChunk(ctx, src);
        _cmsAllocTagPluginChunk(ctx, src);
        _cmsAllocIntentsPluginChunk(ctx, src);
        _cmsAllocOptimizationPluginChunk(ctx, src);
        _cmsAllocTransformPluginChunk(ctx, src);
        _cmsAllocMutexPluginChunk(ctx, src);

        //static void PrintContext(Context ctx, string name)
        //{
        //    if (ctx is null)
        //    {
        //        Console.WriteLine($"\n{name}\tnull");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"\n{name}\t{(ulong)ctx}");
        //        Console.WriteLine($"\t\tNext\t{(ctx->Next is null ? "null" : (ulong)ctx->Next)}");
        //        Console.WriteLine($"\t\tMemPool\t{(ctx->MemPool is null ? "null" : (ulong)ctx->MemPool)}");
        //        Console.WriteLine($"\t\tDefaultMemoryManager\t{ctx->DefaultMemoryManager}");
        //        Console.WriteLine($"\t\tchunks");
        //        for (var i = 0; i < (int)Chunks.Max; i++)
        //            Console.WriteLine($"\t\t\t{Enum.GetName((Chunks)i)}\t{(ulong)ctx->chunks[(Chunks)i]}");
        //    }
        //}
    }

    /// <summary>
    ///     Creates a new context with optional associated plug-ins.
    /// </summary>
    /// <param name="UserData">
    ///     An optional pointer to user-defined data that will be forwarded to plug-ins and logger
    /// </param>
    public static Context? cmsCreateContext(PluginBase? Plugin, object? UserData)
    {
        Context fakeContext = new();

        _cmsInstallAllocFunctions(_cmsFindMemoryPlugin(Plugin), ref fakeContext.DefaultMemoryManager);

        fakeContext.UserData = UserData;
        fakeContext.MemPlugin = fakeContext.DefaultMemoryManager;

        // Create the context structure.
        var ctx = new Context();
        if (ctx is null) return null; // Something very wrong happened!

        // Keep memory manager
        ctx.DefaultMemoryManager = (MemPluginChunkType)fakeContext.DefaultMemoryManager.Dup(ctx)!;

        // Maintain the linked list (with proper locking)
        lock (contextPoolHeadMutex)
        {
            //Console.WriteLine($"\nAdding Context {(ulong)ctx:x16}");
            ctx.Next = contextPoolHead;
            contextPoolHead = ctx;
            //Console.WriteLine("\nAdded to the pool");
            //for (var c = contextPoolHead; c is not null; c = c->Next)
            //    Console.WriteLine($"\t{(ulong)c:x16}\n\t\tV");
            //Console.WriteLine("\tend");
        }

        ctx.UserData = UserData;
        ctx.MemPlugin = ctx.DefaultMemoryManager;

        // Now we can allocate the pool by using default memory manager
        //ctx.MemPool = _cmsCreateSubAlloc(ctx, 22u * _sizeof<nint>()); // default size about 22 pointers
        //if (ctx.MemPool is null)
        //{
        //    cmsDeleteContext(ctx);
        //    return null;
        //}

        AllocChunks(ctx, null);

        // Setup the plug-ins
        if (!cmsPluginTHR(ctx, Plugin))
        {
            cmsDeleteContext(ctx);
            return null;
        }

        ctx.ErrorLogger.FactoryChanged += ErrorLogger_FactoryChanged;

        return ctx;
    }

    private static void ErrorLogger_FactoryChanged(object? sender, FactoryChangedEventArgs e)
    {
        if (sender is LogErrorChunkType logChunk)
        {
            if (loggers.TryGetValue(logChunk, out var logger))
            {
                loggers[logChunk] = logChunk.Factory.CreateLogger("Lcms2");
            }
            else
            {
                logger = logChunk.Factory.CreateLogger("Lcms2");
                loggers.Add(logChunk, logger);

            }
        }
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
        var src = _cmsGetContext(context);

        var userData = NewUserData ?? src.UserData;

        var ctx = new Context();
        if (ctx is null)
            return null;    // Something very wrong happened

        // Setup default memory allocators
        ctx.DefaultMemoryManager = src.DefaultMemoryManager;

        // Maintain the linked list
        lock (contextPoolHeadMutex)
        {
            //Console.WriteLine($"\nAdding Context {(ulong)ctx:x16}");
            ctx.Next = contextPoolHead;
            contextPoolHead = ctx;
            //Console.WriteLine("\nAdded to the pool");
            //for (var c = contextPoolHead; c is not null; c = c->Next)
            //    Console.WriteLine($"\t{(ulong)c:x16}\n\t\tV");
            //Console.WriteLine("\tend");
        }

        ctx.UserData = userData;
        ctx.MemPlugin = ctx.DefaultMemoryManager;

        //ctx.MemPool = _cmsCreateSubAlloc(ctx, 22u * _sizeof<nint>());
        //if (ctx.MemPool is null)
        //{
        //    cmsDeleteContext(ctx);
        //    return null;
        //}

        // Allocate all required chunks.
        AllocChunks(ctx, src, true);

        // Make sure no one failed
        if (ctx.ErrorLogger is null ||
            ctx.AlarmCodes is null ||
            ctx.AdaptationState is null ||
            ctx.MemPlugin is null ||
            ctx.InterpPlugin is null ||
            ctx.CurvesPlugin is null ||
            ctx.FormattersPlugin is null ||
            ctx.TagTypePlugin is null ||
            ctx.TagPlugin is null ||
            ctx.IntentsPlugin is null ||
            ctx.MPEPlugin is null ||
            ctx.OptimizationPlugin is null ||
            ctx.TransformPlugin is null ||
            ctx.MutexPlugin is null)
        {
            cmsDeleteContext(ctx);
            return null;
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
    public static void cmsDeleteContext(Context? ctx)
    {
        //Context fakeContext = new();

        if (ctx is null)
            return;

        //fakeContext.DefaultMemoryManager = ctx.DefaultMemoryManager;

        //fakeContext.UserData = ctx.UserData;
        //fakeContext.MemPlugin = fakeContext.DefaultMemoryManager;

        // Get rid of plugins
        cmsUnregisterPluginsTHR(ctx);

        // Since all memory is allocated in the private pool, all we need to do is destroy the pool
        //if (ctx.MemPool is not null)
        //    _cmsSubAllocDestroy(ctx.MemPool);
        //ctx.MemPool = null;

        // Maintain list
        lock (contextPoolHeadMutex)
        {
            //Console.WriteLine($"\nRemoving Context {(ulong)ctx:x16}");
            if (contextPoolHead == ctx)
            {
                contextPoolHead = ctx.Next;
            }
            else
            {
                // Search for previous
                for (
                    var prev = contextPoolHead;
                    prev is not null;
                    prev = prev.Next)
                {
                    if (prev.Next == ctx)
                    {
                        prev.Next = ctx.Next;
                        break;
                    }
                }
            }
            //Console.WriteLine("\nRemoved from the pool");
            //for (var c = contextPoolHead; c is not null; c = c->Next)
            //    Console.WriteLine($"\t{(ulong)c:x16}\n\t\tV");
            //Console.WriteLine("\tend");
        }

        // free the memory block itself
        //_cmsFree(fakeContext, ctx);
    }

    /// <summary>
    ///     Returns a reference to the user data associated to the given <paramref name="context"/>,
    ///     or <see langword="null"/> if no user data was attached on context creation.
    /// </summary>
    /// <remarks>
    ///     This can be used to change the user data if needed, but probably not thread safe!
    /// </remarks>
    public static ref object? cmsGetContextUserData(Context? context) =>
        ref _cmsGetContext(context).UserData;

    /// <summary>
    ///     Provides thread-safe time
    /// </summary>
    /// <remarks>
    ///     <see cref="DateTime.UtcNow"/> is already thread-safe.
    ///     Providing for completeness.
    /// </remarks>
    internal static bool _cmsGetTime(DateTime* ptr_time)
    {
        *ptr_time = DateTime.UtcNow;
        return true;
    }

    //private static void AllocPluginChunk<T>(Context ctx, in Context src, Chunks mc, T* defaultChunk) where T : struct
    //{
    //    _cmsAssert(ctx);

    //    var from = (src is not null) ? src->chunks[mc] : defaultChunk;

    //    _cmsAssert(from);
    //    ctx->chunks[mc] = _cmsSubAllocDup<T>(ctx->MemPool, from);
    //}

    //private delegate void DupPlugin(Context ctx, in Context src);
    //private static void AllocPluginChunk<T>(Context ctx, in Context src, DupPlugin dup, Chunks mc, T* defaultChunk) where T : struct
    //{
    //    Debug.Assert(ctx is not null);

    //    if (src is not null)
    //    {
    //        // Duplicate the list
    //        dup(ctx, src);
    //    }
    //    else
    //    {
    //        ctx->chunks[mc] = _cmsSubAllocDup<T>(ctx->MemPool, defaultChunk);
    //    }
    //}
}
