using lcms2.state;
using lcms2.types;

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace lcms2.FastFloatPlugin.testbed;
internal static partial class Testbed
{
    private static void ComparativeCt(Context? ct1, Context? ct2, string Title, perf_fn fn1, perf_fn fn2, Memory<byte> inICC, Memory<byte> outICC)
    {
        using (logger.BeginScope(Title))
        {
            var profileIn = inICC.IsEmpty ? CreateCurves() : cmsOpenProfileFromMem(inICC)!;
            var profileOut = outICC.IsEmpty ? CreateCurves() : cmsOpenProfileFromMem(outICC)!;

            var n1 = fn1(ct1, profileIn, profileOut);

            profileIn = inICC.IsEmpty ? CreateCurves() : cmsOpenProfileFromMem(inICC)!;
            profileOut = outICC.IsEmpty ? CreateCurves() : cmsOpenProfileFromMem(outICC)!;

            var n2 = fn2(ct2, profileIn, profileOut);

            trace("{1} MPixel/sec. (16 bit)\t{2} MPixel/sec. (float)", Title, n1, n2);
        }
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

    private static TimeSpan SpeedTest8bitsRGBA(Context? ct, Profile profileIn, Profile profileOut)
    {
        if (profileIn is null || profileOut is null)
            Fail("Unable to open profiles");

        var lcmsxform = cmsCreateTransformTHR(ct, profileIn, TYPE_RGBA_8, profileOut, TYPE_RGBA_8, INTENT_PERCEPTUAL, cmsFLAGS_NOCACHE)!;
        cmsCloseProfile(profileIn);
        cmsCloseProfile(profileOut);

        var Mb = 256 * 256 * 256;
        var In = new Scanline_rgba8bits[Mb];

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
                    In[j].a = 0;

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

    private static TimeSpan SpeedTest15bitsRGB(Context? ct, Profile profileIn, Profile profileOut)
    {
        if (profileIn is null || profileOut is null)
            Fail("Unable to open profiles");

        var lcmsxform = cmsCreateTransformTHR(ct, profileIn, TYPE_RGB_15, profileOut, TYPE_RGB_15, INTENT_PERCEPTUAL, cmsFLAGS_NOCACHE)!;
        cmsCloseProfile(profileIn);
        cmsCloseProfile(profileOut);

        var Mb = 256 * 256 * 256;
        var In = new Scanline_rgb15bits[Mb];

        var j = 0;
        for (var r = 0; r < 256; r++)
        {
            for (var g = 0; g < 256; g++)
            {
                for (var b = 0; b < 256; b++)
                {
                    In[j].r = (ushort)r;
                    In[j].g = (ushort)g;
                    In[j].b = (ushort)b;

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

    private static TimeSpan SpeedTest15bitsRGBA(Context? ct, Profile profileIn, Profile profileOut)
    {
        if (profileIn is null || profileOut is null)
            Fail("Unable to open profiles");

        var lcmsxform = cmsCreateTransformTHR(ct, profileIn, TYPE_RGBA_15, profileOut, TYPE_RGBA_15, INTENT_PERCEPTUAL, cmsFLAGS_NOCACHE)!;
        cmsCloseProfile(profileIn);
        cmsCloseProfile(profileOut);

        var Mb = 256 * 256 * 256;
        var In = new Scanline_rgba15bits[Mb];

        var j = 0;
        for (var r = 0; r < 256; r++)
        {
            for (var g = 0; g < 256; g++)
            {
                for (var b = 0; b < 256; b++)
                {
                    In[j].r = (ushort)r;
                    In[j].g = (ushort)g;
                    In[j].b = (ushort)b;
                    In[j].a = 0;

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

    private static TimeSpan SpeedTest15bitsCMYK(Context? ct, Profile profileIn, Profile profileOut)
    {
        if (profileIn is null || profileOut is null)
            Fail("Unable to open profiles");

        var lcmsxform = cmsCreateTransformTHR(ct, profileIn, TYPE_CMYK_15, profileOut, TYPE_CMYK_15, INTENT_PERCEPTUAL, cmsFLAGS_NOCACHE)!;
        cmsCloseProfile(profileIn);
        cmsCloseProfile(profileOut);

        var Mb = 256 * 256 * 256;
        var In = new Scanline_cmyk15bits[Mb];

        var j = 0;
        for (var r = 0; r < 256; r++)
        {
            for (var g = 0; g < 256; g++)
            {
                for (var b = 0; b < 256; b++)
                {
                    In[j].c = (ushort)r;
                    In[j].m = (ushort)g;
                    In[j].y = (ushort)b;
                    In[j].k = 0;

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

    private static TimeSpan SpeedTest16bitsRGB(Context? ct, Profile profileIn, Profile profileOut)
    {
        if (profileIn is null || profileOut is null)
            Fail("Unable to open profiles");

        var lcmsxform = cmsCreateTransformTHR(ct, profileIn, TYPE_RGB_16, profileOut, TYPE_RGB_16, INTENT_PERCEPTUAL, cmsFLAGS_NOCACHE)!;
        cmsCloseProfile(profileIn);
        cmsCloseProfile(profileOut);

        var Mb = 256 * 256 * 256;
        var In = new Scanline_rgb16bits[Mb];

        var j = 0;
        for (var r = 0; r < 256; r++)
        {
            for (var g = 0; g < 256; g++)
            {
                for (var b = 0; b < 256; b++)
                {
                    In[j].r = FROM_8_TO_16((uint)r);
                    In[j].g = FROM_8_TO_16((uint)g);
                    In[j].b = FROM_8_TO_16((uint)b);
                                
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

    private static TimeSpan SpeedTest16bitsCMYK(Context? ct, Profile profileIn, Profile profileOut)
    {
        if (profileIn is null || profileOut is null)
            Fail("Unable to open profiles");

        var lcmsxform = cmsCreateTransformTHR(ct, profileIn, TYPE_CMYK_16, profileOut, TYPE_CMYK_16, INTENT_PERCEPTUAL, cmsFLAGS_NOCACHE)!;
        cmsCloseProfile(profileIn);
        cmsCloseProfile(profileOut);

        var Mb = 256 * 256 * 256;
        var In = new Scanline_cmyk16bits[Mb];

        var j = 0;
        for (var r = 0; r < 256; r++)
        {
            for (var g = 0; g < 256; g++)
            {
                for (var b = 0; b < 256; b++)
                {
                    In[j].c = (ushort)r;
                    In[j].m = (ushort)g;
                    In[j].y = (ushort)b;
                    In[j].k = (ushort)r;

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

        TimeSpan t0, t1, t2, t3;

        using (logger.BeginScope("8 bit performance"))
        {
            using (logger.BeginScope("Default"))
            {
                trace("P E R F O R M A N C E   T E S T S   8 B I T S  (D E F A U L T)");

                t0 = Performance("8 bits on CLUT profiles", SpeedTest8bitsRGB, noPlugin, TestProfiles.test5, TestProfiles.test3, size_rgb8bits, default);
                t1 = Performance("8 bits on Matrix-Shaper", SpeedTest8bitsRGB, noPlugin, TestProfiles.test5, TestProfiles.test0, size_rgb8bits, default);
                t2 = Performance("8 bits on same Matrix-Shaper", SpeedTest8bitsRGB, noPlugin, TestProfiles.test0, TestProfiles.test0, size_rgb8bits, default);
                t3 = Performance("8 bits on curves", SpeedTest8bitsRGB, noPlugin, "*curves", "*curves", size_rgb8bits, default);
            }

            Thread.Sleep(10);
            Console.WriteLine();

            // Note that context null has the plug-in installed

            using (logger.BeginScope("Plugin"))
            {
                trace("P E R F O R M A N C E   T E S T S   8 B I T S  (P L U G I N)");

                Performance("8 bits on CLUT profiles", SpeedTest8bitsRGB, null, TestProfiles.test5, TestProfiles.test3, size_rgb8bits, t0);
                Performance("8 bits on Matrix-Shaper", SpeedTest8bitsRGB, null, TestProfiles.test5, TestProfiles.test0, size_rgb8bits, t1);
                Performance("8 bits on same Matrix-Shaper", SpeedTest8bitsRGB, null, TestProfiles.test0, TestProfiles.test0, size_rgb8bits, t2);
                Performance("8 bits on curves", SpeedTest8bitsRGB, null, "*curves", "*curves", size_rgb8bits, t3);
            }
        }

        cmsDeleteContext(noPlugin);
    }

    public static void SpeedTest15()
    {
        var size_rgb15bits = Unsafe.SizeOf<Scanline_rgb15bits>();
        var size_cmyk15bits = Unsafe.SizeOf<Scanline_cmyk15bits>();

        var noPlugin = cmsCreateContext();
        Thread.Sleep(10);
        Console.WriteLine();

        using (logger.BeginScope("15 bit performance"))
        {
            using (logger.BeginScope("Plugin"))
            {
                trace("P E R F O R M A N C E   T E S T S   1 5  B I T S  (P L U G I N)");

                Performance("15 bits on CLUT profiles", SpeedTest15bitsRGB, null, TestProfiles.test5, TestProfiles.test3, size_rgb15bits, default);
                Performance("15 bits on Matrix-Shaper profiles", SpeedTest15bitsRGB, null, TestProfiles.test5, TestProfiles.test0, size_rgb15bits, default);
                Performance("15 bits on same Matrix-Shaper", SpeedTest15bitsRGB, null, TestProfiles.test0, TestProfiles.test0, size_rgb15bits, default);
                Performance("15 bits on curves", SpeedTest15bitsRGB, null, "*curves", "*curves", size_rgb15bits, default);
                Performance("15 bits on CMYK CLUT profiles", SpeedTest15bitsCMYK, null, TestProfiles.test1, TestProfiles.test2, size_cmyk15bits, default);
            }
        }

        cmsDeleteContext(noPlugin);
    }

    public static void SpeedTest16()
    {
        var size_rgb16bits = Unsafe.SizeOf<Scanline_rgb16bits>();
        var size_cmyk16bits = Unsafe.SizeOf<Scanline_cmyk16bits>();

        var noPlugin = cmsCreateContext();
        Thread.Sleep(10);
        Console.WriteLine();

        TimeSpan t0, t1, t2, t3, t4;

        using (logger.BeginScope("16 bit performance"))
        {
            using (logger.BeginScope("Default"))
            {
                trace("P E R F O R M A N C E   T E S T S   1 6  B I T S  (D E F A U L T)");

                t0 = Performance("16 bits on CLUT profiles",          SpeedTest16bitsRGB,  noPlugin, TestProfiles.test5, TestProfiles.test3, size_rgb16bits,  default);
                t1 = Performance("16 bits on Matrix-Shaper profiles", SpeedTest16bitsRGB,  noPlugin, TestProfiles.test5, TestProfiles.test0, size_rgb16bits,  default);
                t2 = Performance("16 bits on same Matrix-Shaper",     SpeedTest16bitsRGB,  noPlugin, TestProfiles.test0, TestProfiles.test0, size_rgb16bits,  default);
                t3 = Performance("16 bits on curves",                 SpeedTest16bitsRGB,  noPlugin, "*curves",     "*curves",   size_rgb16bits,  default);
                t4 = Performance("16 bits on CMYK CLUT profiles",     SpeedTest16bitsCMYK, noPlugin, TestProfiles.test1, TestProfiles.test2, size_cmyk16bits, default);
            }

            Thread.Sleep(10);
            Console.WriteLine();

            // Note that context null has the plug-in installed

            using (logger.BeginScope("Plugin"))
            {
                trace("P E R F O R M A N C E   T E S T S   1 6  B I T S  (P L U G I N)");

                Performance("16 bits on CLUT profiles",          SpeedTest16bitsRGB,  null, TestProfiles.test5, TestProfiles.test3, size_rgb16bits,  t0);
                Performance("16 bits on Matrix-Shaper profiles", SpeedTest16bitsRGB,  null, TestProfiles.test5, TestProfiles.test0, size_rgb16bits,  t1);
                Performance("16 bits on same Matrix-Shaper",     SpeedTest16bitsRGB,  null, TestProfiles.test0, TestProfiles.test0, size_rgb16bits,  t2);
                Performance("16 bits on curves",                 SpeedTest16bitsRGB,  null, "*curves",     "*curves",   size_rgb16bits,  t3);
                Performance("16 bits on CMYK CLUT profiles",     SpeedTest16bitsCMYK, null, TestProfiles.test1, TestProfiles.test2, size_cmyk16bits, t4);
            }
        }

        cmsDeleteContext(noPlugin);
    }

    private static TimeSpan SpeedTestFloatRGB(Context? ct, Profile profileIn, Profile profileOut)
    {
        if (profileIn is null || profileOut is null)
            Fail("Unable to open profiles");

        var inFormatter = 0u;
        switch ((uint)cmsGetColorSpace(profileIn))
        {
            case cmsSigRgbData: inFormatter = TYPE_RGB_FLT; break;
            case cmsSigLabData: inFormatter = TYPE_Lab_FLT; break;

            default:
                Fail("Invalid colorspace");
                break;
        }

        var outFormatter = 0u;
        switch ((uint)cmsGetColorSpace(profileOut))
        {
            case cmsSigRgbData: outFormatter = TYPE_RGB_FLT; break;
            case cmsSigLabData: outFormatter = TYPE_Lab_FLT; break;
            case cmsSigXYZData: outFormatter = TYPE_XYZ_FLT; break;

            default:
                Fail("Invalid colorspace");
                break;
        }

        var lcmsxform = cmsCreateTransformTHR(ct, profileIn, inFormatter, profileOut, outFormatter, INTENT_PERCEPTUAL, cmsFLAGS_NOCACHE)!;
        cmsCloseProfile(profileIn);
        cmsCloseProfile(profileOut);

        if (inFormatter == TYPE_RGB_FLT)
        {
            var Mb = 256 * 256 * 256;
            var fill = new Scanline_rgbFloat[Mb];

            var j = 0;
            for (var r = 0; r < 256; r++)
            {
                for (var g = 0; g < 256; g++)
                {
                    for (var b = 0; b < 256; b++)
                    {
                        fill[j].r = r / 255.0f;
                        fill[j].g = g / 255.0f;
                        fill[j].b = b / 255.0f;

                        j++;
                    }
                }
            }

            var atime = Stopwatch.StartNew();

            cmsDoTransform(lcmsxform, fill, fill, (uint)Mb);

            atime.Stop();

            cmsDeleteTransform(lcmsxform);

            return atime.Elapsed;
        }
        else
        {
            var Mb = 100 * 256 * 256;
            var fill = new Scanline_LabFloat[Mb];

            var j = 0;
            for (var L = 0; L < 100; L++)
            {
                for (var a = -127.0f; a < 127.0f; a++)
                {
                    for (var b = -127.0f; b < 127.0f; b++)
                    {
                        fill[j].L = L;
                        fill[j].a = a;
                        fill[j].b = b;

                        j++;
                    }
                }
            }

            var atime = Stopwatch.StartNew();

            cmsDoTransform(lcmsxform, fill, fill, (uint)Mb);

            atime.Stop();

            cmsDeleteTransform(lcmsxform);

            return atime.Elapsed;
        }
    }

    private static TimeSpan SpeedTestFloatCMYK(Context? ct, Profile profileIn, Profile profileOut)
    {
        if (profileIn is null || profileOut is null)
            Fail("Unable to open profiles");

        var lcmsxform = cmsCreateTransformTHR(ct, profileIn, TYPE_CMYK_FLT, profileOut, TYPE_CMYK_FLT, INTENT_PERCEPTUAL, cmsFLAGS_NOCACHE)!;
        cmsCloseProfile(profileIn);
        cmsCloseProfile(profileOut);

        var Mb = 64 * 64 * 64 * 64;
        var In = new Scanline_cmykFloat[Mb];

        var j = 0;
        for (var c = 0; c < 256; c += 4)
        {
            for (var m = 0; m < 256; m += 4)
            {
                for (var y = 0; y < 256; y += 4)
                {
                    for (var k = 0; k < 256; k += 4)
                    {
                        In[j].c = (ushort)c;
                        In[j].m = (ushort)m;
                        In[j].y = (ushort)y;
                        In[j].k = (ushort)k;

                        j++;
                    }
                }
            }
        }

        var atime = Stopwatch.StartNew();

        cmsDoTransform(lcmsxform, In, In, (uint)Mb);

        atime.Stop();

        cmsDeleteTransform(lcmsxform);

        return atime.Elapsed;
    }

    private static TimeSpan SpeedTestFloatLab(Context? ct, Profile profileIn, Profile profileOut)
    {
        if (profileIn is null || profileOut is null)
            Fail("Unable to open profiles");

        if ((uint)cmsGetColorSpace(profileIn) is not cmsSigLabData)
            Fail("Invalid colorspace");

        var outFormatter = 0u;
        switch ((uint)cmsGetColorSpace(profileOut))
        {
            case cmsSigRgbData: outFormatter = TYPE_RGB_FLT; break;
            case cmsSigLabData: outFormatter = TYPE_Lab_FLT; break;
            case cmsSigXYZData: outFormatter = TYPE_XYZ_FLT; break;

            default:
                Fail("Invalid colorspace");
                break;
        }

        var lcmsxform = cmsCreateTransformTHR(ct, profileIn, TYPE_Lab_FLT, profileOut, outFormatter, INTENT_PERCEPTUAL, cmsFLAGS_NOCACHE)!;
        cmsCloseProfile(profileIn);
        cmsCloseProfile(profileOut);

        var Mb = 100 * 256 * 256;
        var fill = new Scanline_LabFloat[Mb];

        var j = 0;
        for (var L = 0; L < 100; L++)
        {
            for (var a = -127.0f; a < 127.0f; a++)
            {
                for (var b = -127.0f; b < 127.0f; b++)
                {
                    fill[j].L = L;
                    fill[j].a = a;
                    fill[j].b = b;

                    j++;
                }
            }
        }

        var atime = Stopwatch.StartNew();

        cmsDoTransform(lcmsxform, fill, fill, (uint)Mb);

        atime.Stop();

        cmsDeleteTransform(lcmsxform);

        return atime.Elapsed;
    }

    public static void SpeedTestFloat()
    {
        var size_rgbFloat = Unsafe.SizeOf<Scanline_rgbFloat>();
        var size_cmykFloat = Unsafe.SizeOf<Scanline_cmykFloat>();
        var size_LabFloat = Unsafe.SizeOf<Scanline_LabFloat>();

        var noPlugin = cmsCreateContext();
        Thread.Sleep(10);
        Console.WriteLine();

        TimeSpan t0, t1, t2, t3, t4, t5, t6, t7;

        using (logger.BeginScope("Floating point performance"))
        {
            using (logger.BeginScope("Default"))
            {
                trace("P E R F O R M A N C E   T E S T S   F L O A T  (D E F A U L T)");

                t0 = Performance("Floating point on CLUT profiles",          SpeedTestFloatRGB,  noPlugin, TestProfiles.test5, TestProfiles.test3, size_rgbFloat, default);
                t1 = Performance("Floating point on Matrix-Shaper profiles", SpeedTestFloatRGB,  noPlugin, TestProfiles.test5, TestProfiles.test0, size_rgbFloat, default);
                t2 = Performance("Floating point on same Matrix-Shaper",     SpeedTestFloatRGB,  noPlugin, TestProfiles.test0, TestProfiles.test0, size_rgbFloat, default);
                t3 = Performance("Floating point on curves",                 SpeedTestFloatRGB,  noPlugin, "*curves", "*curves", size_rgbFloat, default);
                t4 = Performance("Floating point on RGB->Lab",               SpeedTestFloatRGB,  noPlugin, TestProfiles.test5, "*lab", size_rgbFloat, default);
                t5 = Performance("Floating point on RGB->XYZ",               SpeedTestFloatRGB,  noPlugin, TestProfiles.test3, "*xyz", size_rgbFloat, default);
                t6 = Performance("Floating point on CMYK->CMYK",             SpeedTestFloatCMYK, noPlugin, TestProfiles.test1, TestProfiles.test2, size_cmykFloat, default);
                t7 = Performance("Floating point on Lab->RGB",               SpeedTestFloatLab,  noPlugin, "*lab", TestProfiles.test3, size_LabFloat, default);
            }

            Thread.Sleep(10);
            Console.WriteLine();

            // Note that context null has the plug-in installed

            using (logger.BeginScope("Plugin"))
            {
                trace("P E R F O R M A N C E   T E S T S  F L O A T  (P L U G I N)");

                Performance("Floating point on CLUT profiles",          SpeedTestFloatRGB,  null, TestProfiles.test5, TestProfiles.test3, size_rgbFloat, t0);
                Performance("Floating point on Matrix-Shaper profiles", SpeedTestFloatRGB,  null, TestProfiles.test5, TestProfiles.test0, size_rgbFloat, t1);
                Performance("Floating point on same Matrix-Shaper",     SpeedTestFloatRGB,  null, TestProfiles.test0, TestProfiles.test0, size_rgbFloat, t2);
                Performance("Floating point on curves",                 SpeedTestFloatRGB,  null, "*curves",     "*curves", size_rgbFloat, t3);
                Performance("Floating point on RGB->Lab",               SpeedTestFloatRGB,  null, TestProfiles.test5, "*lab", size_rgbFloat, t4);
                Performance("Floating point on RGB->XYZ",               SpeedTestFloatRGB,  null, TestProfiles.test3, "*xyz", size_rgbFloat, t5);
                Performance("Floating point on CMYK->CMYK",             SpeedTestFloatCMYK, null, TestProfiles.test1, TestProfiles.test2, size_cmykFloat, t6);
                Performance("Floating point on Lab->RGB",               SpeedTestFloatLab,  null, "*lab",        TestProfiles.test3, size_LabFloat, t7);
            }
        }

        cmsDeleteContext(noPlugin);
    }
}
