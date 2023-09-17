using lcms2.state;
using lcms2.types;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace lcms2.FastFloatPlugin.testbed;
internal static partial class Testbed
{
    private static double MPixSec(double diff)
    {
        var seconds = diff / 1000;
        return 256.0 * 256.0 * 256.0 / (1024.0 * 1024.0 * seconds);
    }

    private delegate TimeSpan perf_fn(Context? ct, Profile profileIn, Profile profileOut);

    private static TimeSpan Performance(string Title, perf_fn fn, Context? ct, Memory<byte> inICC, Memory<byte> outICC, long sz, TimeSpan prev)
    {
        var profileIn = loadProfile(inICC);
        var profileOut = loadProfile(outICC);

        var n = fn(ct, profileIn, profileOut);

        var prevMPix = MPixSec(prev.TotalMilliseconds);
        var nMPix = MPixSec(n.TotalMilliseconds);
        if (prevMPix > 0.0)
        {
            var imp = nMPix / prevMPix;
            if (imp > 1)
                trace("{0}: {1:F2} MPixel/sec. {2:F2} MByte/sec. (x {3:F1})", Title, nMPix, nMPix * sz, imp);
            else
                trace("{0}: {1:F2} MPixel/sec. {2:F2} MByte/sec.", Title, nMPix, nMPix * sz);

        }
        else
        {
            trace("{0}: {1:F2} MPixel/sec. {2:F2} MByte/sec.", Title, nMPix, nMPix * sz);
        }

        return n;
    }

    private static void ComparativeCt(Context? ct1, Context? ct2, string Title, perf_fn fn1, perf_fn fn2, Memory<byte> inICC, Memory<byte> outICC)
    {
        var profileIn = inICC.IsEmpty ? CreateCurves() : cmsOpenProfileFromMem(inICC)!;
        var profileOut = outICC.IsEmpty ? CreateCurves() : cmsOpenProfileFromMem(outICC)!;

        var n1 = fn1(ct1, profileIn, profileOut);

        profileIn = inICC.IsEmpty ? CreateCurves() : cmsOpenProfileFromMem(inICC)!;
        profileOut = outICC.IsEmpty ? CreateCurves() : cmsOpenProfileFromMem(outICC)!;

        var n2 = fn2(ct2, profileIn, profileOut);

        trace("{0}: {1} MPixel/sec. (16 bit) {2} MPixel/sec. (float)", Title, n1, n2);
    }

    private static void Comparative(string Title, perf_fn fn1, perf_fn fn2, Memory<byte> inICC, Memory<byte> outICC) =>
        ComparativeCt(null, null, Title, fn1, fn2, inICC, outICC);

    private static TimeSpan SpeedTest8bitsRGB(Context? ct, Profile profileIn, Profile profileOut)
    {
        if (profileIn is null || profileOut is null)
            Fail("Unable to open profiles");

        var lcmsxform = cmsCreateTransformTHR(ct, profileIn, TYPE_RGB_8, profileOut, TYPE_RGB_8, INTENT_PERCEPTUAL, cmsFLAGS_NOCACHE)!;
        cmsCloseProfile(profileIn);
        cmsCloseProfile(profileOut);

        var Mb = 256 * 256 * 256;
        var In = new Scanline_rgb8bits[Mb];

        var j = 0;
        for (var r = 0; r < 256; r++)
        {
            for (var g = 0; g < 256; g++)
            {
                for (var b = 0; b < 256; b++)
                {
                    In[j].r = (byte)r;
                    In[j].g = (byte)g;
                    In[j].b = (byte)b;

                    j++;
                }
            }
        }

        var atime = Stopwatch.StartNew();

        cmsDoTransform(lcmsxform, In, In, (uint)Mb);

        atime.Stop();

        cmsDeleteTransform(lcmsxform);

        return atime.Elapsed;
    }

    public static void SpeedTest8()
    {
        var size_rgb8bits = Unsafe.SizeOf<Scanline_rgb8bits>();

        var noPlugin = cmsCreateContext();
        Thread.Sleep(10);
        Console.WriteLine();

        TimeSpan t0;

        using (logger.BeginScope("8bit performance tests"))
        {
            using (logger.BeginScope("Default"))
            {
                trace("P E R F O R M A N C E   T E S T S   8 B I T S  (D E F A U L T)");

                t0 = Performance("8 bits on CLUT profiles", SpeedTest8bitsRGB, noPlugin, TestProfiles.test5, TestProfiles.test3, size_rgb8bits, default);
            }

            Thread.Sleep(10);
            Console.WriteLine();

            // Note that context null has the plug-in installed

            using (logger.BeginScope("Plugin"))
            {
                trace("P E R F O R M A N C E   T E S T S   8 B I T S  (P L U G I N)");

                Performance("8 bits on CLUT profiles", SpeedTest8bitsRGB, null, TestProfiles.test5, TestProfiles.test3, size_rgb8bits, t0);
            }
        }
    }
}
