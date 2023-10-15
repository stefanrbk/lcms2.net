//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2023 Marti Maria Saguer
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

namespace lcms2;
public static partial class Lcms2
{
    // Moved all others to Plugin.cmsmd5.cs
    public static bool cmsMD5computeID(Profile Profile)
    {
        Profile Icc;
        byte[]? Mem = null;
        var Keep = Profile;

        _cmsAssert(Profile);

        var ContextID = cmsGetProfileContextID(Profile);

        // Save a copy of the profile header
        Icc = (Profile)Keep.Clone();
        //memmove(&Keep, Icc);

        // Set RI, attributes and ID
        Icc.attributes = 0;
        Icc.RenderingIntent = 0;
        Icc.ProfileID = default;

        // Compute needed storage
        uint BytesNeeded;
        if (!cmsSaveProfileToMem(Profile, null, out BytesNeeded)) goto Error;

        // Allocate memory
        var pool = _cmsGetContext(ContextID).GetBufferPool<byte>();
        Mem = pool.Rent((int)BytesNeeded);
        //if (Mem is null) goto Error;

        // Save to temporary storage
        if (!cmsSaveProfileToMem(Profile, Mem, out BytesNeeded)) goto Error;

        // Create MD5 object
        var MD5 = cmsMD5alloc(ContextID);
        //if (MD5 is null) goto Error;

        // Add all bytes
        cmsMD5add(ref MD5, Mem, BytesNeeded);

        // Temp storage is no longer needed
        ReturnArray(ContextID, Mem);

        // Restore header
        //memmove(Icc, &Keep);

        // And store the ID
        Icc.ProfileID = cmsMD5finish(MD5);

        return true;

    Error:
        // Free resources as something went wrong
        // "MD5" cannot be other than null here, so no need to free it
        if (Mem is not null) ReturnArray(ContextID, Mem);
        //memmove(Icc, &Keep);
        return false;
    }
}
