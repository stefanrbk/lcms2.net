using lcms2.plugins;

namespace lcms2.state.chunks;

internal class InterpPlugin
{
    private InterpFnFactory? interpolators = null;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        var from = src is not null ? (InterpPlugin?)src.chunks[(int)Chunks.InterpPlugin] : interpPluginChunk;

        ctx.chunks[(int)Chunks.InterpPlugin] = from;
    }

    private InterpPlugin() { }

    internal static InterpPlugin global = new() { interpolators = null };
    private readonly static InterpPlugin interpPluginChunk = new() { interpolators = null };

    internal static InterpFunction DefaultInterpolatorsFactory(int _numInputChannels, int _numOutputChannels, LerpFlag _flags) =>
        throw new NotImplementedException();
}
