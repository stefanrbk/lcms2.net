﻿//---------------------------------------------------------------------------------
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
//using lcms2.old.types;

//namespace lcms2.testbed;

//public static class ColorspaceTests
//{
//    #region Public Methods

//    public static bool CheckLab2LCh()
//    {
//        for (var l = 0; l < 100; l += 10)
//        {
//            for (var a = -128; a < 128; a += 8)
//            {
//                for (var b = -128; b < 128; b += 8)
//                {
//                    var lab = new Lab(l, a, b);
//                    var lch = (LCh)lab;
//                    var lab2 = (Lab)lch;

//                    var dist = DeltaE(lab, lab2);
//                    if (dist >= 1E-12)
//                        return Fail($"({l},{a},{b}): Difference outside tolerence. Was {dist}");
//                }
//            }
//        }

//        return true;
//    }

//    public static bool CheckLab2xyY()
//    {
//        for (var l = 0; l < 100; l += 10)
//        {
//            for (var a = -128; a < 128; a += 8)
//            {
//                for (var b = -128; b < 128; b += 8)
//                {
//                    var lab = new Lab(l, a, b);
//                    var xyz = (XYZ)lab;
//                    var xyy = (xyY)xyz;
//                    xyz = (XYZ)xyy;
//                    var lab2 = (Lab)xyz;

//                    var dist = DeltaE(lab, lab2);
//                    if (dist >= 1E-12)
//                        return Fail($"({l},{a},{b}): Difference outside tolerence. Was {dist}");
//                }
//            }
//        }

//        return true;
//    }

//    public static bool CheckLab2XYZ()
//    {
//        for (var l = 0; l < 100; l += 10)
//        {
//            for (var a = -128; a < 128; a += 8)
//            {
//                for (var b = -128; b < 128; b += 8)
//                {
//                    var lab = new Lab(l, a, b);
//                    var xyz = (XYZ)lab;
//                    var lab2 = (Lab)xyz;

//                    var dist = DeltaE(lab, lab2);
//                    if (dist >= 1E-12)
//                        return Fail($"({l},{a},{b}): Difference outside tolerence. Was {dist}");
//                }
//            }
//        }

//        return true;
//    }

//    public static bool CheckLabV2EncodingTest()
//    {
//        for (var j = 0; j < 65535; j++)
//        {
//            var aw1 = new LabEncodedV2((ushort)j, (ushort)j, (ushort)j);
//            var lab = (Lab)aw1;
//            var aw2 = (LabEncodedV2)lab;

//            if (aw1.L != aw2.L ||
//                aw1.a != aw2.a ||
//                aw1.b != aw2.b)
//            {
//                return Fail($"({j},{j},{j}): Was ({aw2.L},{aw2.a},{aw2.b})");
//            }
//        }

//        return true;
//    }

//    public static bool CheckLabV4EncodingTest()
//    {
//        for (var j = 0; j < 65535; j++)
//        {
//            var aw1 = new LabEncoded((ushort)j, (ushort)j, (ushort)j);
//            var lab = (Lab)aw1;
//            var aw2 = (LabEncoded)lab;

//            if (aw1.L != aw2.L ||
//                aw1.a != aw2.a ||
//                aw1.b != aw2.b)
//            {
//                return Fail($"({j},{j},{j}): Was ({aw2.L},{aw2.a},{aw2.b})");
//            }
//        }

//        return true;
//    }

//    #endregion Public Methods
//}
