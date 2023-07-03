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
        var Buffer = stackalloc byte[256];
        var Buffer2 = stackalloc byte[256];
        var rc = true;

        // Allocate a MLU structure, no preferred size
        var mlu = cmsMLUalloc(DbgThread(), 0);

        // Add some localizations
        const string enHello = "Hello, world";
        const string esHello = "Hola, mundo";
        const string frHello = "Bonjour, le monde";
        const string caHello = "Hola, món";

        var enLang = stackalloc byte[3] { (byte)'e', (byte)'n', 0 };
        var esLang = stackalloc byte[3] { (byte)'e', (byte)'s', 0 };
        var frLang = stackalloc byte[3] { (byte)'f', (byte)'r', 0 };
        var caLang = stackalloc byte[3] { (byte)'c', (byte)'a', 0 };

        var us = stackalloc byte[3] { (byte)'U', (byte)'S', 0 };
        var es = stackalloc byte[3] { (byte)'E', (byte)'S', 0 };
        var fr = stackalloc byte[3] { (byte)'F', (byte)'R', 0 };
        var ca = stackalloc byte[3] { (byte)'C', (byte)'A', 0 };

        cmsMLUsetWide(mlu, enLang, us, enHello);
        cmsMLUsetWide(mlu, esLang, es, esHello);
        cmsMLUsetWide(mlu, frLang, fr, frHello);
        cmsMLUsetWide(mlu, caLang, ca, caHello);

        // Check the returned string for each language

        cmsMLUgetASCII(mlu, enLang, us, Buffer, 256);
        if (strcmp(Buffer, Encoding.ASCII.GetBytes(enHello)) is not 0)
        {
            Fail($"Unexpected string '{new string((sbyte*)Buffer)}'");
            rc = false;
        }

        cmsMLUgetASCII(mlu, esLang, es, Buffer, 256);
        if (strcmp(Buffer, Encoding.ASCII.GetBytes(esHello)) is not 0)
        {
            Fail($"Unexpected string '{new string((sbyte*)Buffer)}'");
            rc = false;
        }

        cmsMLUgetASCII(mlu, frLang, fr, Buffer, 256);
        if (strcmp(Buffer, Encoding.ASCII.GetBytes(frHello)) is not 0)
        {
            Fail($"Unexpected string '{new string((sbyte*)Buffer)}'");
            rc = false;
        }

        //cmsMLUgetASCII(mlu, caLang, ca, Buffer, 256);
        //if (strcmp(Buffer, (byte*)&caHello) is not 0)
        //{
        //    Fail($"Unexpected string '{new string((sbyte*)Buffer)}'");
        //    rc = false;
        //}

        // So far, so good.
        cmsMLUfree(mlu);

        // Now for performance, allocate an empty struct
        mlu = cmsMLUalloc(DbgThread(), 0);

        // Fill it with several thousands of different languages
        var Lang = stackalloc byte[3];
        for (var i = 0; i < 4096; i++)
        {
            Lang[0] = (byte)(i % 255);
            Lang[1] = (byte)(i / 255);
            Lang[2] = 0;

            sprintf(Buffer, $"String #{i}");
            cmsMLUsetASCII(mlu, Lang, Lang, Buffer);
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
            Lang[2] = 0;

            cmsMLUgetASCII(mlu2, Lang, Lang, Buffer2, 256);
            sprintf(Buffer, $"String #{i}");

            if (strcmp(Buffer, Buffer2) is not 0) { rc = false; break; }
        }

        if (!rc)
            Fail($"Unexpected string '{new string((sbyte*)Buffer2)}'");

        // Check profile IO

        var h = cmsOpenProfileFromFileTHR(DbgThread(), "mlucheck.icc", "w");

        cmsSetProfileVersion(h, 4.3);

        cmsWriteTag(h, cmsSigProfileDescriptionTag, mlu2);
        cmsCloseProfile(h); h = null;
        cmsMLUfree(mlu2);

        h = cmsOpenProfileFromFileTHR(DbgThread(), "mlucheck.icc", "r");

        var mlu3 = (Mlu*)cmsReadTag(h, cmsSigProfileDescriptionTag);
        if (mlu3 is null) { Fail("Profile didn't get the MLU\n"); rc = false; goto Error; }

        // Check all is still in place
        for (var i = 0; i < 4096; i++)
        {
            Lang[0] = (byte)(i % 255);
            Lang[1] = (byte)(i / 255);
            Lang[2] = 0;

            cmsMLUgetASCII(mlu3, Lang, Lang, Buffer2, 256);
            sprintf(Buffer, $"String #{i}");

            if (strcmp(Buffer, Buffer2) is not 0) { rc = false; break; }
        }

        if (!rc)
            Fail($"Unexpected string '{new string((sbyte*)Buffer2)}', looking for '{new string((sbyte*)Buffer)}'");

        Error:

        if (h is not null) cmsCloseProfile(h);
        File.Delete("mlucheck.icc");

        return rc;
    }
}
