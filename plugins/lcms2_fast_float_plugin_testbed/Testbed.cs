//---------------------------------------------------------------------------------
//
//  Little Color Management System, fast floating point extensions
//  Copyright (c) 1998-2022 Marti Maria Saguer, all rights reserved
//  Copyright (c) 2022-2023 Stefan Kewatt, all rights reserved
//
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//---------------------------------------------------------------------------------
using lcms2.testbed;
using lcms2.types;

using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace lcms2.FastFloatPlugin.testbed;
internal static partial class Testbed
{
    public static readonly ILoggerFactory factory = BuildDebugLogger();
    public static readonly ILogger logger = factory.CreateLogger<Program>();
    private static readonly List<(string name, int time)> testTimes = new();

    [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort FROM_8_TO_15(byte x8) =>
        (ushort)(((ulong)x8 << 15) / 0xFF);

    [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static byte FROM_15_TO_8(ushort x8) =>
        (byte)((((ulong)x8 * 0xFF) + 0x4000) >> 15);

    internal const double EPSILON_FLOAT_TESTS = 5e-3;

    public static ILoggerFactory BuildDebugLogger()
    {
        return LoggerFactory.Create(builder =>
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddTestBedFormatter(options => { options.IncludeScopes = true; options.SingleLine = true; }));
    }

    [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void trace(string frm, params object[] args) =>
        logger.LogInformation("{frm}", string.Format(frm, args));

    [DebuggerStepThrough, DoesNotReturn]
    public static void Fail(string message, params object[] args)
    {
        logger.LogError("{msg}", string.Format(message, args));
        Thread.Sleep(1000);
        Environment.Exit(1);
    }

    [DebuggerStepThrough, DoesNotReturn]
    public static void Fail(EventId eventId, string message, params object[] args)
    {
        logger.LogError(eventId, "{msg}", string.Format(message, args));
        Environment.Exit(1);
    }

    private static Profile CreateCurves()
    {
        var Gamma = cmsBuildGamma(null, 1.1)!;
        var Transfer = new ToneCurve[3] { Gamma, Gamma, Gamma };

        var h = cmsCreateLinearizationDeviceLink(cmsSigRgbData, Transfer);

        cmsFreeToneCurve(Gamma);

        return h!;
    }

    private static Profile loadProfile(string name)
    {
        if (name.StartsWith('*'))
        {
            if (name == "*lab")
            {
                return cmsCreateLab4Profile(null)!;
            }
            else if (name == "*xyz")
            {
                return cmsCreateXYZProfile()!;
            }
            else if (name == "*curves")
            {
                return CreateCurves();
            }
            else
            {
                Fail("Unknown builtin '{0}'", name);
            }
        }

        return cmsOpenProfileFromFile(name, "r")!;
    }

    private static Profile loadProfile(Memory<byte> mem) =>
        cmsOpenProfileFromMem(mem)!;
}
