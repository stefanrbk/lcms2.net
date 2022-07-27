using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.state;

namespace lcms2.types;
public class Sequence
{
    public int Count;
    public Context Context;
    public ProfileSequenceDescriptor[] Seq;
}
