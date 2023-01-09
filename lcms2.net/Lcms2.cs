//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022      Stefan Kewatt
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

namespace lcms2;

public delegate void FreeUserDataFn(object? state, ref object data);

public static class Lcms2
{
    #region Fields

    public const int MaxPath = 256;
    public const int MaxTypesInPlugin = 20;
    public const int Version = 2131;
    public static readonly XYZ D50 = (0.9642, 1.0, 0.8249);

    public static readonly XYZ PerceptualBlack = (0.00336, 0.0034731, 0.00287);

    public const uint MagicNumber = 0x61637370;
    public const uint LcmsSignature = 0x6C636D73;

    internal const int maxTableTag = 100;
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

    public static Stream? cmsOpenIOhandlerFromFile(object? state, string fileName, IOHandler.AccessMode accessMode)
    {
        Stream? fm = null;
        try
        {
            fm = accessMode switch
            {
                IOHandler.AccessMode.Read => File.Open(fileName, FileMode.Open),
                IOHandler.AccessMode.Write => File.Open(fileName, FileMode.Create),
                _ => throw new Exception(),
            };
        }
        catch (Exception)
        {
            State.SignalError(state, ErrorCode.File, accessMode switch
            {
                IOHandler.AccessMode.Read => $"Could not open file '{fileName}'",
                IOHandler.AccessMode.Write => $"Could not create file '{fileName}'",
                _ => $"Unknown access mode '{accessMode}'"
            });
        }

        return fm;
    }

    public static Stream? cmsOpenIOhandlerFromMem(object? state, byte[] buffer, int size, IOHandler.AccessMode accessMode)
    {
        switch (accessMode)
        {
            case IOHandler.AccessMode.Read:
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

            case IOHandler.AccessMode.Write:
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
}

public delegate object? DupUserDataFn(object? state, in object? data);
