﻿//---------------------------------------------------------------------------------
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

    internal const int typesInLcmsPlugin = 20;

    #endregion Fields

    public static Stream cmsOpenIOhandlerFromNULL() =>
        new NullStream();
}

public delegate object? DupUserDataFn(object? state, in object? data);
