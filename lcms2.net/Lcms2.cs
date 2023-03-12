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

using lcms2.state;
using lcms2.types;

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace lcms2;

public static unsafe partial class Lcms2
{
    #region lcms2.h

    internal const ushort LCMS_VERSION = 2131;

    internal const ushort MAX_PATH = 256;

    internal const double cmsD50X = 0.9642;
    internal const double cmsD50Y = 1.0;
    internal const double cmsD50Z = 0.8249;

    internal const double cmsPERCEPTUAL_BLACK_X = 0.00336;
    internal const double cmsPERCEPTUAL_BLACK_Y = 0.0034731;
    internal const double cmsPERCEPTUAL_BLACK_Z = 0.00287;

    internal const byte cmsMAXCHANNELS = 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T PREMUL_SH<T>(T m) where T : IShiftOperators<T, int, T> => m << 23;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T FLOAT_SH<T>(T m) where T : IShiftOperators<T, int, T> => m << 22;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T OPTIMIZED_SH<T>(T m) where T : IShiftOperators<T, int, T> => m << 21;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T COLORSPACE_SH<T>(T m) where T : IShiftOperators<T, int, T> => m << 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T SWAPFIRST_SH<T>(T m) where T : IShiftOperators<T, int, T> => m << 14;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T FLAVOR_SH<T>(T m) where T : IShiftOperators<T, int, T> => m << 13;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T PLANAR_SH<T>(T m) where T : IShiftOperators<T, int, T> => m << 12;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T ENDIAN16_SH<T>(T m) where T : IShiftOperators<T, int, T> => m << 11;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T DOSWAP_SH<T>(T m) where T : IShiftOperators<T, int, T> => m << 10;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T EXTRA_SH<T>(T m) where T : IShiftOperators<T, int, T> => m << 7;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T CHANNELS_SH<T>(T m) where T : IShiftOperators<T, int, T> => m << 3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T BYTES_SH<T>(T m) where T : IShiftOperators<T, int, T> => m << 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T T_PREMUL<T>(T m) where T : IShiftOperators<T, int, T>, IBitwiseOperators<T, int, T> => (m >> 23) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T T_FLOAT<T>(T m) where T : IShiftOperators<T, int, T>, IBitwiseOperators<T, int, T> => (m >> 22) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T T_OPTIMIZED<T>(T m) where T : IShiftOperators<T, int, T>, IBitwiseOperators<T, int, T> => (m >> 21) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T T_COLORSPACE<T>(T m) where T : IShiftOperators<T, int, T>, IBitwiseOperators<T, int, T> => (m >> 16) & 31;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T T_SWAPFIRST<T>(T m) where T : IShiftOperators<T, int, T>, IBitwiseOperators<T, int, T> => (m >> 14) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T T_FLAVOR<T>(T m) where T : IShiftOperators<T, int, T>, IBitwiseOperators<T, int, T> => (m >> 13) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T T_PLANAR<T>(T m) where T : IShiftOperators<T, int, T>, IBitwiseOperators<T, int, T> => (m >> 12) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T T_ENDIAN16<T>(T m) where T : IShiftOperators<T, int, T>, IBitwiseOperators<T, int, T> => (m >> 11) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T T_DOSWAP<T>(T m) where T : IShiftOperators<T, int, T>, IBitwiseOperators<T, int, T> => (m >> 10) & 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T T_EXTRA<T>(T m) where T : IShiftOperators<T, int, T>, IBitwiseOperators<T, int, T> => (m >> 7) & 7;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T T_CHANNELS<T>(T m) where T : IShiftOperators<T, int, T>, IBitwiseOperators<T, int, T> => (m >> 3) & 15;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T T_BYTES<T>(T m) where T : IShiftOperators<T, int, T>, IBitwiseOperators<T, int, T> => (m >> 0) & 7;

    internal const int PT_ANY = 0;
    internal const int PT_GRAY = 3;
    internal const int PT_RGB = 4;
    internal const int PT_CMY = 5;
    internal const int PT_CMYK = 6;
    internal const int PT_YCbCr = 7;
    internal const int PT_YUV = 8;
    internal const int PT_XYZ = 9;
    internal const int PT_Lab = 10;
    internal const int PT_YUVK = 11;
    internal const int PT_HSV = 12;
    internal const int PT_HLS = 13;
    internal const int PT_Yxy = 14;
    internal const int PT_MCH1 = 15;
    internal const int PT_MCH2 = 16;
    internal const int PT_MCH3 = 17;
    internal const int PT_MCH4 = 18;
    internal const int PT_MCH5 = 19;
    internal const int PT_MCH6 = 20;
    internal const int PT_MCH7 = 21;
    internal const int PT_MCH8 = 22;
    internal const int PT_MCH9 = 23;
    internal const int PT_MCH10 = 24;
    internal const int PT_MCH11 = 25;
    internal const int PT_MCH12 = 26;
    internal const int PT_MCH13 = 27;
    internal const int PT_MCH14 = 28;
    internal const int PT_MCH15 = 29;
    internal const int PT_LabV2 = 30;

    #endregion lcms2.h

    #region lcms2_plugin.h

    internal const int VX = 0;
    internal const int VY = 1;
    internal const int VZ = 2;

    internal const byte MAX_TYPES_IN_LCMS_PLUGIN = 20;

    internal const byte MAX_INPUT_DIMENSIONS = 15;

    #endregion lcms2_plugin.h

    #region lcms2_internal.h

    internal const double M_PI = 3.14159265358979323846;
    internal const double M_LOG10E = 0.434294481903251827651;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T _cmsALIGNLONG<T>(T x) where T : IBitwiseOperators<T, uint, T>, IAdditionOperators<T, uint, T> =>
        (x + ((uint)sizeof(uint) - 1)) & ~((uint)sizeof(uint) - 1);

    internal static ushort CMS_PTR_ALIGNMENT = (ushort)sizeof(void*);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T _cmsALIGNMEM<T>(T x) where T : IBitwiseOperators<T, uint, T>, IAdditionOperators<T, uint, T> =>
        (x + ((uint)CMS_PTR_ALIGNMENT - 1)) & ~((uint)CMS_PTR_ALIGNMENT - 1);

    internal const double MAX_ENCODEABLE_XYZ = 1 + (32767.0 / 32768);
    internal const double MIN_ENCODEABLE_ab2 = -128.0;
    internal const double MAX_ENCODEABLE_ab2 = (65535.0 / 256) - 128;
    internal const double MIN_ENCODEABLE_ab4 = -128.0;
    internal const double MAX_ENCODEABLE_ab4 = 127.0;

    internal const byte MAX_STAGE_CHANNELS = 128;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort FROM_8_TO_16(uint rgb) => (ushort)((rgb << 8) | rgb);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static byte FROM_16_TO_8(uint rgb) => (byte)((((rgb * 65281u) + 8388608u) >> 24) & 0xFFu);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void _cmsAssert(bool condition) =>
        Debug.Assert(condition);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void _cmsAssert(void* ptr) =>
        Debug.Assert(ptr is not null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void _cmsAssert(object? obj) =>
        Debug.Assert(obj is not null);

    internal const double MATRIX_DET_TOLERANCE = 1e-4;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int FIXED_TO_INT(int x) => x >> 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int FIXED_REST_TO_INT(int x) => x & 0xFFFF;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ROUND_FIXED_TO_INT(int x) => (x + 0x8000) >> 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int _cmsToFixedDomain(int a) => a + ((a + 0x7fff) / 0xffff);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int _cmsFromFixedDomain(int a) => a - ((a + 0x7fff) >> 16);

    [StructLayout(LayoutKind.Explicit)]
    internal struct _temp
    {
        [FieldOffset(0)]
        public double val;

        [FieldOffset(0)]
        public fixed int halves[2];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int _cmsQuickFloor(double val)
    {
#if CMS_DONT_USE_FAST_FLOOR
        (int)Math.Floor(val);
#else
        const double _lcms_double2fixmagic = 68719476736.0 * 1.5;
        _temp temp;
        temp.val = val + _lcms_double2fixmagic;

        return temp.halves[0] >> 16;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort _cmsQuickFloorWord(double d) =>
        (ushort)(_cmsQuickFloor(d - 32767) + 32767);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort _cmsQuickSaturateWord(double d)
    {
        d += 0.5;
        return d switch
        {
            <= 0 => 0,
            >= 65535.0 => 0xffff,
            _ => _cmsQuickFloorWord(d),
        };
    }

    internal const byte MAX_TABLE_TAG = 100;

    #endregion lcms2_internal.h

    private static readonly Destructor Finalise = new();

    private sealed class Destructor
    {
        ~Destructor()
        {
            // Context and plugins
            free(globalContext);
            free(globalLogErrorChunk);
            free(globalAlarmCodesChunk);
            free(globalAdaptationStateChunk);
            free(globalMemPluginChunk);
            free(globalInterpPluginChunk);
            free(globalCurvePluginChunk);
            free(globalFormattersPluginChunk);
            free(supportedTagTypes);
            free(globalTagTypePluginChunk);
            free(supportedTags);
            free(globalTagPluginChunk);
            free(globalIntentsPluginChunk);
            free(supportedMPEtypes);
            free(globalMPETypePluginChunk);
            free(globalOptimizationPluginChunk);
            free(globalTransformPluginChunk);
            free(globalMutexPluginChunk);

            // WhitePoint defaults
            free(D50XYZ);
        }
    }

    static Lcms2()
    {
        #region Context and plugins

        var defaultTag = default(TagLinkedList);
        var tagNextOffset = (nuint)(&defaultTag.Next) - (nuint)(&defaultTag);

        var defaultTagType = default(TagTypeLinkedList);
        var tagTypeNextOffset = (nuint)(&defaultTagType.Next) - (nuint)(&defaultTagType);

        // Error logger
        fixed (LogErrorChunkType* plugin = &LogErrorChunk)
            globalLogErrorChunk = dup<LogErrorChunkType>(plugin);

        // Alarm Codes
        fixed (AlarmCodesChunkType* plugin = &AlarmCodesChunk)
        {
            plugin->AlarmCodes[0] = plugin->AlarmCodes[1] = plugin->AlarmCodes[2] = 0x7F00;

            globalAlarmCodesChunk = dup<AlarmCodesChunkType>(plugin);
        }

        // Adaptation State
        fixed (AdaptationStateChunkType* plugin = &AdaptationStateChunk)
            globalAdaptationStateChunk = dup<AdaptationStateChunkType>(plugin);

        // Memory Handler
        globalMemPluginChunk = alloc<MemPluginChunkType>();
        *globalMemPluginChunk = new()
        {
            MallocPtr = &_cmsMallocDefaultFn,
            MallocZeroPtr = &_cmsMallocZeroDefaultFn,
            FreePtr = &_cmsFreeDefaultFn,
            ReallocPtr = &_cmsReallocDefaultFn,
            CallocPtr = &_cmsCallocDefaultFn,
            DupPtr = &_cmsDupDefaultFn
        };

        // Interpolation Plugin
        fixed (InterpPluginChunkType* plugin = &InterpPluginChunk)
            globalInterpPluginChunk = dup<InterpPluginChunkType>(plugin);

        // Curves Plugin
        fixed (ParametricCurvesCollection* curves = &defaultCurves)
        {
            fixed (int* defaultFunctionTypes = defaultCurvesFunctionTypes)
                memcpy(curves->FunctionTypes, defaultFunctionTypes, 10 * sizeof(int));
            fixed (uint* defaultParameterCount = defaultCurvesParameterCounts)
                memcpy(curves->ParameterCount, defaultParameterCount, 10 * sizeof(uint));
        }
        fixed (CurvesPluginChunkType* plugin = &CurvesPluginChunk)
            globalCurvePluginChunk = dup<CurvesPluginChunkType>(plugin);

        // Formatters Plugin
        globalFormattersPluginChunk = alloc<FormattersPluginChunkType>();
        *globalFormattersPluginChunk = new();

        // Tag Type Plugin
        ReadOnlySpan<TagTypeLinkedList> tagTypes = new TagTypeLinkedList[]
        {
            new(new(Signature.TagType.Chromaticity, null, null, null, null, null, 0)),
        };
        supportedTagTypes = BuildList(tagTypeNextOffset, tagTypes);
        fixed (TagTypePluginChunkType* plugin = &TagTypePluginChunk)
            globalTagTypePluginChunk = dup<TagTypePluginChunkType>(plugin);

        // Tag Plugin
        ReadOnlySpan<TagLinkedList> tags = stackalloc TagLinkedList[]
        {
            new(Signature.Tag.AToB0, new(1, new[] { Signature.TagType.Lut16, Signature.TagType.LutAtoB, Signature.TagType.Lut8 }, null)),
        };
        supportedTags = BuildList(tagNextOffset, tags);
        fixed (TagPluginChunkType* plugin = &TagPluginChunk)
            globalTagPluginChunk = dup<TagPluginChunkType>(plugin);

        // Intents Plugin
        fixed (IntentsPluginChunkType* plugin = &IntentsPluginChunk)
            globalIntentsPluginChunk = dup<IntentsPluginChunkType>(plugin);

        // MPE Type Plugin
        ReadOnlySpan<TagTypeLinkedList> mpeTypes = new TagTypeLinkedList[]
        {
            new(new(Signature.Stage.BAcsElem, null, null, null, null, null, 0)),
            new(new(Signature.Stage.EAcsElem, null, null, null, null, null, 0)),
            new(new(Signature.Stage.CurveSetElem, null, null, null, null, null, 0)),
            new(new(Signature.Stage.MatrixElem, null, null, null, null, null, 0)),
            new(new(Signature.Stage.CLutElem, null, null, null, null, null, 0)),
        };
        supportedMPEtypes = BuildList(tagTypeNextOffset, mpeTypes);
        fixed (TagTypePluginChunkType* plugin = &MPETypePluginChunk)
            globalMPETypePluginChunk = dup<TagTypePluginChunkType>(plugin);

        // Optimization Plugin
        fixed (OptimizationPluginChunkType* plugin = &OptimizationPluginChunk)
            globalOptimizationPluginChunk = dup<OptimizationPluginChunkType>(plugin);

        // Transform Plugin
        fixed (TransformPluginChunkType* plugin = &TransformPluginChunk)
            globalTransformPluginChunk = dup<TransformPluginChunkType>(plugin);

        // Mutex Plugin
        fixed (MutexPluginChunkType* plugin = &MutexChunk)
            globalMutexPluginChunk = dup<MutexPluginChunkType>(plugin);

        // Global Context
        globalContext = (Context*)NativeMemory.Alloc((nuint)sizeof(Context));
        *globalContext = new()
        {
            Next = null,
            MemPool = null,
            DefaultMemoryManager = default,
        };
        globalContext->chunks.parent = globalContext;

        globalContext->chunks[Chunks.UserPtr] = null;
        globalContext->chunks[Chunks.Logger] = globalLogErrorChunk;
        globalContext->chunks[Chunks.AlarmCodesContext] = globalAlarmCodesChunk;
        globalContext->chunks[Chunks.AdaptationStateContext] = globalAdaptationStateChunk;
        globalContext->chunks[Chunks.MemPlugin] = globalMemPluginChunk;
        globalContext->chunks[Chunks.InterpPlugin] = globalInterpPluginChunk;
        globalContext->chunks[Chunks.CurvesPlugin] = globalCurvePluginChunk;
        globalContext->chunks[Chunks.FormattersPlugin] = globalFormattersPluginChunk;
        globalContext->chunks[Chunks.TagTypePlugin] = globalTagTypePluginChunk;
        globalContext->chunks[Chunks.TagPlugin] = globalTagPluginChunk;
        globalContext->chunks[Chunks.IntentPlugin] = globalIntentsPluginChunk;
        globalContext->chunks[Chunks.MPEPlugin] = globalMPETypePluginChunk;
        globalContext->chunks[Chunks.OptimizationPlugin] = globalOptimizationPluginChunk;
        globalContext->chunks[Chunks.TransformPlugin] = globalTransformPluginChunk;
        globalContext->chunks[Chunks.MutexPlugin] = globalMutexPluginChunk;

        static T* BuildList<T>(nuint nextOffset, ReadOnlySpan<T> list) where T : struct
        {
            var head = calloc<T>((uint)list.Length);
            var ptr = head;
            var ptrNext = (T**)((byte*)ptr + nextOffset);

            foreach (var entry in list)
            {
                *ptr = entry;
                *ptrNext = ++ptr;
            }
            *ptrNext = null;

            return head;
        }

        #endregion Context and plugins

        #region WhitePoint defaults

        D50XYZ = alloc<CIEXYZ>();
        *D50XYZ = new() { X = cmsD50X, Y = cmsD50Y, Z = cmsD50Z };

        #endregion WhitePoint defaults
    }

    internal static void* alloc(nuint size) =>
        NativeMemory.Alloc(size);

    internal static void* alloc(nint size) =>
        NativeMemory.Alloc((nuint)size);

    internal static T* alloc<T>() where T : struct =>
        (T*)alloc(sizeof(T));

    internal static void* dup(in void* org, nint size) =>
        dup(org, (nuint)size);

    internal static void* dup(in void* org, nuint size)
    {
        var value = alloc(size);
        memcpy(value, org, size);

        return value;
    }

    internal static T* dup<T>(in void* org) where T : struct
    {
        var value = alloc<T>();
        memcpy(value, org, (nuint)sizeof(T));

        return value;
    }

    internal static void memset<T>(T* dst, int val) where T : struct =>
        memset(dst, val, sizeof(T));

    internal static void memset(void* dst, int val, nint size) =>
        NativeMemory.Fill(dst, (uint)size, (byte)val);

    internal static void memset(void* dst, int val, nuint size) =>
        NativeMemory.Fill(dst, size, (byte)val);

    internal static void memmove<T>(T* dst, in T* src) where T : struct =>
        memcpy(dst, src);

    internal static void memmove(void* dst, in void* src, nuint size) =>
        memcpy(dst, src, size);

    internal static void memmove(void* dst, in void* src, nint size) =>
        memcpy(dst, src, size);

    internal static void memcpy<T>(T* dst, in T* src) where T : struct =>
        memcpy(dst, src, sizeof(T));

    internal static void memcpy(void* dst, in void* src, nuint size) =>
        NativeMemory.Copy(src, dst, size);

    internal static void memcpy(void* dst, in void* src, nint size) =>
        NativeMemory.Copy(src, dst, (nuint)size);

    internal static void free(void* ptr) =>
        NativeMemory.Free(ptr);

    internal static void* calloc(uint num, nuint size) =>
        NativeMemory.AllocZeroed(num, size);

    internal static void* calloc(uint num, nint size) =>
        NativeMemory.AllocZeroed(num, (nuint)size);

    internal static T* calloc<T>(uint num) where T : struct =>
        (T*)calloc(num, sizeof(T));
}
