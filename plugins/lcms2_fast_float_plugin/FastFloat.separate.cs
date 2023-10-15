//---------------------------------------------------------------------------------
//
//  Little Color Management System, fast floating point extensions
//  Copyright (c) 1998-2023 Marti Maria Saguer, all rights reserved
//                2022-2023 Stefan Kewatt, all rights reserved
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
namespace lcms2.FastFloatPlugin;
public static partial class FastFloat
{
    // Separable input. It just computes the distance from 
    // each component to the next one in bytes. It gives components RGB in this order
    // 
    // Encoding  Starting      Increment   DoSwap   Swapfirst Extra 
    // RGB,       012            333          0         0       0   
    // RGBA,      012            444          0         0       1   
    // ARGB,      123            444          0         1       1   
    // BGR,       210            333          1         0       0   
    // BGRA,      210            444          1         1       1   
    // ABGR       321            444          1         0       1   
    //
    //
    //  On planar configurations, the distance is the stride added to any non-negative
    //
    //  RGB       0, S, 2*S      111
    //  RGBA      0, S, 2*S      111    (fourth plane is safely ignored)
    //  ARGB      S, 2*S, 3*S    111
    //  BGR       2*S, S, 0      111
    //  BGRA      2*S, S, 0,     111    (fourth plane is safely ignored)
    //  ABGR      3*S, 2*S, S    111
    //

    private static int trueBytesSize(uint Format)
    {
        var fmt_bytes = T_BYTES(Format);

        // For double, the T_BYTES field returns zero
        if (fmt_bytes is 0)
            return sizeof(double);

        // Otherwise, it is already correct for all formats
        return fmt_bytes;
    }

    private static void ComputeIncrementsForChunky(uint Format,
                                                   uint _,
                                                   out uint nChannels,
                                                   out uint nAlpha,
                                                   Span<uint> ComponentStartingOrder,
                                                   Span<uint> ComponentPointerIncrements)
    {
        // RGBA -> normal
        // ARGB -> swap first
        // ABGR -> doSwap
        // BGRA -> doSwap swapFirst

        var extra = T_EXTRA(Format);
        var channels = T_CHANNELS(Format);
        var total_chans = channels + extra;
        var channelSize = trueBytesSize(Format);
        var pixelSize = channelSize * total_chans;

        // Setup the counts
        nChannels = (uint)channels;

        nAlpha = (uint)extra;

        // Separation is independent of starting point and only depends on channel size
        for (var i = 0; i < total_chans; i++)
            ComponentPointerIncrements[i] = (uint)pixelSize;

        // Handle do swap
        for (var i = 0; i < total_chans; i++)
        {
            ComponentStartingOrder[i] =
                T_DOSWAP(Format) is not 0
                    ? (uint)(total_chans - i - 1)
                    : (uint)i;
        }

        // Handle swap first (ROL of positions), example CMYK -> KCMY | 0123 -> 3012
        if (T_SWAPFIRST(Format) is not 0)
        {
            var tmp = ComponentStartingOrder[0];
            for (var i = 0; i < total_chans - 1; i++)
                ComponentStartingOrder[i] = ComponentStartingOrder[i + 1];

            ComponentStartingOrder[total_chans - 1] = tmp;
        }

        // Handle size
        if (channelSize > 1)
        {
            for (var i = 0; i < total_chans; i++)
                ComponentStartingOrder[i] *= (uint)channelSize;
        }
    }

    private static void ComputeIncrementsForPlanar(uint Format,
                                                   uint BytesPerPlane,
                                                   out uint nChannels,
                                                   out uint nAlpha,
                                                   Span<uint> ComponentStartingOrder,
                                                   Span<uint> ComponentPointerIncrements)
    {
        var extra = T_EXTRA(Format);
        var channels = T_CHANNELS(Format);
        var total_chans = channels + extra;
        var channelSize = trueBytesSize(Format);

        // Setup the counts
        nChannels = (uint)channels;

        nAlpha = (uint)extra;

        // Separation is independent of starting point and only depends on channel size
        for (var i = 0; i < total_chans; i++)
            ComponentPointerIncrements[i] = (uint)channelSize;

        // Handle do swap
        for (var i = 0; i < total_chans; i++)
        {
            ComponentStartingOrder[i] =
                T_DOSWAP(Format) is not 0
                    ? (uint)(total_chans - i - 1)
                    : (uint)i;
        }

        // Handle swap first (ROL of positions), example CMYK -> KCMY | 0123 -> 3012
        if (T_SWAPFIRST(Format) is not 0)
        {
            var tmp = ComponentStartingOrder[0];
            for (var i = 0; i < total_chans - 1; i++)
                ComponentStartingOrder[i] = ComponentStartingOrder[i + 1];

            ComponentStartingOrder[total_chans - 1] = tmp;
        }

        // Handle size
        for (var i = 0; i < total_chans; i++)
            ComponentStartingOrder[i] *= BytesPerPlane;
    }

    internal static void _cmsComputeComponentIncrements(uint Format,
                                                        uint BytesPerPlane,
                                                        out uint nChannels,
                                                        out uint nAlpha,
                                                        Span<uint> ComponentStartingOrder,
                                                        Span<uint> ComponentPointerIncrements)
    {
        if (T_PLANAR(Format) is not 0)
        {
            ComputeIncrementsForPlanar(Format, BytesPerPlane, out nChannels, out nAlpha, ComponentStartingOrder, ComponentPointerIncrements);
        }
        else
        {
            ComputeIncrementsForChunky(Format, BytesPerPlane, out nChannels, out nAlpha, ComponentStartingOrder, ComponentPointerIncrements);
        }
    }
}
