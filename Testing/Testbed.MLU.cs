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
        var enHello = "Hello, world";
        var esHello = "Hola, mundo";
        var frHello = "Bonjour, le monde";
        var caHello = "Hola, món";

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
        cmsCloseProfile(h);
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
}
