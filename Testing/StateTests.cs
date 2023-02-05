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
using lcms2.state;

namespace lcms2.testbed;

public static class StateTests
{
    #region Public Methods

    public static bool TestAdaptationStateState()
    {
        var rc = false;
        var old1 = cmsSetAdaptationStateTHR(null, -1);

        var c1 = cmsCreateContext(null, null);

        cmsSetAdaptationStateTHR(c1, 0.7);

        var c2 = cmsDupContext(c1, null);
        var c3 = cmsDupContext(c2, null);

        rc = IsGoodVal("Adaption state", cmsSetAdaptationStateTHR(c3, -1), 0.7, 0.001);

        cmsDeleteContext(c1);
        cmsDeleteContext(c2);
        cmsDeleteContext(c3);

        var old2 = cmsSetAdaptationStateTHR(null, -1);

        if (old1 != old2)
            return Fail("Adaptation state has changed");

        return rc;
    }

    public static unsafe bool TestAlarmCodes()
    {
        var codes = stackalloc ushort[16] { 0x0000, 0x1111, 0x2222, 0x3333, 0x4444, 0x5555, 0x6666, 0x7777, 0x8888, 0x9999, 0xaaaa, 0xbbbb, 0xcccc, 0xdddd, 0xeeee, 0xffff };
        var values = stackalloc uint[16];

        var c1 = cmsCreateContext(null, null);

        cmsSetAlarmCodesTHR(c1, new(codes, 16));
        var c2 = cmsDupContext(c1, null);
        var c3 = cmsDupContext(c2, null);

        cmsGetAlarmCodesTHR(c3, new(values, 16));

        for (var i = 0; i < 16; i++)
        {
            if (values[i] != codes[i])
                return Fail($"Bad alarm code #{i}: {values[i]} != {codes[i]}");
        }

        return true;
    }

    public static bool TestSimpleState()
    {
        object a = 1;
        object b = 32;
        var rc = false;

        var c1 = cmsCreateContext(null, a);

        // Let's check duplication
        var c2 = cmsDupContext(c1, null);
        var c3 = cmsDupContext(c1, null);

        // User data should have been propagated
        rc = State.GetUserData(c3) == a;

        cmsDeleteContext(c1);
        cmsDeleteContext(c2);
        cmsDeleteContext(c3);

        if (!rc)
            return Fail("Creation of user data failed");

        c1 = cmsCreateContext(null, a);

        c2 = cmsDupContext(c1, null);
        c3 = cmsDupContext(c2, b);

        // New user data should be applied to c3
        rc = State.GetUserData(c1) == a &&
             State.GetUserData(c2) == a &&
             State.GetUserData(c3) == b;

        cmsDeleteContext(c1);
        cmsDeleteContext(c2);
        cmsDeleteContext(c3);

        if (!rc)
            Fail("Modification of user data failed");

        return true;
    }

    #endregion Public Methods
}
