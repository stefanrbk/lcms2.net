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
using lcms2.types;

using System.Text;

namespace lcms2.testbed;

internal static unsafe partial class Testbed
{
    public static bool CheckMLU()
    {
        Span<byte> Buffer = stackalloc byte[256];
        Span<byte> Buffer2 = stackalloc byte[256];
        var rc = true;

        // Allocate a MLU structure, no preferred size
        var mlu = cmsMLUalloc(DbgThread(), 0);

        // Add some localizations
        const string enHello = "Hello, world";
        const string esHello = "Hola, mundo";
        const string frHello = "Bonjour, le monde";
        const string caHello = "Hola, món";

        var enLang = "en"u8;
        var esLang = "es"u8;
        var frLang = "fr"u8;
        var caLang = "ca"u8;

        var us = "US"u8;
        var es = "ES"u8;
        var fr = "FR"u8;
        var ca = "CA"u8;

        cmsMLUsetWide(mlu, enLang, us, enHello);
        cmsMLUsetWide(mlu, esLang, es, esHello);
        cmsMLUsetWide(mlu, frLang, fr, frHello);
        cmsMLUsetWide(mlu, caLang, ca, caHello);

        // Check the returned string for each language

        cmsMLUgetASCII(mlu, enLang, us, Buffer);
        if (strcmp(Buffer[..Encoding.ASCII.GetByteCount(enHello)], Encoding.ASCII.GetBytes(enHello)) is not 0)
        {
            Fail($"Unexpected string '{enHello}' but found '{Encoding.ASCII.GetString(Buffer)}'");
            rc = false;
        }

        cmsMLUgetASCII(mlu, esLang, es, Buffer);
        if (strcmp(Buffer[..Encoding.ASCII.GetByteCount(esHello)], Encoding.ASCII.GetBytes(esHello)) is not 0)
        {
            Fail($"Unexpected string '{esHello}' but found '{Encoding.ASCII.GetString(Buffer)}'");
            rc = false;
        }

        cmsMLUgetASCII(mlu, frLang, fr, Buffer);
        if (strcmp(Buffer[..Encoding.ASCII.GetByteCount(frHello)], Encoding.ASCII.GetBytes(frHello)) is not 0)
        {
            Fail($"Unexpected string '{frHello}' but found '{Encoding.ASCII.GetString(Buffer)}'");
            rc = false;
        }

        //cmsMLUgetASCII(mlu, caLang, ca, Buffer);
        //if (strcmp(Buffer[..Encoding.ASCII.GetByteCount(caHello)], Encoding.ASCII.GetBytes(caHello)) is not 0)
        //{
        //    Fail($"Unexpected string '{caHello}' but found '{Encoding.ASCII.GetString(Buffer)}'");
        //    rc = false;
        //}

        // So far, so good.
        cmsMLUfree(mlu);

        // Now for performance, allocate an empty struct
        mlu = cmsMLUalloc(DbgThread(), 0);

        // Fill it with several thousands of different languages
        Span<byte> Lang = stackalloc byte[2];
        for (var i = 0; i < 4096; i++)
        {
            Lang[0] = (byte)(i % 255);
            Lang[1] = (byte)(i / 255);

            var tmp = Encoding.ASCII.GetBytes($"String #{i}");
            tmp.AsSpan().CopyTo(Buffer[..tmp.Length]);
            cmsMLUsetASCII(mlu, Lang, Lang, Buffer[..tmp.Length]);
        }

        // Duplicate it
        var mlu2 = cmsMLUdup(mlu);

        // Get rid of original
        cmsMLUfree(mlu);

        // Check all is still in place
        for (var i = 0; i < 4096; i++)
        {
            Lang[0] = (byte)(i % 255);
            Lang[1] = (byte)(i / 255);

            cmsMLUgetASCII(mlu2, Lang, Lang, Buffer2);
            var tmp = Encoding.ASCII.GetBytes($"String #{i}");
            tmp.AsSpan().CopyTo(Buffer[..tmp.Length]);

            if (strcmp(Buffer[..tmp.Length], Buffer2[..tmp.Length]) is not 0) { rc = false; break; }
        }

        if (!rc)
            Fail($"Unexpected string '{Encoding.ASCII.GetString(Buffer)}' but found '{Encoding.ASCII.GetString(Buffer2)}'");

        // Check profile IO

        var h = cmsOpenProfileFromFileTHR(DbgThread(), "mlucheck.icc", "w");

        cmsSetProfileVersion(h, 4.3);

        cmsWriteTag(h, cmsSigProfileDescriptionTag, mlu2);
        cmsCloseProfile(h); h = null;
        cmsMLUfree(mlu2);

        h = cmsOpenProfileFromFileTHR(DbgThread(), "mlucheck.icc", "r");

        if (cmsReadTag(h, cmsSigProfileDescriptionTag) is not Mlu mlu3)
        {
            Fail("Profile didn't get the MLU\n");
            rc = false;
            goto Error;
        }

        // Check all is still in place
        for (var i = 0; i < 4096; i++)
        {
            Lang[0] = (byte)(i % 255);
            Lang[1] = (byte)(i / 255);

            cmsMLUgetASCII(mlu3, Lang, Lang, Buffer2);
            var tmp = Encoding.ASCII.GetBytes($"String #{i}");
            tmp.AsSpan().CopyTo(Buffer[..tmp.Length]);

            if (strcmp(Buffer[..tmp.Length], Buffer2[..tmp.Length]) is not 0)
            { rc = false; break; }
        }

        if (!rc)
            Fail($"Unexpected string '{Encoding.ASCII.GetString(Buffer)}' but found '{Encoding.ASCII.GetString(Buffer2)}'");

        Error:

        if (h is not null) cmsCloseProfile(h);
        File.Delete("mlucheck.icc");

        return rc;
    }

    // A lightweight test of named color structures.
    public static bool CheckNamedColorList()
    {
        NamedColorList* nc = null, nc2;
        int i, j;
        bool rc = true;
        var Name = stackalloc byte[cmsMAX_PATH];
        var PCS = stackalloc ushort[3];
        var Colorant = stackalloc ushort[cmsMAXCHANNELS];
        var CheckName = stackalloc byte[cmsMAX_PATH];
        var CheckPCS = stackalloc ushort[3];
        var CheckColorant = stackalloc ushort[cmsMAXCHANNELS];
        HPROFILE h;

        nc = cmsAllocNamedColorList(DbgThread(), 0, 4, "prefix"u8, "suffix"u8);
        if (nc == null) return false;

        for (i = 0; i < 4096; i++)
        {


            PCS[0] = PCS[1] = PCS[2] = (ushort)i;
            Colorant[0] = Colorant[1] = Colorant[2] = Colorant[3] = (ushort)(4096 - i);

            sprintf(Name, "#{0}", i);
            if (!cmsAppendNamedColor(nc, Name, PCS, Colorant)) { rc = false; break; }
        }

        for (i = 0; i < 4096; i++)
        {

            CheckPCS[0] = CheckPCS[1] = CheckPCS[2] = (ushort)i;
            CheckColorant[0] = CheckColorant[1] = CheckColorant[2] = CheckColorant[3] = (ushort)(4096 - i);

            sprintf(CheckName, "#{0}", i);
            if (!cmsNamedColorInfo(nc, (uint)i, Name, null, null, PCS, Colorant)) { rc = false; goto Error; }


            for (j = 0; j < 3; j++)
            {
                if (CheckPCS[j] != PCS[j]) { rc = Fail("Invalid PCS"); goto Error; }
            }

            for (j = 0; j < 4; j++)
            {
                if (CheckColorant[j] != Colorant[j]) { rc = Fail("Invalid Colorant"); goto Error; };
            }

            if (strcmp(Name, CheckName) != 0) { rc = Fail("Invalid Name"); goto Error; };
        }

        h = cmsOpenProfileFromFileTHR(DbgThread(), "namedcol.icc", "w");
        if (h == null) return false;
        if (!cmsWriteTag(h, cmsSigNamedColor2Tag, nc)) return false;
        cmsCloseProfile(h);
        cmsFreeNamedColorList(nc);
        nc = null;

        h = cmsOpenProfileFromFileTHR(DbgThread(), "namedcol.icc", "r");
        nc2 = (NamedColorList*)cmsReadTag(h, cmsSigNamedColor2Tag);

        if (cmsNamedColorCount(nc2) != 4096) { rc = Fail("Invalid count"); goto Error; }

        i = cmsNamedColorIndex(nc2, "#123"u8);
        if (i != 123) { rc = Fail("Invalid index"); goto Error; }


        for (i = 0; i < 4096; i++)
        {

            CheckPCS[0] = CheckPCS[1] = CheckPCS[2] = (ushort)i;
            CheckColorant[0] = CheckColorant[1] = CheckColorant[2] = CheckColorant[3] = (ushort)(4096 - i);

            sprintf(CheckName, "#{0}", i);
            if (!cmsNamedColorInfo(nc2, (uint)i, Name, null, null, PCS, Colorant)) { rc = false; goto Error; }


            for (j = 0; j < 3; j++)
            {
                if (CheckPCS[j] != PCS[j]) { rc = Fail("Invalid PCS"); goto Error; }
            }

            for (j = 0; j < 4; j++)
            {
                if (CheckColorant[j] != Colorant[j]) { rc = Fail("Invalid Colorant"); goto Error; };
            }

            if (strcmp(Name, CheckName) != 0) { rc = Fail("Invalid Name"); goto Error; };
        }

        cmsCloseProfile(h);
        File.Delete("namedcol.icc");

    Error:
        if (nc != null) cmsFreeNamedColorList(nc);
        return rc;
    }



    // For educational purposes ONLY. No error checking is performed!
    public static bool CreateNamedColorProfile()
    {
        // Color list database
        NamedColorList* colors = cmsAllocNamedColorList(null, 10, 4, "PANTONE"u8, "TCX"u8);

        // Containers for names
        Mlu* DescriptionMLU, CopyrightMLU;

        // Create n empty profile
        HPROFILE hProfile = cmsOpenProfileFromFile("named.icc", "w");

        // Values
        CIELab Lab;
        var PCS = stackalloc ushort[3];
        var Colorant = stackalloc ushort[4];

        // Set profile class
        cmsSetProfileVersion(hProfile, 4.3);
        cmsSetDeviceClass(hProfile, cmsSigNamedColorClass);
        cmsSetColorSpace(hProfile, cmsSigCmykData);
        cmsSetPCS(hProfile, cmsSigLabData);
        cmsSetHeaderRenderingIntent(hProfile, INTENT_PERCEPTUAL);

        // Add description and copyright only in english/US
        DescriptionMLU = cmsMLUalloc(null, 1);
        CopyrightMLU = cmsMLUalloc(null, 1);

        cmsMLUsetWide(DescriptionMLU, "en"u8, "US"u8, "Profile description");
        cmsMLUsetWide(CopyrightMLU, "en"u8, "US"u8, "Profile copyright");

        cmsWriteTag(hProfile, cmsSigProfileDescriptionTag, DescriptionMLU);
        cmsWriteTag(hProfile, cmsSigCopyrightTag, CopyrightMLU);

        // Set the media white point
        cmsWriteTag(hProfile, cmsSigMediaWhitePointTag, cmsD50_XYZ());


        // Populate one value, Colorant = CMYK values in 16 bits, PCS[] = Encoded Lab values (in V2 format!!)
        Lab.L = 50; Lab.a = 10; Lab.b = -10;
        cmsFloat2LabEncodedV2(PCS, &Lab);
        Colorant[0] = 10 * 257; Colorant[1] = 20 * 257; Colorant[2] = 30 * 257; Colorant[3] = 40 * 257;
        cmsAppendNamedColor(colors, "Hazelnut 14-1315"u8, PCS, Colorant);

        // Another one. Consider to write a routine for that
        Lab.L = 40; Lab.a = -5; Lab.b = 8;
        cmsFloat2LabEncodedV2(PCS, &Lab);
        Colorant[0] = 10 * 257; Colorant[1] = 20 * 257; Colorant[2] = 30 * 257; Colorant[3] = 40 * 257;
        cmsAppendNamedColor(colors, "Kale 18-0107"u8, PCS, Colorant);

        // Write the colors database
        cmsWriteTag(hProfile, cmsSigNamedColor2Tag, colors);

        // That will create the file
        cmsCloseProfile(hProfile);

        // Free resources
        cmsFreeNamedColorList(colors);
        cmsMLUfree(DescriptionMLU);
        cmsMLUfree(CopyrightMLU);

        File.Delete("named.icc");

        return true;
    }
}
