using lcms2.plugins;

namespace lcms2.state;

internal class InterpPluginChunk
{
    private InterpFnFactory? interpolators = null;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        var from = src is not null ? (InterpPluginChunk?)src.chunks[(int)Chunks.InterpPlugin] : interpPluginChunk;

        ctx.chunks[(int)Chunks.InterpPlugin] = from;
    }

    private InterpPluginChunk() { }

    internal static InterpPluginChunk global = new() { interpolators = null };
    private readonly static InterpPluginChunk interpPluginChunk = new() { interpolators = null };

    internal static InterpFunction DefaultInterpolatorsFactory(int _numInputChannels, int _numOutputChannels, LerpFlag _flags) =>
        throw new NotImplementedException();
}
