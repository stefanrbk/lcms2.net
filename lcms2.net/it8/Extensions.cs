using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lcms2.it8;
internal static class Extensions
{
    internal static KeyValue? Find(this List<KeyValue> list, string key, string? subkey) =>
        list.Find(k => k.Key == key && k.Subkey == subkey);

    internal static void Add(this List<KeyValue> list, string key, string? subkey, string? value, WriteMode mode)
    {
        if (list.Find(key, subkey) is null)
        {
            var kv = new KeyValue(key, subkey!, value!, mode);
            list.Add(kv);
        }
    }

    internal static int FindIndex<T>(this T[] array, Predicate<T> match)
    {
        for (var i = 0; i < array.Length; i++)
        {
            if (match(array[i]))
                return i;
        }

        return -1;
    }

    internal static void Cook(this List<Table> tables, Stack<StreamReader>? fileStack = null, int? lineNo = null)
    {
        for (var j = 0; j < tables.Count; j++)
        {
            var t = tables[j];

            t.sampleId = 0;

            for (var field = 0; field < t.NumSamples; field++)
            {
                if (t.dataFormat is null)
                {
                    if (fileStack is not null && lineNo is not null)
                        throw new IT8Exception(fileStack, lineNo.Value, "Undefined DATA_FORMAT");
                    else
                        throw new IT8Exception("Undefined DATA_FORMAT");
                }

                var fld = t.dataFormat[field];
                if (String.IsNullOrEmpty(fld)) continue;

                if (String.Compare(fld, "SAMPLE_ID") == 0)
                    t.sampleId = field;

                // "LABEL" is an extension. It keeps references to forward tables
                if (String.Compare(fld, "LABEL") == 0 || fld[0] == '$')
                {
                    // Search for table references...
                    for (var i = 0; i < t.NumPatches; i++)
                    {
                        var label = t.GetData(i, field);

                        if (!String.IsNullOrEmpty(label))
                        {
                            // This is the label, search for a table containing this property
                            for (var k = 0; k < tables.Count; k++)
                            {
                                var table = tables[k];

                                KeyValue? p;
                                if ((p = t.header.Find(label, null)) is not null)
                                {
                                    var type = p.Value;
                                    var numTable = k;

                                    var s = $"{label} {numTable} {type}";
                                    if (s.Length > 255)
                                        s = s[..255];
                                    t.SetData(i, field, s);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
