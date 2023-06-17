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

using lcms2.io;
using lcms2.state;
using lcms2.types;

namespace lcms2;

public static unsafe partial class Lcms2
{
    private static readonly byte* PSBuffer;
    private const byte MAXPSCOLS = 60;
    /*
        Implementation
        --------------

      PostScript does use XYZ as its internal PCS. But since PostScript
      interpolation tables are limited to 8 bits, I use Lab as a way to
      improve the accuracy, favoring perceptual results. So, for the creation
      of each CRD, CSA the profiles are converted to Lab via a device
      link between  profile -> Lab or Lab -> profile. The PS code necessary to
      convert Lab <-> XYZ is also included.



      Color Space Arrays (CSA)
      ==================================================================================

      In order to obtain precision, code chooses between three ways to implement
      the device -> XYZ transform. These cases identifies monochrome profiles (often
      implemented as a set of curves), matrix-shaper and Pipeline-based.

      Monochrome
      -----------

      This is implemented as /CIEBasedA CSA. The prelinearization curve is
      placed into /DecodeA section, and matrix equals to D50. Since here is
      no interpolation tables, I do the conversion directly to XYZ

      NOTE: CLUT-based monochrome profiles are NOT supported. So, cmsFLAGS_MATRIXINPUT
      flag is forced on such profiles.

        [ /CIEBasedA
          <<
                /DecodeA { transfer function } bind
                /MatrixA [D50]
                /RangeLMN [ 0.0 cmsD50X 0.0 cmsD50Y 0.0 cmsD50Z ]
                /WhitePoint [D50]
                /BlackPoint [BP]
                /RenderingIntent (intent)
          >>
        ]

       On simpler profiles, the PCS is already XYZ, so no conversion is required.


       Matrix-shaper based
       -------------------

       This is implemented both with /CIEBasedABC or /CIEBasedDEF depending on the
       profile implementation. Since here there are no interpolation tables, I do
       the conversion directly to XYZ



        [ /CIEBasedABC
                <<
                    /DecodeABC [ {transfer1} {transfer2} {transfer3} ]
                    /MatrixABC [Matrix]
                    /RangeLMN [ 0.0 cmsD50X 0.0 cmsD50Y 0.0 cmsD50Z ]
                    /DecodeLMN [ { / 2} dup dup ]
                    /WhitePoint [D50]
                    /BlackPoint [BP]
                    /RenderingIntent (intent)
                >>
        ]


        CLUT based
        ----------

         Lab is used in such cases.

        [ /CIEBasedDEF
                <<
                /DecodeDEF [ <prelinearization> ]
                /Table [ p p p [<...>]]
                /RangeABC [ 0 1 0 1 0 1]
                /DecodeABC[ <postlinearization> ]
                /RangeLMN [ -0.236 1.254 0 1 -0.635 1.640 ]
                   % -128/500 1+127/500 0 1  -127/200 1+128/200
                /MatrixABC [ 1 1 1 1 0 0 0 0 -1]
                /WhitePoint [D50]
                /BlackPoint [BP]
                /RenderingIntent (intent)
        ]


      Color Rendering Dictionaries (CRD)
      ==================================
      These are always implemented as CLUT, and always are using Lab. Since CRD are expected to
      be used as resources, the code adds the definition as well.

      <<
        /ColorRenderingType 1
        /WhitePoint [ D50 ]
        /BlackPoint [BP]
        /MatrixPQR [ Bradford ]
        /RangePQR [-0.125 1.375 -0.125 1.375 -0.125 1.375 ]
        /TransformPQR [
        {4 index 3 get div 2 index 3 get mul exch pop exch pop exch pop exch pop } bind
        {4 index 4 get div 2 index 4 get mul exch pop exch pop exch pop exch pop } bind
        {4 index 5 get div 2 index 5 get mul exch pop exch pop exch pop exch pop } bind
        ]
        /MatrixABC <...>
        /EncodeABC <...>
        /RangeABC  <.. used for  XYZ -> Lab>
        /EncodeLMN
        /RenderTable [ p p p [<...>]]

        /RenderingIntent (Perceptual)
      >>
      /Current exch /ColorRendering defineresource pop


      The following stages are used to convert from XYZ to Lab
      --------------------------------------------------------

      Input is given at LMN stage on X, Y, Z

      Encode LMN gives us f(X/Xn), f(Y/Yn), f(Z/Zn)

      /EncodeLMN [

        { 0.964200  div dup 0.008856 le {7.787 mul 16 116 div add}{1 3 div exp} ifelse } bind
        { 1.000000  div dup 0.008856 le {7.787 mul 16 116 div add}{1 3 div exp} ifelse } bind
        { 0.824900  div dup 0.008856 le {7.787 mul 16 116 div add}{1 3 div exp} ifelse } bind

        ]


      MatrixABC is used to compute f(Y/Yn), f(X/Xn) - f(Y/Yn), f(Y/Yn) - f(Z/Zn)

      | 0  1  0|
      | 1 -1  0|
      | 0  1 -1|

      /MatrixABC [ 0 1 0 1 -1 1 0 0 -1 ]

     EncodeABC finally gives Lab values.

      /EncodeABC [
        { 116 mul  16 sub 100 div  } bind
        { 500 mul 128 add 255 div  } bind
        { 200 mul 128 add 255 div  } bind
        ]

      The following stages are used to convert Lab to XYZ
      ----------------------------------------------------

        /RangeABC [ 0 1 0 1 0 1]
        /DecodeABC [ { 100 mul 16 add 116 div } bind
                     { 255 mul 128 sub 500 div } bind
                     { 255 mul 128 sub 200 div } bind
                   ]

        /MatrixABC [ 1 1 1 1 0 0 0 0 -1]
        /DecodeLMN [
                    {dup 6 29 div ge {dup dup mul mul} {4 29 div sub 108 841 div mul} ifelse 0.964200 mul} bind
                    {dup 6 29 div ge {dup dup mul mul} {4 29 div sub 108 841 div mul} ifelse } bind
                    {dup 6 29 div ge {dup dup mul mul} {4 29 div sub 108 841 div mul} ifelse 0.824900 mul} bind
                    ]


    */

    /*

     PostScript algorithms discussion.
     =========================================================================================================

      1D interpolation algorithm


      1D interpolation (float)
      ------------------------

        val2 = Domain * Value;

        cell0 = (int) floor(val2);
        cell1 = (int) ceil(val2);

        rest = val2 - cell0;

        y0 = LutTable[cell0] ;
        y1 = LutTable[cell1] ;

        y = y0 + (y1 - y0) * rest;



      PostScript code                   Stack
      ================================================

      {                                 % v
        <check 0..1.0>
        [array]                         % v tab
        dup                             % v tab tab
        length 1 sub                    % v tab dom

        3 -1 roll                       % tab dom v

        mul                             % tab val2
        dup                             % tab val2 val2
        dup                             % tab val2 val2 val2
        floor cvi                       % tab val2 val2 cell0
        exch                            % tab val2 cell0 val2
        ceiling cvi                     % tab val2 cell0 cell1

        3 index                         % tab val2 cell0 cell1 tab
        exch                            % tab val2 cell0 tab cell1
        get                             % tab val2 cell0 y1

        4 -1 roll                       % val2 cell0 y1 tab
        3 -1 roll                       % val2 y1 tab cell0
        get                             % val2 y1 y0

        dup                             % val2 y1 y0 y0
        3 1 roll                        % val2 y0 y1 y0

        sub                             % val2 y0 (y1-y0)
        3 -1 roll                       % y0 (y1-y0) val2
        dup                             % y0 (y1-y0) val2 val2
        floor cvi                       % y0 (y1-y0) val2 floor(val2)
        sub                             % y0 (y1-y0) rest
        mul                             % y0 t1
        add                             % y
        65535 div                       % result

      } bind


    */

    private struct PsSamplerCargo
    {
        public StageCLutData Pipeline;
        public IOHandler m;

        public int FirstComponent;
        public int SecondComponent;

        public byte* PreMaj;
        public byte* PostMaj;
        public byte* PreMin;
        public byte* PostMin;

        public bool FixWhite;        // Force mapping of pure white

        public Signature ColorSpace;    // ColorSpace of profile
    }

    private static int _cmsPSActualColumn = 0;

    private static byte Word2Byte(ushort w) =>
        (byte)Math.Floor((w / 257.0) + 0.5);

    private static void WriteByte(IOHandler m, byte b)
    {
        _cmsIOPrintf(m, "{0:x2}", b);
        _cmsPSActualColumn += 2;

        if (_cmsPSActualColumn > MAXPSCOLS)
        {
            _cmsIOPrintf(m, "\n");
            _cmsPSActualColumn = 0;
        }
    }

    private static byte* RemoveCR(byte* txt)
    {
        strncpy(PSBuffer, txt, 2047);
        PSBuffer[2047] = 0;
        for (var pt = PSBuffer; *pt is not 0; pt++)
        {
            if (*pt is (byte)'\n' or (byte)'\r')
                *pt = (byte)' ';
        }

        return PSBuffer;
    }

    private static string ctime(DateTime timer) =>
        timer.ToString("ddd MMM dd HH:mm:ss yyyy") + '\n';

    private static void EmitHeader(IOHandler m, byte* Title, Profile Profile)
    {
        var DescASCII = stackalloc byte[256];
        var CopyrightASCII =  stackalloc byte[256];

        var timer = DateTime.UtcNow;

        var Description = (BoxPtr<Mlu>?)cmsReadTag(Profile, cmsSigProfileDescriptionTag);
        var Copyright = (BoxPtr<Mlu>?)cmsReadTag(Profile, cmsSigCopyrightTag);

        DescASCII[0] = DescASCII[255] = 0;
        CopyrightASCII[0] = CopyrightASCII[255] = 0;

        if (Description is not null) cmsMLUgetASCII(Description, cmsNoLanguage, cmsNoCountry, DescASCII, 255);
        if (Copyright is not null) cmsMLUgetASCII(Copyright, cmsNoLanguage, cmsNoCountry, CopyrightASCII, 255);

        _cmsIOPrintf(m, "%!PS-Adobe-3.0\n");
        _cmsIOPrintf(m, "%\n");
        _cmsIOPrintf(m, "% {0}\n", new string((sbyte*)Title));
        _cmsIOPrintf(m, "% Source: {0}\n", new string((sbyte*)RemoveCR(DescASCII)));
        _cmsIOPrintf(m, "%         {0}\n", new string((sbyte*)RemoveCR(CopyrightASCII)));
        _cmsIOPrintf(m, "% Created: {0}", ctime(timer));    // ctime appends a \n!!!
        _cmsIOPrintf(m, "%\n");
        _cmsIOPrintf(m, "%%BeginResource\n");
    }

    private static void EmitWhiteBlackD50(IOHandler m, CIEXYZ* BlackPoint)
    {
        _cmsIOPrintf(m, "/BlackPoint [{0} {1} {2}]\n", BlackPoint->X, BlackPoint->Y, BlackPoint->Z);
        _cmsIOPrintf(m, "/WhitePoint [{0} {1} {2}]\n", cmsD50_XYZ()->X, cmsD50_XYZ()->Y, cmsD50_XYZ()->Z);
    }

    private static void EmitRangeCheck(IOHandler m) =>
        _cmsIOPrintf(m, "dup 0.0 lt {{ pop 0.0 }} if dup 1.0 gt {{ pop 1.0 }} if ");

    private static void EmitIntent(IOHandler m, uint RenderingIntent)
    {
        var intent = RenderingIntent switch
        {
            INTENT_PERCEPTUAL => "Perceptual",
            INTENT_RELATIVE_COLORIMETRIC => "RelativeColorimetric",
            INTENT_ABSOLUTE_COLORIMETRIC => "AbsoluteColorimetric",
            INTENT_SATURATION => "Saturation",
            _ => "Undefined"
        };
        _cmsIOPrintf(m, "/RenderingIntent ({0})\n", intent);
    }

    private static void EmitLab2XYZ(IOHandler m)
    {
        _cmsIOPrintf(m, "/RangeABC [ 0 1 0 1 0 1]\n");
        _cmsIOPrintf(m, "/DecodeABC [\n");
        _cmsIOPrintf(m, "{{100 mul  16 add 116 div }} bind\n");
        _cmsIOPrintf(m, "{{255 mul 128 sub 500 div }} bind\n");
        _cmsIOPrintf(m, "{{255 mul 128 sub 200 div }} bind\n");
        _cmsIOPrintf(m, "]\n");
        _cmsIOPrintf(m, "/MatrixABC [ 1 1 1 1 0 0 0 0 -1]\n");
        _cmsIOPrintf(m, "/RangeLMN [ -0.236 1.254 0 1 -0.635 1.640 ]\n");
        _cmsIOPrintf(m, "/DecodeLMN [\n");
        _cmsIOPrintf(m, "{{dup 6 29 div ge {{dup dup mul mul}} {{4 29 div sub 108 841 div mul}} ifelse 0.964200 mul}} bind\n");
        _cmsIOPrintf(m, "{{dup 6 29 div ge {{dup dup mul mul}} {{4 29 div sub 108 841 div mul}} ifelse }} bind\n");
        _cmsIOPrintf(m, "{{dup 6 29 div ge {{dup dup mul mul}} {{4 29 div sub 108 841 div mul}} ifelse 0.824900 mul}} bind\n");
        _cmsIOPrintf(m, "]\n");
    }

    private static void EmitSafeGuardBegin(IOHandler m, byte* name)
    {
        var nameStr = new string((sbyte*)name);
        _cmsIOPrintf(m, "%%LCMS2: Save previous definition of {0} on the operand stack\n", nameStr);
        _cmsIOPrintf(m, "currentdict /{0} known {{ /{0} load }} {{ null }} ifelse\n", nameStr);
    }

    private static void EmitSafeGuardEnd(IOHandler m, byte* name, int depth)
    {
        var nameStr = new string((sbyte*)name);

        _cmsIOPrintf(m, "%%LCMS2: Restore previous definition of {0}\n", nameStr);
        if (depth > 1)
        {
            // cycle topmost items on the stack to bring the previous definition to the front
            _cmsIOPrintf(m, "{0} -1 roll ", depth);
        }
        _cmsIOPrintf(m, "dup null eq {{ pop currentdict /{0} undef }} {{ /{0} exch def }} ifelse\n", nameStr);
    }

    private static void Emit1Gamma(IOHandler m, ToneCurve* Table, byte* name)
    {
        if (Table is null ||        // Error
            Table->nEntries <= 0)   // Empty table
        {
            return;
        }

        // Suppress whole if identity
        if (cmsIsToneCurveLinear(Table)) return;

        // Check if is really an exponetial. If so, emit "exp"
        var gamma = cmsEstimateGamma(Table, 0.001);
        if (gamma > 0)
        {
            _cmsIOPrintf(m, "/{0} {{ {1:g} exp }} bind def\n", new string((sbyte*)name), gamma);
            return;
        }

        var lcms2gammatable = "lcms2gammatable".ToBytePtr();
        EmitSafeGuardBegin(m, lcms2gammatable);
        _cmsIOPrintf(m, "/lcms2gammatable [");

        for (var i = 0; i < Table->nEntries; i++)
        {
            if (i % 10 is 0)
                _cmsIOPrintf(m, "\n  ");
            _cmsIOPrintf(m, "{0:d} ", Table->Table16[i]);
        }

        _cmsIOPrintf(m, "] def\n");

        // Emit interpolation code

        // PostScript code                            Stack
        // ===============                            ========================
        // v
        _cmsIOPrintf(m, "/{0} {{\n  ", new string((sbyte*)name));

        // Bounds check
        EmitRangeCheck(m);

        _cmsIOPrintf(m, "\n  //lcms2gammatable ");    // v tab
        _cmsIOPrintf(m, "dup ");                      // v tab tab
        _cmsIOPrintf(m, "length 1 sub ");             // v tab dom
        _cmsIOPrintf(m, "3 -1 roll ");                // tab dom v
        _cmsIOPrintf(m, "mul ");                      // tab val2
        _cmsIOPrintf(m, "dup ");                      // tab val2 val2
        _cmsIOPrintf(m, "dup ");                      // tab val2 val2 val2
        _cmsIOPrintf(m, "floor cvi ");                // tab val2 val2 cell0
        _cmsIOPrintf(m, "exch ");                     // tab val2 cell0 val2
        _cmsIOPrintf(m, "ceiling cvi ");              // tab val2 cell0 cell1
        _cmsIOPrintf(m, "3 index ");                  // tab val2 cell0 cell1 tab
        _cmsIOPrintf(m, "exch ");                     // tab val2 cell0 tab cell1
        _cmsIOPrintf(m, "get\n  ");                   // tab val2 cell0 y1
        _cmsIOPrintf(m, "4 -1 roll ");                // val2 cell0 y1 tab
        _cmsIOPrintf(m, "3 -1 roll ");                // val2 y1 tab cell0
        _cmsIOPrintf(m, "get ");                      // val2 y1 y0
        _cmsIOPrintf(m, "dup ");                      // val2 y1 y0 y0
        _cmsIOPrintf(m, "3 1 roll ");                 // val2 y0 y1 y0
        _cmsIOPrintf(m, "sub ");                      // val2 y0 (y1-y0)
        _cmsIOPrintf(m, "3 -1 roll ");                // y0 (y1-y0) val2
        _cmsIOPrintf(m, "dup ");                      // y0 (y1-y0) val2 val2
        _cmsIOPrintf(m, "floor cvi ");                // y0 (y1-y0) val2 floor(val2)
        _cmsIOPrintf(m, "sub ");                      // y0 (y1-y0) rest
        _cmsIOPrintf(m, "mul ");                      // y0 t1
        _cmsIOPrintf(m, "add ");                      // y
        _cmsIOPrintf(m, "65535 div\n");               // result

        _cmsIOPrintf(m, "}} bind def\n");

        EmitSafeGuardEnd(m, lcms2gammatable, 1);
    }

    private static bool GammaTableEquals(ushort* g1, ushort* g2, uint nG1, uint nG2) =>
        nG1 == nG2
            ? memcmp(g1, g2, (nint)(nG1 * _sizeof<ushort>())) == 0
            : false;

    private static void EmitNGamma(IOHandler m, uint n, ToneCurve** g, byte* nameprefix)
    {
        var buffer = stackalloc byte[2048];

        for (var i = 0u; i < n; i++)
        {
            if (g[i] is null) return;   // Error

            if (i > 0 && GammaTableEquals(g[i-1]->Table16, g[i]->Table16, g[i-1]->nEntries, g[i]->nEntries))
            {
                _cmsIOPrintf(m, "/{0}{1:d} /{0}{2:d} load def\n", new string((sbyte*)nameprefix), i, i - 1);
            }
            else
            {
                snprintf(buffer, 2048, "{0}{1:d}".ToBytePtr(), new string((sbyte*)nameprefix), i);
                buffer[2047] = 0;
                Emit1Gamma(m, g[i], buffer);
            }
        }
    }

    private static bool OutputValueSampler(in ushort* In, ushort* Out, void* Cargo)
    {
        var sc = (PsSamplerCargo*)Cargo;

        if (sc->FixWhite)
        {
            if (In[0] == 0xffff)    // Only in L* = 100, ab = [-8..8]
            {
                if ((In[1] is >= 0x7800 and <= 0x8800) &&
                    (In[2] is >= 0x7800 and <= 0x8800))
                {
                    ushort* Black;
                    ushort* White;
                    uint nOutputs;

                    if (!_cmsEndPointsBySpace(sc->ColorSpace, &White, &Black, &nOutputs))
                        return false;

                    for (var i = 0u; i < nOutputs; i++)
                        Out[i] = White[i];
                }
            }
        }

        // Handle the parenthesis on rows

        if (In[0] != sc->FirstComponent)
        {
            if (sc->FirstComponent != -1)
            {
                _cmsIOPrintf(sc->m, new string((sbyte*)sc->PostMin));
                sc->SecondComponent = -1;
                _cmsIOPrintf(sc->m, new string((sbyte*)sc->PostMaj));
            }

            // Begin block
            _cmsPSActualColumn = 0;

            _cmsIOPrintf(sc->m, new string((sbyte*)sc->PreMaj));
            sc->FirstComponent = In[0];
        }

        if (In[1] != sc->SecondComponent)
        {
            if (sc->SecondComponent != -1)
            {
                _cmsIOPrintf(sc->m, new string((sbyte*)sc->PostMin));
            }

            _cmsIOPrintf(sc->m, new string((sbyte*)sc->PreMin));
            sc->SecondComponent = In[1];
        }

        // Dump table.

        for (var i = 0u; i < sc->Pipeline.Params->nOutputs; i++)
        {
            var wWordOut = Out[i];

            // We always deal with Lab4;

            var wByteOut = Word2Byte(wWordOut);
            WriteByte(sc->m, wByteOut);
        }

        return true;
    }

    private static void WriteCLUT(
        IOHandler m,
        Stage* mpe,
        byte* PreMaj,
        byte* PostMaj,
        byte* PreMin,
        byte* PostMin,
        bool FixWhite,
        Signature ColorSpace)
    {
        PsSamplerCargo sc;

        sc.FirstComponent = -1;
        sc.SecondComponent = -1;
        sc.Pipeline = (StageCLutData)mpe->Data;
        sc.m = m;
        sc.PreMaj = PreMaj;
        sc.PostMaj = PostMaj;

        sc.PreMin = PreMin;
        sc.PostMin = PostMin;
        sc.FixWhite = FixWhite;
        sc.ColorSpace = ColorSpace;

        _cmsIOPrintf(m, "[");

        for (var i = 0u; i < sc.Pipeline.Params->nInputs; i++)
            _cmsIOPrintf(m, " {0:d} ", sc.Pipeline.Params->nSamples[i]);

        _cmsIOPrintf(m, " [\n");

        cmsStageSampleCLut16bit(mpe, OutputValueSampler, &sc, SamplerFlag.Inspect);

        _cmsIOPrintf(m, new string((sbyte*)PostMin));
        _cmsIOPrintf(m, new string((sbyte*)PostMaj));
        _cmsIOPrintf(m, "] ");
    }

    private static bool EmitCIEBasedA(IOHandler m, ToneCurve* Curve, CIEXYZ* BlackPoint)
    {
        var lcms2gammaproc = "lcms2gammaproc".ToBytePtr();

        _cmsIOPrintf(m, "[ /CIEBasedA\n");
        _cmsIOPrintf(m, "  <<\n");

        EmitSafeGuardBegin(m, lcms2gammaproc);
        Emit1Gamma(m, Curve, lcms2gammaproc);

        _cmsIOPrintf(m, "/DecodeA /lcms2gammaproc load\n");
        EmitSafeGuardEnd(m, lcms2gammaproc, 3);

        _cmsIOPrintf(m, "/MatrixA [ 0.9642 1.0000 0.8249 ]\n");
        _cmsIOPrintf(m, "/RangeLMN [ 0.0 0.9642 0.0 1.0000 0.0 0.8249 ]\n");

        EmitWhiteBlackD50(m, BlackPoint);
        EmitIntent(m, INTENT_PERCEPTUAL);

        _cmsIOPrintf(m, ">>\n");
        _cmsIOPrintf(m, "]\n");

        return true;
    }

    private static bool EmitCIEBasedABC(IOHandler m, double* Matrix, ToneCurve** CurveSet, CIEXYZ* BlackPoint)
    {
        var lcms2gammaproc = "lcms2gammaproc".ToBytePtr();
        var lcms2gammaproc0 = "lcms2gammaproc0".ToBytePtr();
        var lcms2gammaproc1 = "lcms2gammaproc1".ToBytePtr();
        var lcms2gammaproc2 = "lcms2gammaproc2".ToBytePtr();

        _cmsIOPrintf(m, "[ /CIEBasedABC\n");
        _cmsIOPrintf(m, "<<\n");

        EmitSafeGuardBegin(m, lcms2gammaproc0);
        EmitSafeGuardBegin(m, lcms2gammaproc1);
        EmitSafeGuardBegin(m, lcms2gammaproc2);
        EmitNGamma(m, 3, CurveSet, lcms2gammaproc);
        _cmsIOPrintf(m, "/DecodeABC [\n");
        _cmsIOPrintf(m, "   /lcms2gammaproc0 load\n");
        _cmsIOPrintf(m, "   /lcms2gammaproc1 load\n");
        _cmsIOPrintf(m, "   /lcms2gammaproc2 load\n");
        _cmsIOPrintf(m, "]\n");
        EmitSafeGuardEnd(m, lcms2gammaproc2, 3);
        EmitSafeGuardEnd(m, lcms2gammaproc1, 3);
        EmitSafeGuardEnd(m, lcms2gammaproc0, 3);

        _cmsIOPrintf(m, "/MatrixABC [ ");

        for (var i = 0u; i < 3; i++)
        {

            _cmsIOPrintf(m, "{0:f6} {1:f6} {2:f6} ", Matrix[i + 3 * 0],
                                                         Matrix[i + 3 * 1],
                                                         Matrix[i + 3 * 2]);
        }


        _cmsIOPrintf(m, "]\n");

        _cmsIOPrintf(m, "/RangeLMN [ 0.0 0.9642 0.0 1.0000 0.0 0.8249 ]\n");

        EmitWhiteBlackD50(m, BlackPoint);
        EmitIntent(m, INTENT_PERCEPTUAL);

        _cmsIOPrintf(m, ">>\n");
        _cmsIOPrintf(m, "]\n");

        return true;
    }

    private static bool EmitCIEBasedDEF(IOHandler m, Pipeline* Pipeline, uint Intent, CIEXYZ* BlackPoint)
    {
        byte* PreMaj;
        byte* PostMaj;
        byte* PreMin, PostMin;
        var buffer = stackalloc byte[2048];

        var mpe = Pipeline->Elements;

        switch(cmsStageInputChannels(mpe))
        {
            case 3:
                _cmsIOPrintf(m, "[ /CIEBasedDEF\n");
                PreMaj = "<".ToBytePtr();
                PostMaj = ">\n".ToBytePtr();
                PreMin = PostMin = "".ToBytePtr();
                break;
            case 4:
                _cmsIOPrintf(m, "[ /CIEBasedDEFG\n");
                PreMaj = "[".ToBytePtr();
                PostMaj = "]\n".ToBytePtr();
                PreMin = "<".ToBytePtr();
                PostMin = ">\n".ToBytePtr();
                break;
            default:
                return false;
        }

        _cmsIOPrintf(m, "<<\n");

        if ((uint)cmsStageType(mpe) is cmsSigCurveSetElemType)
        {
            var numchans = (int)cmsStageOutputChannels(mpe);
            var format1 = "lcms2gammaproc{0:d}".ToBytePtr();
            var format2 = "lcms2gammaproc{0:d} load\n".ToBytePtr();
            for (var i = 0; i < numchans; i++)
            {
                snprintf(buffer, 2048, format1, i);
                buffer[2047] = 0;
                EmitSafeGuardBegin(m, buffer);
            }
            EmitNGamma(m, cmsStageOutputChannels(mpe), _cmsStageGetPtrToCurveSet(mpe), "lcms2gammaproc".ToBytePtr());
            _cmsIOPrintf(m, "/DecodeDEF [\n");
            for (var i = 0; i < numchans; i++)
            {
                snprintf(buffer, 2048, format2, i);
                buffer[2047] = 0;
                _cmsIOPrintf(m, new string((sbyte*)buffer));
            }
            _cmsIOPrintf(m, "]\n");
            for (var i = 0; i < numchans; i++)
            {
                snprintf(buffer, 2048, format1, i);
                buffer[2047] = 0;
                EmitSafeGuardEnd(m, buffer, 3);
            }

            mpe = mpe->Next;
        }

        if ((uint)cmsStageType(mpe) is cmsSigCLutElemType)
        {
            _cmsIOPrintf(m, "/Table ");
            WriteCLUT(m, mpe, PreMaj, PostMaj, PreMin, PostMin, false, default);
            _cmsIOPrintf(m, "]\n");
        }

        EmitLab2XYZ(m);
        EmitWhiteBlackD50(m, BlackPoint);
        EmitIntent(m, Intent);

        _cmsIOPrintf(m, "   >>\n");
        _cmsIOPrintf(m, "]\n");

        return true;
    }

    private static ToneCurve* ExtractGray2Y(Context? ContextID, Profile Profile, uint Intent)
    {
        var Out = cmsBuildTabulatedToneCurve16(ContextID, 256, null);
        var hXYZ = cmsCreateXYZProfile();
        var xform = cmsCreateTransformTHR(ContextID, Profile, TYPE_GRAY_8, hXYZ, TYPE_XYZ_DBL, Intent, cmsFLAGS_NOOPTIMIZE);

        if (Out is not null && xform is not null)
        {
            for (var i = 0; i < 256; i++)
            {
                var Gray = (byte)i;
                CIEXYZ XYZ;

                cmsDoTransform(xform, &Gray, &XYZ, 1);

                Out->Table16[i] = _cmsQuickSaturateWord(XYZ.Y * 65535.0);
            }
        }

        if (xform is not null) cmsDeleteTransform(xform);
        if (hXYZ is not null) cmsCloseProfile(hXYZ);
        return Out;
    }

    private static bool WriteInputLUT(IOHandler m, Profile Profile, uint Intent, uint dwFlags)
    {
        CIEXYZ BlackPointAdaptedToD50;
        var Profiles = new Profile[2];

        // Does create a device-link based transform.
        // The DeviceLink is next dumped as working CSA.

        var InputFormat = cmsFormatterForColorspaceOfProfile(Profile, 2, false);
        var nChannels = T_CHANNELS(InputFormat);

        cmsDetectBlackPoint(&BlackPointAdaptedToD50, Profile, Intent, 0);

        // Adjust output to Lab4
        var hLab = cmsCreateLab4ProfileTHR(m.ContextID, null);

        Profiles[0] = Profile;
        Profiles[1] = hLab;

        var xform = cmsCreateMultiprofileTransform(Profiles, 2, InputFormat, TYPE_Lab_DBL, Intent, 0);
        cmsCloseProfile(hLab);

        if (xform is null)
        {
            cmsSignalError(m.ContextID, cmsERROR_COLORSPACE_CHECK, "Cannot create transform Profile -> Lab");
            return false;
        }

        // Only 1, 3, and 4 channels are allowed

        switch (nChannels)
        {
            case 1:
                {
                    var Gray2Y = ExtractGray2Y(m.ContextID, Profile, Intent);
                    EmitCIEBasedA(m, Gray2Y, &BlackPointAdaptedToD50);
                    cmsFreeToneCurve(Gray2Y);
                }
                break;
            case 3:
            case 4:
                {
                    var OutFrm = TYPE_Lab_16;
                    var v = xform;

                    var DeviceLink = cmsPipelineDup(v->Lut);
                    if (DeviceLink is null) return false;

                    dwFlags |= cmsFLAGS_FORCE_CLUT;
                    _cmsOptimizePipeline(m.ContextID, &DeviceLink, Intent, &InputFormat, &OutFrm, &dwFlags);

                    var rc = EmitCIEBasedDEF(m, DeviceLink, Intent, &BlackPointAdaptedToD50);
                    cmsPipelineFree(DeviceLink);
                    if (!rc) return false;
                }
                break;
            default:
                cmsSignalError(m.ContextID, cmsERROR_COLORSPACE_CHECK, "Only 3, 4 channels are supported for CSA. This profile has {0} channels.", nChannels);
                return false;
        }

        cmsDeleteTransform(xform);

        return true;
    }

    private static double* GetPtrToMatrix(Stage* mpe) =>
        ((StageMatrixData)mpe->Data).Double;

    private static bool WriteInputMatrixShaper(IOHandler m, Profile Profile, Stage* Matrix, Stage* Shaper)
    {
        bool rc;
        CIEXYZ BlackPointAdaptedToD50;

        var ColorSpace = cmsGetColorSpace(Profile);

        cmsDetectBlackPoint(&BlackPointAdaptedToD50, Profile, INTENT_RELATIVE_COLORIMETRIC, 0);

        if ((uint)ColorSpace is cmsSigGrayData)
        {
            var ShaperCurve = _cmsStageGetPtrToCurveSet(Shaper);
            rc = EmitCIEBasedA(m, ShaperCurve[0], &BlackPointAdaptedToD50);
        }
        else if ((uint)ColorSpace is cmsSigRgbData)
        {
            MAT3 Mat;

            memmove(&Mat, GetPtrToMatrix(Matrix), _sizeof<MAT3>());

            for (var i = 0; i < 9; i++)
                ((double*)&Mat)[i] *= MAX_ENCODEABLE_XYZ;

            rc = EmitCIEBasedABC(m, (double*)&Mat, _cmsStageGetPtrToCurveSet(Shaper), &BlackPointAdaptedToD50);
        }
        else
        {
            cmsSignalError(m.ContextID, cmsERROR_COLORSPACE_CHECK, "Profile is not suitable for CSA. Unsupported colorspace.");
            return false;
        }

        return rc;
    }

    private static bool WriteNamedColorCSA(IOHandler m, Profile hNamedColor, uint Intent)
    {
        var ColorName = stackalloc byte[cmsMAX_PATH];

        var hLab = cmsCreateLab4ProfileTHR(m.ContextID, null);
        var xform = cmsCreateTransform(hNamedColor, TYPE_NAMED_COLOR_INDEX, hLab, TYPE_Lab_DBL, Intent, 0);
        if (xform is null) return false;

        var NamedColorList = cmsGetNamedColorList(xform);
        if (NamedColorList is null) return false;

        _cmsIOPrintf(m, "<<\n");
        _cmsIOPrintf(m, "(colorlistcomment) (Named color CSA)\n");
        _cmsIOPrintf(m, "(Prefix) [ (Pantone ) (PANTONE ) ]\n");
        _cmsIOPrintf(m, "(Suffix) [ ( CV) ( CVC) ( C) ]\n");

        var nColors = cmsNamedColorCount(NamedColorList);

        var In = stackalloc ushort[1];
        for (var i = 0u; i < nColors; i++)
        {
            CIELab Lab;

            In[0] = (ushort)i;

            if (!cmsNamedColorInfo(NamedColorList, i, ColorName, null, null, null, null))
                continue;

            cmsDoTransform(xform, In, &Lab, 1);
            _cmsIOPrintf(m, "  ({0}) [ {1:f3} {2:f3} {3:f3} ]\n", new string((sbyte*)ColorName), Lab.L, Lab.a, Lab.b);
        }

        _cmsIOPrintf(m, ">>\n");

        cmsDeleteTransform(xform);
        cmsCloseProfile(hLab);
        return true;
    }

    private static uint GenerateCSA(
        Context? ContextID,
        Profile Profile,
        uint Intent,
        uint dwFlags,
        IOHandler mem)
    {
        Pipeline* lut = null;
        Stage* Matrix, Shaper;

        // Is a named color profile?
        if ((uint)cmsGetDeviceClass(Profile) is cmsSigNamedColorClass)
        {
            if (!WriteNamedColorCSA(mem, Profile, Intent)) goto Error;
        }
        else
        {
            // Any profile class are allowed (including devicelink), but
            // output (PCS) colorspace must be XYZ or LAB
            var ColorSpace = cmsGetPCS(Profile);

            if ((uint)ColorSpace is not cmsSigXYZData and not cmsSigLabData)
            {
                cmsSignalError(ContextID, cmsERROR_COLORSPACE_CHECK, "Invalid output color space");
                goto Error;
            }

            // Read the lut with all necessary conversion stages
            lut = _cmsReadInputLUT(Profile, Intent);
            if (lut is null) goto Error;

            // TOne curves + matrix can be implemented without and LUT
            if (cmsPipelineCheckAndRetrieveStages(lut, &Shaper, &Matrix, cmsSigCurveSetElemType, cmsSigMatrixElemType))
            {
                if (!WriteInputMatrixShaper(mem, Profile, Matrix, Shaper)) goto Error;
            }
            else
            {
                // We need a LUT for the rest
                if (!WriteInputLUT(mem, Profile, Intent, dwFlags)) goto Error;
            }
        }

        // Done, keep memory usage
        var dwBytesUsed = mem.UsedSpace;

        // Get rid of LUT
        if (lut is not null) cmsPipelineFree(lut);

        // Finally, return used byte count
        return dwBytesUsed;

    Error:
        if (lut is not null) cmsPipelineFree(lut);
        return 0;
    }

    private static void EmitPQRStage(IOHandler m, Profile Profile, bool DoBPC, bool lIsAbsolute)
    {
        if (lIsAbsolute)
        {
            // For absolute colorimetric intent, encode back to relative
            // and generate a relative Pipeline

            // Relative encoding is obtained across XYZpcs*(D50/WhitePoint)

            CIEXYZ White;

            _cmsReadMediaWhitePoint(&White, Profile);

            _cmsIOPrintf(m, "/MatrixPQR [1 0 0 0 1 0 0 0 1 ]\n");
            _cmsIOPrintf(m, "/RangePQR [ -0.5 2 -0.5 2 -0.5 2 ]\n");

            _cmsIOPrintf(m, "% Absolute colorimetric -- encode to relative to maximize LUT usage\n" +
                      "/TransformPQR [\n"+
                      "{{0.9642 mul {0:g} div exch pop exch pop exch pop exch pop}} bind\n"+
                      "{{1.0000 mul {1:g} div exch pop exch pop exch pop exch pop}} bind\n"+
                      "{{0.8249 mul {2:g} div exch pop exch pop exch pop exch pop}} bind\n]\n",
                      White.X, White.Y, White.Z);
            return;
        }

        _cmsIOPrintf(m, "% Bradford Cone Space\n"+
                 "/MatrixPQR [0.8951 -0.7502 0.0389 0.2664 1.7135 -0.0685 -0.1614 0.0367 1.0296 ] \n");

        _cmsIOPrintf(m, "/RangePQR [ -0.5 2 -0.5 2 -0.5 2 ]\n");

        // No BPC

        if (!DoBPC)
        {
            _cmsIOPrintf(m, "% VonKries-like transform in Bradford Cone Space\n"+
                      "/TransformPQR [\n"+
                      "{{exch pop exch 3 get mul exch pop exch 3 get div}} bind\n"+
                      "{{exch pop exch 4 get mul exch pop exch 4 get div}} bind\n"+
                      "{{exch pop exch 5 get mul exch pop exch 5 get div}} bind\n]\n");
        }
        else
        {
            // BPC

            _cmsIOPrintf(m, "%% VonKries-like transform in Bradford Cone Space plus BPC\n"+
                      "/TransformPQR [\n");

            _cmsIOPrintf(m, "{{4 index 3 get div 2 index 3 get mul "+
                    "2 index 3 get 2 index 3 get sub mul "+
                    "2 index 3 get 4 index 3 get 3 index 3 get sub mul sub "+
                    "3 index 3 get 3 index 3 get exch sub div "+
                    "exch pop exch pop exch pop exch pop }} bind\n");

            _cmsIOPrintf(m, "{{4 index 4 get div 2 index 4 get mul "+
                    "2 index 4 get 2 index 4 get sub mul "+
                    "2 index 4 get 4 index 4 get 3 index 4 get sub mul sub "+
                    "3 index 4 get 3 index 4 get exch sub div "+
                    "exch pop exch pop exch pop exch pop }} bind\n");

            _cmsIOPrintf(m, "{{4 index 5 get div 2 index 5 get mul "+
                    "2 index 5 get 2 index 5 get sub mul "+
                    "2 index 5 get 4 index 5 get 3 index 5 get sub mul sub "+
                    "3 index 5 get 3 index 5 get exch sub div "+
                    "exch pop exch pop exch pop exch pop }} bind\n]\n");

        }
    }

    private static void EmitXYZ2Lab(IOHandler m)
    {
        _cmsIOPrintf(m, "/RangeLMN [ -0.635 2.0 0 2 -0.635 2.0 ]\n");
        _cmsIOPrintf(m, "/EncodeLMN [\n");
        _cmsIOPrintf(m, "{{ 0.964200  div dup 0.008856 le {{7.787 mul 16 116 div add}}{{1 3 div exp}} ifelse }} bind\n");
        _cmsIOPrintf(m, "{{ 1.000000  div dup 0.008856 le {{7.787 mul 16 116 div add}}{{1 3 div exp}} ifelse }} bind\n");
        _cmsIOPrintf(m, "{{ 0.824900  div dup 0.008856 le {{7.787 mul 16 116 div add}}{{1 3 div exp}} ifelse }} bind\n");
        _cmsIOPrintf(m, "]\n");
        _cmsIOPrintf(m, "/MatrixABC [ 0 1 0 1 -1 1 0 0 -1 ]\n");
        _cmsIOPrintf(m, "/EncodeABC [\n");


        _cmsIOPrintf(m, "{{ 116 mul  16 sub 100 div  }} bind\n");
        _cmsIOPrintf(m, "{{ 500 mul 128 add 256 div  }} bind\n");
        _cmsIOPrintf(m, "{{ 200 mul 128 add 256 div  }} bind\n");


        _cmsIOPrintf(m, "]\n");
    }

    private static bool WriteOutputLUT(IOHandler m, Profile Profile, uint Intent, uint dwFlags)
    {
        var Profiles = new Profile[3];
        CIEXYZ BlackPointAdaptedToD50;
        var lDoBPC = (dwFlags & cmsFLAGS_BLACKPOINTCOMPENSATION) is not 0;
        var lFixWhite = (dwFlags & cmsFLAGS_NOWHITEONWHITEFIXUP) is 0;
        var InFrm = TYPE_Lab_16;

        var hLab = cmsCreateLab4ProfileTHR(m.ContextID, null);
        if (hLab is null) return false;

        var OutputFormat = cmsFormatterForColorspaceOfProfile(Profile, 2, false);
        var nChannels = T_CHANNELS(OutputFormat);

        var ColorSpace = cmsGetColorSpace(Profile);

        // For absolute colorimetric, the LUT is encoded as relative in order to preserve precision.

        var RelativeEncodingIntent = Intent;
        if (RelativeEncodingIntent is INTENT_ABSOLUTE_COLORIMETRIC)
            RelativeEncodingIntent = INTENT_RELATIVE_COLORIMETRIC;

        // Use V4 Lab always
        Profiles[0] = hLab;
        Profiles[1] = Profile;

        var xform = cmsCreateMultiprofileTransformTHR(m.ContextID, Profiles, 2, TYPE_Lab_DBL, OutputFormat,
                                                      RelativeEncodingIntent, 0);

        cmsCloseProfile(hLab);

        if (xform is null)
        {
            cmsSignalError(m.ContextID, cmsERROR_COLORSPACE_CHECK, "Cannot create transform Lab -> Profile in CRD creation");
            return false;
        }

        // Get a copy of the internal devicelink
        var v = xform;
        var DeviceLink = cmsPipelineDup(v->Lut);
        if (DeviceLink is null) return false;

        // We need a CLUT
        dwFlags |= cmsFLAGS_FORCE_CLUT;
        _cmsOptimizePipeline(m.ContextID, &DeviceLink, RelativeEncodingIntent, &InFrm, &OutputFormat, &dwFlags);

        _cmsIOPrintf(m, "<<\n");
        _cmsIOPrintf(m, "/ColorRenderingType 1\n");

        cmsDetectBlackPoint(&BlackPointAdaptedToD50, Profile, Intent, 0);

        // Emit headers, etc.
        EmitWhiteBlackD50(m, &BlackPointAdaptedToD50);
        EmitPQRStage(m, Profile, lDoBPC, Intent is INTENT_ABSOLUTE_COLORIMETRIC);
        EmitXYZ2Lab(m);

        // FIXUP: map Lab (100, 0, 0) to perfect white, because the particular encoding for Lab
        // does map a=b=0 not falling into any specific node. Since range a,b goes -128..127,
        // zero is slightly moved towards right, so assure next node (in L=100 slice) is mapped to
        // zero. This would sacrifice a bit of highlights, but failure to do so would cause
        // scum dot. Ouch.

        if (Intent is INTENT_ABSOLUTE_COLORIMETRIC)
            lFixWhite = false;

        _cmsIOPrintf(m, "/RenderTable ");

        WriteCLUT(m, cmsPipelineGetPtrToFirstStage(DeviceLink), "<".ToBytePtr(), ">\n".ToBytePtr(), "".ToBytePtr(),
                  "".ToBytePtr(), lFixWhite, ColorSpace);

        _cmsIOPrintf(m, " {0:d} {{}} bind ", nChannels);

        for (var i = 1u; i < nChannels; i++)
            _cmsIOPrintf(m, "dup ");

        _cmsIOPrintf(m, "]\n");

        EmitIntent(m, Intent);

        _cmsIOPrintf(m, ">>\n");

        if ((dwFlags & cmsFLAGS_NODEFAULTRESOURCEDEF) is 0)
            _cmsIOPrintf(m, "/Current exch /ColorRendering defineresource pop\n");

        cmsPipelineFree(DeviceLink);
        cmsDeleteTransform(xform);

        return true;
    }

    private static void BuildColorantList(byte* Colorant, uint nColorant, ushort* Out)
    {
        var Buff = stackalloc byte[32];

        Colorant[0] = 0;
        if (nColorant > cmsMAXCHANNELS)
            nColorant = cmsMAXCHANNELS;

        var format = "{0:f3}".ToBytePtr();
        for (var j = 0u; j < nColorant; j++)
        {
            snprintf(Buff, 31, format, Out[j] / 65535.0);
            Buff[31] = 0;
            strcat(Colorant, Buff);
            if (j < nColorant - 1)
                strcat(Colorant, " ".ToBytePtr());
        }
    }

    private static bool WriteNamedColorCRD(IOHandler m, Profile hNamedColor, uint Intent, uint dwFlags)
    {
        var ColorName = stackalloc byte[cmsMAX_PATH];
        var Colorant = stackalloc byte[512];

        var OutputFormat = cmsFormatterForColorspaceOfProfile(hNamedColor, 2, false);
        var nColorant = T_CHANNELS(OutputFormat);

        var xform = cmsCreateTransform(hNamedColor, TYPE_NAMED_COLOR_INDEX, null, OutputFormat, Intent, dwFlags);
        if (xform is null) return false;

        var NamedColorList = cmsGetNamedColorList(xform);
        if (NamedColorList is null) return false;

        _cmsIOPrintf(m, "<<\n");
        _cmsIOPrintf(m, "(colorlistcomment) (Named profile) \n");
        _cmsIOPrintf(m, "(Prefix) [ (Pantone ) (PANTONE ) ]\n");
        _cmsIOPrintf(m, "(Suffix) [ ( CV) ( CVC) ( C) ]\n");

        var nColors = cmsNamedColorCount(NamedColorList);

        var In = stackalloc ushort[1];
        var Out = stackalloc ushort[cmsMAXCHANNELS];
        for (var i = 0u; i < nColors; i++)
        {
            In[0] = (ushort)i;

            if (!cmsNamedColorInfo(NamedColorList, i, ColorName, null, null, null, null))
                continue;

            cmsDoTransform(xform, In, Out, 1);
            BuildColorantList(Colorant, nColorant, Out);
            _cmsIOPrintf(m, "  ({0}) [ {1} ]\n", new string((sbyte*)ColorName), new string((sbyte*)Colorant));
        }

        _cmsIOPrintf(m, "   >>");

        if ((dwFlags & cmsFLAGS_NODEFAULTRESOURCEDEF) is 0)
        {
            _cmsIOPrintf(m, " /Current exch /HPSpotTable defineresource pop\n");
        }

        cmsDeleteTransform(xform);
        return true;
    }

    private static uint GenerateCRD(Context? _, Profile Profile, uint Intent, uint dwFlags, IOHandler mem)
    {
        if ((dwFlags & cmsFLAGS_NODEFAULTRESOURCEDEF) is 0)
            EmitHeader(mem, "Color Rendering Dictionary (CRD)".ToBytePtr(), Profile);

        // Is a named color profile?
        if ((uint)cmsGetDeviceClass(Profile) is cmsSigNamedColorClass)
        {
            if (!WriteNamedColorCRD(mem, Profile, Intent, dwFlags))
                return 0;
        }
        else
        {
            // CRD are always implemented as LUT
            if (!WriteOutputLUT(mem, Profile, Intent, dwFlags))
                return 0;
        }

        if ((dwFlags & cmsFLAGS_NODEFAULTRESOURCEDEF) is 0)
        {
            _cmsIOPrintf(mem, "%%EndResource\n");
            _cmsIOPrintf(mem, "\n% CRD End\n");
        }

        // Return used byte count
        return mem.UsedSpace;
    }

    public static uint cmsGetPostScriptColorResource(Context? ContextID, PostScriptResourceType Type, Profile Profile,
                                                     uint Intent, uint dwFlags, IOHandler io) =>
        Type switch
        {
            PostScriptResourceType.CSA => GenerateCSA(ContextID, Profile, Intent, dwFlags, io),
            _ => GenerateCRD(ContextID, Profile, Intent, dwFlags, io)
        };

    public static uint cmsGetPostScriptCRD(Context? ContextID, Profile Profile, uint Intent, uint dwFlags,
                                           void* Buffer, uint dwBufferLen)
    {
        // Set up the serialization engine
        var mem = Buffer is null
            ? cmsOpenIOhandlerFromNULL(ContextID)
            : cmsOpenIOhandlerFromMem(ContextID, Buffer, dwBufferLen, "w");

        if (mem is null) return 0;

        var dwBytesUsed = cmsGetPostScriptColorResource(ContextID, PostScriptResourceType.CRD, Profile, Intent, dwFlags, mem);

        // Get rid of memory stream
        cmsCloseIOhandler(mem);

        return dwBytesUsed;
    }

    public static uint cmsGetPostScriptCSA(Context? ContextID, Profile Profile, uint Intent, uint dwFlags,
                                           void* Buffer, uint dwBufferLen)
    {
        // Set up the serialization engine
        var mem = Buffer is null
            ? cmsOpenIOhandlerFromNULL(ContextID)
            : cmsOpenIOhandlerFromMem(ContextID, Buffer, dwBufferLen, "w");

        if (mem is null) return 0;

        var dwBytesUsed = cmsGetPostScriptColorResource(ContextID, PostScriptResourceType.CSA, Profile, Intent, dwFlags, mem);

        // Get rid of memory stream
        cmsCloseIOhandler(mem);

        return dwBytesUsed;
    }
}
