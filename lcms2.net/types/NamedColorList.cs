using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.state;

namespace lcms2.types;
public class NamedColorList
{
    internal int NumColors;
    internal int Allocated;
    internal int ColorantCount;

    internal string Prefix;
    internal string Suffix;

    internal NamedColor? List;

    internal Context Context;
}
