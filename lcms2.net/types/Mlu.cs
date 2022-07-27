using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.state;

namespace lcms2.types;
public class Mlu
{
    internal Context Context;

    internal int AllocatedEntries;
    internal int UsedEntries;
    internal MluEntry[] Entries;

    internal int PoolSize;
    internal int PoolUsed;

    byte[] MemPool;
}

internal struct MluEntry
{
    public ushort Language;
    public ushort Country;

    public uint OffsetToStr;
    public uint Len;
}
