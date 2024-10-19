//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright ©️ 1998-2024 Marti Maria Saguer
//              2022-2024 Stefan Kewatt
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

using lcms2.state;
using lcms2.types;

using System.Runtime.CompilerServices;

namespace lcms2;

public static partial class Lcms2
{
    private static readonly InterpPluginChunkType InterpPluginChunk = new();
    private static readonly InterpPluginChunkType globalInterpPluginChunk = new();

    internal static void _cmsAllocInterpPluginChunk(Context ctx, in Context? src)
    {
        _cmsAssert(ctx);

        var from = src is not null
            ? src.InterpPlugin
            : InterpPluginChunk;

        _cmsAssert(from);

        ctx.InterpPlugin = (InterpPluginChunkType)((ICloneable)from).Clone();

        //fixed (InterpPluginChunkType* @default = &InterpPluginChunk)
        //    AllocPluginChunk(ctx, src, Chunks.InterpPlugin, @default);
    }

    internal static bool _cmsRegisterInterpPlugin(Context? ctx, PluginBase? Data)
    {
        var Plugin = (PluginInterpolation?)Data;
        var ptr = _cmsGetContext(ctx).InterpPlugin;

        if (Data is not null)
        {
            // Set replacement functions
            ptr.Interpolators = Plugin!.InterpolatorsFactory;
            return true;
        }
        else
        {
            ptr.Interpolators = null;
            return true;
        }
    }

    internal static InterpParams<T>? _cmsComputeInterpParamsEx<T>(
        Context? ContextID, ReadOnlySpan<uint> nSamples, uint InputChan, uint OutputChan, T[]? Table, LerpFlag flags) =>
        _cmsComputeInterpParamsEx(ContextID, nSamples, InputChan, OutputChan, Table?.AsMemory() ?? Memory<T>.Empty, flags);

    internal static InterpParams<T>? _cmsComputeInterpParamsEx<T>(
        Context? ContextID, ReadOnlySpan<uint> nSamples, uint InputChan, uint OutputChan, Memory<T> Table, LerpFlag flags)
    {
        var dwFlags = (uint)flags;

        // Check for maximum inputs
        if (InputChan > MAX_INPUT_DIMENSIONS)
        {
            cmsSignalError(ContextID, ErrorCodes.Range, $"Too many input channels ({InputChan} channels, max={MAX_INPUT_DIMENSIONS})");
            return null;
        }

        // Creates an empty object
        //var p = _cmsMallocZero<InterpParams>(ContextID);
        //if (p is null) return null;

        //p->dwFlags = dwFlags;
        //p->nInputs = InputChan;
        //p->nOutputs = OutputChan;
        //p->Table = Table;
        //p->ContextID = ContextID;

        var p = new InterpParams<T>(ContextID)
        {
            // Keep original parameters
            nInputs = InputChan,
            nOutputs = OutputChan,
            Table = Table,
            dwFlags = dwFlags
        };

        // Fill samples per input direction and domain (which is number of nodes minus one)
        for (var i = 0; i < InputChan; i++)
        {
            p.nSamples[i] = nSamples[i];
            p.Domain[i] = nSamples[i] - 1;
        }

        // Compute factors to apply to each component to index the grid array
        p.opta[0] = p.nOutputs;
        for (var i = 1; i < InputChan; i++)
            p.opta[i] = p.opta[i - 1] * nSamples[(int)InputChan - i];

        if (!_cmsSetInterpolationRoutine(ContextID, p))
        {
            cmsSignalError(ContextID, ErrorCodes.UnknownExtension, $"Unsupported interpolation ({InputChan}->{OutputChan} channels)");
            //_cmsFree(ContextID, p);
            p.Dispose();
            return null;
        }

        // All seems ok
        return p;
    }

    internal static InterpParams<T>? _cmsComputeInterpParams<T>(
        Context? ContextID, uint nSamples, uint InputChan, uint OutputChan, T[]? Table, LerpFlag flags) =>
        _cmsComputeInterpParams(ContextID, nSamples, InputChan, OutputChan, Table?.AsMemory() ?? Memory<T>.Empty, flags);

    internal static InterpParams<T>? _cmsComputeInterpParams<T>(
        Context? ContextID, uint nSamples, uint InputChan, uint OutputChan, Memory<T> Table, LerpFlag flags)
    {
        Span<uint> Samples = stackalloc uint[MAX_INPUT_DIMENSIONS];

        // Fill the auxiliary array
        for (var i = 0; i < MAX_INPUT_DIMENSIONS; i++)
            Samples[i] = nSamples;

        // Call the extended function
        return _cmsComputeInterpParamsEx<T>(ContextID, Samples, InputChan, OutputChan, Table, flags);
    }

    internal static void _cmsFreeInterpParams<T>(InterpParams<T>? p) =>
        //if (p is not null) _cmsFree(p.ContextID, p);
        p?.Dispose();
}
