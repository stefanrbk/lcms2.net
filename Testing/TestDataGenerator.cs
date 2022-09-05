using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.types;

namespace lcms2.testing;
public static class TestDataGenerator
{
    #region Fields

    private static readonly ushort[][] _checkXDData =
    {
        new ushort[] { 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000 },
        new ushort[] { 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF, 0x0000, 0xFFFF, 0xFFFF, 0xFFFF },
        new ushort[] { 0x8080, 0x8080, 0x8080, 0x8080, 0x1234, 0x1122, 0x0056, 0x0011 },
        new ushort[] { 0x0000, 0xFE00, 0x80FF, 0x8888, 0x8878, 0x2233, 0x0088, 0x2020 },
        new ushort[] { 0x1111, 0x2222, 0x3333, 0x4444, 0x1455, 0x3344, 0x1987, 0x4532 },
        new ushort[] { 0x0000, 0x0012, 0x0013, 0x0014, 0x2333, 0x4455, 0x9988, 0x1200 },
        new ushort[] { 0x3141, 0x1415, 0x1592, 0x9261, 0x4567, 0x5566, 0xFE56, 0x6666 },
        new ushort[] { 0xFF00, 0xFF01, 0xFF12, 0xFF13, 0xF344, 0x6677, 0xBABE, 0xFACE },
    };

    private static readonly uint[][] _checkXDDims =
    {
        new uint[] { 7, 8, 9 },
        new uint[] { 9, 8, 7, 6 },
        new uint[] { 3, 2, 2, 2, 2 },
        new uint[] { 4, 3, 3, 2, 2, 2 },
        new uint[] { 4, 3, 3, 2, 2, 2, 2 },
        new uint[] { 4, 3, 3, 2, 2, 2, 2, 2 }
    };

    #endregion Fields

    #region Public Methods

    public static IEnumerable<object[]> Check1D()
    {
        yield return new object[] { 2u, false, 0 };
        yield return new object[] { 3u, false, 1 };
        yield return new object[] { 4u, false, 0 };
        yield return new object[] { 6u, false, 0 };
        yield return new object[] { 18u, false, 0 };
        yield return new object[] { 2u, true, 0 };
        yield return new object[] { 3u, true, 1 };
        yield return new object[] { 6u, true, 0 };
        yield return new object[] { 18u, true, 0 };
    }

    public static IEnumerable<object[]> CheckXD(int x)
    {
        for (var i = 0; i < _checkXDData.Length; i++)
            yield return new object[] { (uint)x, _checkXDData[i][..x] };
    }

    public static IEnumerable<object[]> CheckXDGranular(int x)
    {
        for (var i = 0; i < _checkXDData.Length; i++)
            yield return new object[] { _checkXDDims[x - 3], (uint)x, _checkXDData[i][..x] };
    }

    public static IEnumerable<object[]> ExhaustiveCheck1D()
    {
        for (uint i = 0, j = 1; i < 16; i++, j++)
            yield return new object[] { i == 0 ? 10 : (i * 256), j * 256 };
    }

    public static IEnumerable<object[]> ExhaustiveCheck3D()
    {
        for (uint i = 0, j = 1; i < 16; i++, j++)
            yield return new object[] { i * 16, j * 16 };
    }

    #endregion Public Methods
}