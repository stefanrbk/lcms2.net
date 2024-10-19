using lcms2.types;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using S15Fixed16Number = System.Int32;
using U8Fixed8Number = System.UInt16;

namespace lcms2;
public static class Conversions
{
    public static double U8Fixed8ToDouble(U8Fixed8Number fixed8) =>  // _cms8Fixed8toDouble
        fixed8 / 256.0;

    public static U8Fixed8Number DoubleToU8Fixed8(double val)    // _cmsDoubleTo8Fixed8
    {
        var tmp = DoubleToS15Fixed16(val);
        return (ushort)((tmp >> 8) & 0xffff);
    }

    public static double S15Fixed16ToDouble(S15Fixed16Number fix32) =>   // _cms15Fixed16toDouble
        fix32 / 65536.0;

    public static S15Fixed16Number DoubleToS15Fixed16(double v) =>   // _cmsDoubleTo15Fixed16
        (S15Fixed16Number)Math.Floor((v * 65536.0) + 0.5);
}
