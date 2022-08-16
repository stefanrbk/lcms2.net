using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lcms2.it8;
public class KeyValue
{
    public KeyValue? Next;
    public string Keyword;          // Name of variable
    public KeyValue? NextSubkey;    // If key is a dictionary, points to the next item
    public object Value;            // Points to value
    WriteMode WriteAs;              // How to write the value
}
