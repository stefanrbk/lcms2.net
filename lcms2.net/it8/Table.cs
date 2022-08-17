using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualBasic;

namespace lcms2.it8;
public class Table
{
    public string? SheetType;
    public int NumSamples, NumPatches;
    public KeyValue? HeaderList;
    public string[]? DataFormat;
    public string[]? Data;
}
