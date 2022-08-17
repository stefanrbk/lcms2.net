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
    public string? Subkey;          // If key is a dictionary, points to the subkey name
    public string? Value;           // Points to value
    public WriteMode WriteAs;       // How to write the value

    public static bool IsAvailableOnList(KeyValue? p, string key, string subkey, out KeyValue? lastPtr)
    {
        lastPtr = p;

        for (; p is not null; p = p.Next) {

            lastPtr = p;

            if (key[0] != '#') { // Comments are ignored
                if (string.Compare(key, p.Keyword) == 0)
                    break;
            }
        }

        if (p is null)
            return false;

        if (string.IsNullOrEmpty(subkey))
            return true;

        for (; p is not null; p = p.NextSubkey) {

            if (string.IsNullOrEmpty(p.Subkey)) continue;

            lastPtr = p;

            if (string.Compare(subkey, p.Subkey) == 0)
                return true;
        }

        return false;
    }

    public static KeyValue AddToList(ref KeyValue? head, string key, string subkey, string xValue, WriteMode writeAs)
    {
        // Check if property is already in list
        if (IsAvailableOnList(head, key, subkey, out var p)) {

            /* Oops, nothing here!*/

        } else {
            var last = p;

            // Allocate the container
            p = new KeyValue()
            {
                Keyword = key,
                Subkey = subkey
            };

            // Keep the container in out list
            if (head is null)
                head = p;
            else {
                if (!string.IsNullOrEmpty(subkey) && last is not null) {

                    last.NextSubkey = p;

                    // If subkey is not null, then last is the last property with the same key,
                    // but not necessarily is the last property in the list, so we need to move
                    // to the actual list end
                    while (last.Next is not null) {
                        last = last.Next;
                    }
                }

                if (last is not null) last.Next = p;
            }

            p.Next = null;
            p.NextSubkey = null;
        }

        p!.WriteAs = writeAs;

        p.Value = xValue;

        return p;
    }
}
