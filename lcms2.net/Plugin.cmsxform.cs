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

namespace lcms2;
public static partial class Plugin
{
    public static void _cmsSetTransformUserData(Transform CMMcargo, object? ptr, FreeUserDataFn? FreePrivateDataFn)
    {
        _cmsAssert(CMMcargo);
        CMMcargo.UserData = ptr;
        CMMcargo.FreeUserData = FreePrivateDataFn;
    }

    public static object? _cmsGetTransformUserData(Transform CMMcargo)
    {
        _cmsAssert(CMMcargo);
        return CMMcargo.UserData;
    }

    public static void _cmsGetTransformFormatters16(
        Transform CMMcargo,
        out Formatter16In FromInput,
        out Formatter16Out ToOutput)
    {
        _cmsAssert(CMMcargo);
        FromInput = CMMcargo.FromInput;
        ToOutput = CMMcargo.ToOutput;
    }

    public static void _cmsGetTransformFormattersFloat(
        Transform CMMcargo,
        out FormatterFloatIn FromInput,
        out FormatterFloatOut ToOutput)
    {
        _cmsAssert(CMMcargo);
        FromInput = CMMcargo.FromInputFloat;
        ToOutput = CMMcargo.ToOutputFloat;
    }

    public static uint _cmsGetTransformFlags(Transform CMMcargo)
    {
        _cmsAssert(CMMcargo);
        return CMMcargo.dwOriginalFlags;
    }

    public static Transform2Fn? _cmsGetTransformWorker(Transform CMMcargo)
    {
        _cmsAssert(CMMcargo);
        return CMMcargo.Worker;
    }

    public static int _cmsGetTransformMaxWorkers(Transform CMMcargo)
    {
        _cmsAssert(CMMcargo);
        return CMMcargo.MaxWorkers;
    }

    public static uint _cmsGetTransformWorkerFlags(Transform CMMcargo)
    {
        _cmsAssert(CMMcargo);
        return CMMcargo.WorkerFlags;
    }
}
