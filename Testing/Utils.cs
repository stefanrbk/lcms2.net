using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.it8;
using lcms2.state;
using lcms2.types;

namespace lcms2.testing;
public static class Utils
{
    public static void DumpToneCurve(ToneCurve gamma, string filename)
    {
        var it8 = new IT8();

        it8.SetPropertyDouble("NUMBER_OF_FIELDS", 2);
        it8.SetPropertyDouble("NUMBER_OF_SETS", gamma.NumEntries);

        it8.SetDataFormat(0, "SAMPLE_ID");
        it8.SetDataFormat(1, "VALUE");

        for (var i = 0; i < gamma.NumEntries; i++)
        {
            it8.SetDataRowCol(i, 0, i);
            it8.SetDataRowCol(i, 1, gamma.EstimatedTable[i]);
        }

        it8.SaveToFile(filename);
    }

    public static void IsGoodDouble(string title, double actual, double expected, double delta) =>
        Assert.That(actual, Is.EqualTo(expected).Within(delta), "({0}): Must be {1}, But is {2}", title, actual, expected);

}
