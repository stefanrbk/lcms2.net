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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace lcms2;

public delegate void FreeUserDataFn(object? state, ref object data);

public static partial class Lcms2
{
    #region Fields

    internal const double maxEncodableAb2 = (65535.0 / 256.0) - 128.0;
    internal const double maxEncodableAb4 = 127.0;
    internal const double maxEncodableXYZ = 1 + (32767.0 / 32768.0);
    internal const double minEncodableAb2 = -128.0;
    internal const double minEncodableAb4 = -128.0;
    internal const int maxStageChannels = 128;
    internal const double determinantTolerance = 0.0001;
    internal const int maxTableTag = 100;
    internal const uint flagsCanChangeFormatter = 0x02000000;

    public const int MaxPath = 256;
    public const int MaxTypesInPlugin = 20;
    public const int Version = 2131;
    public static readonly XYZ D50 = (0.9642, 1.0, 0.8249);

    public static readonly XYZ PerceptualBlack = (0.00336, 0.0034731, 0.00287);

    public const uint MagicNumber = 0x61637370;
    public const uint LcmsSignature = 0x6C636D73;

    internal const int typesInLcmsPlugin = 20;

    #endregion Fields

    #region Public Methods

    public static void cmsCloseIOhandler(Stream io) =>
        io.Close();

    public static IccProfile cmsCreateProfilePlaceholder(object? state) =>
        new(state);

    public static object? cmsGetProfileContextID(IccProfile profile) =>
        profile.ContextId;

    public static Stream? cmsGetProfileIOhandler(IccProfile profile) =>
        profile.IOhandler;

    public static int cmsGetTagCount(IccProfile profile) =>
            (int)profile.TagCount;

    public static Signature cmsGetTagSignature(IccProfile profile, int n) =>
        profile.GetTagName(n);

    public static Stream? cmsOpenIOhandlerFromFile(object? state, string fileName, IOHandlerHelpers.AccessMode accessMode)
    {
        Stream? fm = null;
        try
        {
            fm = accessMode switch
            {
                IOHandlerHelpers.AccessMode.Read => File.Open(fileName, FileMode.Open),
                IOHandlerHelpers.AccessMode.Write => File.Open(fileName, FileMode.Create),
                _ => throw new Exception(),
            };
        }
        catch (Exception)
        {
            State.SignalError(state, ErrorCode.File, accessMode switch
            {
                IOHandlerHelpers.AccessMode.Read => $"Could not open file '{fileName}'",
                IOHandlerHelpers.AccessMode.Write => $"Could not create file '{fileName}'",
                _ => $"Unknown access mode '{accessMode}'"
            });
        }

        return fm;
    }

    public static Stream? cmsOpenIOhandlerFromMem(object? state, byte[] buffer, int size, IOHandlerHelpers.AccessMode accessMode)
    {
        switch (accessMode)
        {
            case IOHandlerHelpers.AccessMode.Read:
                if (size < 0)
                {
                    State.SignalError(state, ErrorCode.Seek, "Cannot read a file of negative size.");
                    return null;
                }

                var fm = new MemoryStream(size);
                fm.Write(buffer, 0, size);
                fm.Flush();
                fm.Seek(0, SeekOrigin.Begin);

                return fm;

            case IOHandlerHelpers.AccessMode.Write:
                if (size < 0)
                {
                    State.SignalError(state, ErrorCode.Seek, "Cannot create IO with a negative size.");
                    return null;
                }

                return new MemoryStream(buffer, 0, size, true);

            default:
                State.SignalError(state, ErrorCode.UnknownExtension, $"Unknown access mode '{accessMode}'");
                return null;
        }
    }

    public static Stream cmsOpenIOhandlerFromNULL() =>
        new NullStream();

    public static uint cmsGetHeaderRenderingIntent(IccProfile profile) =>
        profile.RenderingIntent;

    public static void cmsSetHeaderRenderingIntent(IccProfile profile, uint renderingIntent) =>
        profile.RenderingIntent = renderingIntent;

    public static uint cmsGetHeaderCreator(IccProfile profile) =>
        profile.Creator;

    public static uint cmsGetHeaderModel(IccProfile profile) =>
        profile.Model;

    public static void cmsSetHeaderModel(IccProfile profile, uint model) =>
        profile.Model = model;

    public static ulong cmsGetHeaderAttributes(IccProfile profile) =>
        profile.Attributes;

    public static void cmsSetHeaderAttributes(IccProfile profile, ulong attributes) =>
        profile.Attributes = attributes;

    public static ProfileID cmsGetHeaderProfileID(IccProfile profile) =>
        profile.ProfileID;

    public static void cmsSetHeaderProfileID(IccProfile profile, ProfileID id) =>
        profile.ProfileID = id;

    public static DateTime cmsGetHeaderCreationDateTime(IccProfile profile) =>
        profile.Created;

    public static Signature cmsGetPCS(IccProfile profile) =>
        profile.PCS;

    public static void cmsSetPCS(IccProfile profile, Signature pcs) =>
        profile.PCS = pcs;

    public static Signature cmsGetColorSpace(IccProfile profile) =>
        profile.ColorSpace;

    public static void cmsSetColorSpace(IccProfile profile, Signature colorSpace) =>
        profile.ColorSpace = colorSpace;

    public static Signature cmsGetDeviceClass(IccProfile profile) =>
        profile.DeviceClass;

    public static void cmsSetDeviceClass(IccProfile profile, Signature deviceClass) =>
        profile.DeviceClass = deviceClass;

    public static uint cmsGetEncodedICCversion(IccProfile profile) =>
        profile.EncodedVersion;

    public static void cmsSetEncodedICCversion(IccProfile profile, uint version) =>
        profile.EncodedVersion = version;

    public static double cmsGetProfileVersion(IccProfile profile) =>
        profile.Version;

    public static void cmsSetProfileVersion(IccProfile profile, double version) =>
        profile.Version = version;

    #endregion Public Methods

    /// <summary>
    ///     Fast floor conversion
    /// </summary>
    /// <remarks>
    ///     Thanks to Sree Kotay and Stuart Nixon<br/>
    ///     Note: this only works in the range ..-32767...+32767 because
    ///     the mantissa is interpreted as 15.16 fixed point.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ExcludeFromCodeCoverage]
    internal static unsafe int _cmsQuickFloor(double val)
    {
#if DONT_USE_FAST_FLOOR
        return (int)Math.Floor(val);
#else
        QuickFloorTemp temp;
        const double magic = 68719476736.0 * 1.5;

        temp.val = val + magic;

        return BitConverter.IsLittleEndian ?
            temp.halves[0] >> 16 :
            temp.halves[1] >> 16;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long _cmsAlignLong(long x) =>
        (x + (sizeof(uint) - 1)) & ~(sizeof(uint) - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe long _cmsAlignMem(long x) =>
        (x + (sizeof(void*) - 1)) & ~(sizeof(void*) - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort From8to16(byte rgb) =>
        (ushort)((rgb << 8) | rgb);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static byte From16to8(ushort rgb) =>
        (byte)((((rgb * (uint)65281) + 8388608) >> 24) & 0xFF);

    [StructLayout(LayoutKind.Explicit)]
    private unsafe struct QuickFloorTemp
    {
        [FieldOffset(0)]
        public double val;

        [FieldOffset(0)]
        public fixed int halves[2];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort _cmsQuickFloorWord(double d) =>
        (ushort)(_cmsQuickFloor(d - 32767.0) + 32767);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort _cmsQuickSaturateWord(double d) =>
        (d += 0.5) switch
        {
            <= 0 => 0,
            >= 65535.0 => 0xFFFF,
            _ => _cmsQuickFloorWord(d),
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int FixedRestToInt(int x) =>
        x & 0xFFFF;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int FixedToInt(int x) =>
        x >> 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int RoundFixedToInt(int x) =>
        (x + 0x8000) >> 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int _cmsFromFixedDomain(int a) =>
        a - ((a + 0x7FFF) >> 16);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int _cmsToFixedDomain(int a) =>
        a + ((a + 0x7FFF) / 0xFFFF);
}

public delegate object? DupUserDataFn(object? state, in object? data);
