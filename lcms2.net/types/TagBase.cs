using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace lcms2.types;


#if PLUGIN
    public
#else
internal
#endif
        record TagBase(Signature Sig, byte[] Reserved)
{
}
