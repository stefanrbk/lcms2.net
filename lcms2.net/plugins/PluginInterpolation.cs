using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.types;

namespace lcms2.plugins;

#if PLUGIN
    public
#else
internal
#endif
sealed class PluginInterpolation : PluginBase
{
    public InterpFnFactory? InterpolatorsFactory;

    public PluginInterpolation(Signature magic, uint expectedVersion, Signature type, InterpFnFactory? interpolatorsFactory)
        : base(magic, expectedVersion, type) =>
        InterpolatorsFactory = interpolatorsFactory;
}
