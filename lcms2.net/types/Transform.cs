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

using System.Diagnostics.CodeAnalysis;

namespace lcms2.types;

public unsafe struct Transform
{
    public uint InputFormat, OutputFormat;

    public Transform2Fn xform;

    public Formatter16 FromInput;
    public Formatter16 ToOutput;

    public FormatterFloat FromInputFloat;
    public FormatterFloat ToOutputFloat;

    public Cache Cache;

    public Pipeline? Lut;

    public Pipeline? GamutCheck;

    public NamedColorList* InputColorant;
    public NamedColorList* OutputColorant;

    public Signature EntryColorSpace;
    public Signature ExitColorSpace;

    public CIEXYZ EntryWhitePoint;
    public CIEXYZ ExitWhitePoint;

    public Sequence* Sequence;

    public uint dwOriginalFlags;
    public double AdaptationState;

    public uint RenderingIntent;

    public Context? ContextID;

    public void* UserData;
    public FreeUserDataFn? FreeUserData;

    public TransformFn? OldXform;
}
