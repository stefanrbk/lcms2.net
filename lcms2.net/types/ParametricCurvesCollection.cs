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

namespace lcms2.types;

internal unsafe struct ParametricCurvesCollection : INextOffset
{
    public uint nFunctions;
    public fixed int FunctionTypes[MAX_TYPES_IN_LCMS_PLUGIN];
    public fixed uint ParameterCount[MAX_TYPES_IN_LCMS_PLUGIN];
    public delegate*<int, in double*, double, double> Evaluator;

    public ParametricCurvesCollection* Next;

    public static nuint NextOffset { get; } = (nuint)(sizeof(uint) + (sizeof(int) * MAX_TYPES_IN_LCMS_PLUGIN) + (sizeof(uint) * MAX_TYPES_IN_LCMS_PLUGIN) + sizeof(nuint));
}
