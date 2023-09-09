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
using Microsoft.Extensions.Logging;

namespace lcms2.FastFloatPlugin.testbed;
internal static partial class Testbed
{
    private static bool checkSingleComputeIncrements(uint Format, uint planeStride, uint ExpectedChannels, uint ExpectedAlpha, params uint[] args)
    {
        Span<uint> ComponentStartingOrder = stackalloc uint[cmsMAXCHANNELS];
        Span<uint> ComponentPointerIncrements = stackalloc uint[cmsMAXCHANNELS];

        _cmsComputeComponentIncrements(Format, planeStride, out var nChannels, out var nAlpha, ComponentStartingOrder, ComponentPointerIncrements);

        if (nChannels != ExpectedChannels ||
            nAlpha != ExpectedAlpha)
        {
            return false;
        }

        var nTotal = nAlpha + nChannels;

        var argIndex = 0;
        for (var i = 0; i < nTotal; i++)
        {
            var so = args[argIndex++];
            if (so != ComponentStartingOrder[i])
                return false;
        }

        for (var i = 0; i < nTotal; i++)
        {
            var so = args[argIndex++];
            if (so != ComponentPointerIncrements[i])
                return false;
        }

        return true;
    }
    public static bool CheckComputeIncrements()
    {
        using (logger.BeginScope("Check compute increments"))
        {
            CHECK(nameof(TYPE_GRAY_8), 0, 1, 0, /**/ 0,    /**/ 1);
            CHECK(nameof(TYPE_GRAYA_8), 0, 1, 1, /**/ 0, 1, /**/ 2, 2);
            CHECK(nameof(TYPE_AGRAY_8), 0, 1, 1, /**/ 1, 0, /**/ 2, 2);
            CHECK(nameof(TYPE_GRAY_16), 0, 1, 0, /**/ 0,    /**/ 2);
            CHECK(nameof(TYPE_GRAYA_16), 0, 1, 1, /**/ 0, 2, /**/ 4, 4);
            CHECK(nameof(TYPE_AGRAY_16), 0, 1, 1, /**/ 2, 0, /**/ 4, 4);

            CHECK(nameof(TYPE_GRAY_FLT), 0, 1, 0, /**/ 0,    /**/ 4);
            CHECK(nameof(TYPE_GRAYA_FLT), 0, 1, 1, /**/ 0, 4, /**/ 8, 8);
            CHECK(nameof(TYPE_AGRAY_FLT), 0, 1, 1, /**/ 4, 0, /**/ 8, 8);

            CHECK(nameof(TYPE_GRAY_DBL), 0, 1, 0, /**/ 0,      /**/ 8);
            CHECK(nameof(TYPE_AGRAY_DBL), 0, 1, 1, /**/ 8, 0,   /**/ 16, 16);

            CHECK(nameof(TYPE_RGB_8), 0, 3, 0, /**/ 0, 1, 2,     /**/ 3, 3, 3);
            CHECK(nameof(TYPE_RGBA_8), 0, 3, 1, /**/ 0, 1, 2, 3,  /**/ 4, 4, 4, 4);
            CHECK(nameof(TYPE_ARGB_8), 0, 3, 1, /**/ 1, 2, 3, 0,  /**/ 4, 4, 4, 4);

            CHECK(nameof(TYPE_RGB_16), 0, 3, 0, /**/ 0, 2, 4,     /**/ 6, 6, 6);
            CHECK(nameof(TYPE_RGBA_16), 0, 3, 1, /**/ 0, 2, 4, 6,  /**/ 8, 8, 8, 8);
            CHECK(nameof(TYPE_ARGB_16), 0, 3, 1, /**/ 2, 4, 6, 0,  /**/ 8, 8, 8, 8);

            CHECK(nameof(TYPE_RGB_FLT), 0, 3, 0, /**/ 0, 4, 8,     /**/ 12, 12, 12);
            CHECK(nameof(TYPE_RGBA_FLT), 0, 3, 1, /**/ 0, 4, 8, 12,  /**/ 16, 16, 16, 16);
            CHECK(nameof(TYPE_ARGB_FLT), 0, 3, 1, /**/ 4, 8, 12, 0,  /**/ 16, 16, 16, 16);

            CHECK(nameof(TYPE_BGR_8), 0, 3, 0, /**/ 2, 1, 0,     /**/ 3, 3, 3);
            CHECK(nameof(TYPE_BGRA_8), 0, 3, 1, /**/ 2, 1, 0, 3,  /**/ 4, 4, 4, 4);
            CHECK(nameof(TYPE_ABGR_8), 0, 3, 1, /**/ 3, 2, 1, 0,  /**/ 4, 4, 4, 4);

            CHECK(nameof(TYPE_BGR_16), 0, 3, 0, /**/ 4, 2, 0,     /**/ 6, 6, 6);
            CHECK(nameof(TYPE_BGRA_16), 0, 3, 1, /**/ 4, 2, 0, 6,  /**/ 8, 8, 8, 8);
            CHECK(nameof(TYPE_ABGR_16), 0, 3, 1, /**/ 6, 4, 2, 0,  /**/ 8, 8, 8, 8);

            CHECK(nameof(TYPE_BGR_FLT), 0, 3, 0,  /**/ 8, 4, 0,     /**/  12, 12, 12);
            CHECK(nameof(TYPE_BGRA_FLT), 0, 3, 1, /**/ 8, 4, 0, 12,  /**/ 16, 16, 16, 16);
            CHECK(nameof(TYPE_ABGR_FLT), 0, 3, 1, /**/ 12, 8, 4, 0,  /**/ 16, 16, 16, 16);


            CHECK(nameof(TYPE_CMYK_8), 0, 4, 0, /**/ 0, 1, 2, 3,     /**/ 4, 4, 4, 4);
            CHECK(nameof(TYPE_CMYKA_8), 0, 4, 1, /**/ 0, 1, 2, 3, 4,  /**/ 5, 5, 5, 5, 5);
            CHECK(nameof(TYPE_ACMYK_8), 0, 4, 1, /**/ 1, 2, 3, 4, 0,  /**/ 5, 5, 5, 5, 5);

            CHECK(nameof(TYPE_KYMC_8), 0, 4, 0, /**/ 3, 2, 1, 0,     /**/ 4, 4, 4, 4);
            CHECK(nameof(TYPE_KYMCA_8), 0, 4, 1, /**/ 3, 2, 1, 0, 4,  /**/ 5, 5, 5, 5, 5);
            CHECK(nameof(TYPE_AKYMC_8), 0, 4, 1, /**/ 4, 3, 2, 1, 0,  /**/ 5, 5, 5, 5, 5);

            CHECK(nameof(TYPE_KCMY_8), 0, 4, 0, /**/ 1, 2, 3, 0,      /**/ 4, 4, 4, 4);

            CHECK(nameof(TYPE_CMYK_16), 0, 4, 0, /**/ 0, 2, 4, 6,      /**/ 8, 8, 8, 8);
            CHECK(nameof(TYPE_CMYKA_16), 0, 4, 1, /**/ 0, 2, 4, 6, 8,  /**/ 10, 10, 10, 10, 10);
            CHECK(nameof(TYPE_ACMYK_16), 0, 4, 1, /**/ 2, 4, 6, 8, 0,  /**/ 10, 10, 10, 10, 10);

            CHECK(nameof(TYPE_KYMC_16), 0, 4, 0,  /**/ 6, 4, 2, 0,     /**/ 8, 8, 8, 8);
            CHECK(nameof(TYPE_KYMCA_16), 0, 4, 1, /**/ 6, 4, 2, 0, 8,  /**/ 10, 10, 10, 10, 10);
            CHECK(nameof(TYPE_AKYMC_16), 0, 4, 1, /**/ 8, 6, 4, 2, 0,  /**/ 10, 10, 10, 10, 10);

            CHECK(nameof(TYPE_KCMY_16), 0, 4, 0, /**/ 2, 4, 6, 0,      /**/ 8, 8, 8, 8);

            // Planar

            CHECK(nameof(TYPE_GRAYA_8_PLANAR), 100, 1, 1, /**/ 0, 100,  /**/ 1, 1);
            CHECK(nameof(TYPE_AGRAY_8_PLANAR), 100, 1, 1, /**/ 100, 0,  /**/ 1, 1);

            CHECK(nameof(TYPE_GRAYA_16_PLANAR), 100, 1, 1, /**/ 0, 100,   /**/ 2, 2);
            CHECK(nameof(TYPE_AGRAY_16_PLANAR), 100, 1, 1, /**/ 100, 0,   /**/ 2, 2);

            CHECK(nameof(TYPE_GRAYA_FLT_PLANAR), 100, 1, 1, /**/ 0, 100,   /**/ 4, 4);
            CHECK(nameof(TYPE_AGRAY_FLT_PLANAR), 100, 1, 1, /**/ 100, 0,   /**/ 4, 4);

            CHECK(nameof(TYPE_GRAYA_DBL_PLANAR), 100, 1, 1, /**/ 0, 100,   /**/ 8, 8);
            CHECK(nameof(TYPE_AGRAY_DBL_PLANAR), 100, 1, 1, /**/ 100, 0,   /**/ 8, 8);

            CHECK(nameof(TYPE_RGB_8_PLANAR), 100, 3, 0, /**/ 0, 100, 200,      /**/ 1, 1, 1);
            CHECK(nameof(TYPE_RGBA_8_PLANAR), 100, 3, 1, /**/ 0, 100, 200, 300, /**/ 1, 1, 1, 1);
            CHECK(nameof(TYPE_ARGB_8_PLANAR), 100, 3, 1, /**/ 100, 200, 300, 0,  /**/ 1, 1, 1, 1);

            CHECK(nameof(TYPE_BGR_8_PLANAR), 100, 3, 0, /**/ 200, 100, 0,       /**/ 1, 1, 1);
            CHECK(nameof(TYPE_BGRA_8_PLANAR), 100, 3, 1, /**/ 200, 100, 0, 300,  /**/ 1, 1, 1, 1);
            CHECK(nameof(TYPE_ABGR_8_PLANAR), 100, 3, 1, /**/ 300, 200, 100, 0,  /**/ 1, 1, 1, 1);

            CHECK(nameof(TYPE_RGB_16_PLANAR), 100, 3, 0, /**/ 0, 100, 200,      /**/ 2, 2, 2);
            CHECK(nameof(TYPE_RGBA_16_PLANAR), 100, 3, 1, /**/ 0, 100, 200, 300, /**/ 2, 2, 2, 2);
            CHECK(nameof(TYPE_ARGB_16_PLANAR), 100, 3, 1, /**/ 100, 200, 300, 0,  /**/ 2, 2, 2, 2);

            CHECK(nameof(TYPE_BGR_16_PLANAR), 100, 3, 0, /**/ 200, 100, 0,       /**/ 2, 2, 2);
            CHECK(nameof(TYPE_BGRA_16_PLANAR), 100, 3, 1, /**/ 200, 100, 0, 300,  /**/ 2, 2, 2, 2);
            CHECK(nameof(TYPE_ABGR_16_PLANAR), 100, 3, 1, /**/ 300, 200, 100, 0,  /**/ 2, 2, 2, 2);

            return true;
        }

        static bool CHECK(string frm, uint plane, uint chans, uint alpha, params uint[] args)
        {
            using (logger.BeginScope("{frm}", frm))
            {
                var field = typeof(FastFloat).GetProperty(frm) ?? typeof(Lcms2).GetProperty(frm);
                var value = (uint)field!.GetValue(null)!;

                if (!checkSingleComputeIncrements(value, plane, chans, alpha, args))
                {
                    logger.LogError("Format failed!");
                    return false;
                }
                return true;
            }
        }
    }
}
