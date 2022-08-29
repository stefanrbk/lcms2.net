using lcms2.plugins;
using lcms2.state;
using lcms2.types;

namespace lcms2.testing;

[TestFixture(TestOf = typeof(InterpolationPlugin))]
public class InterpolationTests: ITest
{
    private object _state;

    [SetUp]
    public void Setup() =>
        _state = State.CreateStateContainer()!;

    [TearDown]
    public void Teardown() =>
        State.DeleteStateContainer(_state);

    // A single function that does check 1D interpolation
    // nNodesToCheck = number on nodes to check
    // Down = Create decreasing tables
    // Reverse = Check reverse interpolation
    // max_err = max allowed error
    [TestCase(2, false, 0, ExpectedResult = true)]
    [TestCase(3, false, 1, ExpectedResult = true)]
    [TestCase(4, false, 0, ExpectedResult = true)]
    [TestCase(6, false, 0, ExpectedResult = true)]
    [TestCase(18, false, 0, ExpectedResult = true)]
    [TestCase(2, true, 0, ExpectedResult = true)]
    [TestCase(3, true, 1, ExpectedResult = true)]
    [TestCase(6, true, 0, ExpectedResult = true)]
    [TestCase(18, true, 0, ExpectedResult = true)]
    public bool Check1DTest(int numNodesToCheck, bool down, int maxErr)
    {
        var tab = new ushort[numNodesToCheck];

        var p = InterpParams.Compute(_state, numNodesToCheck, 1, 1, tab, LerpFlag.Ushort);
        if (p is null) return false;

        BuildTable(numNodesToCheck, ref tab, down);

        for (var i = 0; i <= 0xFFFF; i++)
        {
            var @in = new ushort[] { (ushort)i };
            var @out = new ushort[1];

            p.Interpolation.Lerp16(in @in, ref @out, p);

            if (down) @out[0] = (ushort)(0xFFFF - @out[0]);

            Assert.That(@out[0], Is.EqualTo(@in[0]).Within(maxErr), $"({numNodesToCheck}): Must be {@in[0]}, but is {@out[0]} : ");
        }

        return true;
    }

    [Explicit("Exhaustive test")]
    [TestCase(10, 256 * 1, ExpectedResult = true)]
    [TestCase(256 * 1, 256 * 2, ExpectedResult = true)]
    [TestCase(256 * 2, 256 * 3, ExpectedResult = true)]
    [TestCase(256 * 3, 256 * 4, ExpectedResult = true)]
    [TestCase(256 * 4, 256 * 5, ExpectedResult = true)]
    [TestCase(256 * 5, 256 * 6, ExpectedResult = true)]
    [TestCase(256 * 6, 256 * 7, ExpectedResult = true)]
    [TestCase(256 * 7, 256 * 8, ExpectedResult = true)]
    [TestCase(256 * 8, 256 * 9, ExpectedResult = true)]
    [TestCase(256 * 9, 256 * 10, ExpectedResult = true)]
    [TestCase(256 * 10, 256 * 11, ExpectedResult = true)]
    [TestCase(256 * 11, 256 * 12, ExpectedResult = true)]
    [TestCase(256 * 12, 256 * 13, ExpectedResult = true)]
    [TestCase(256 * 13, 256 * 14, ExpectedResult = true)]
    [TestCase(256 * 14, 256 * 15, ExpectedResult = true)]
    [TestCase(256 * 15, 256 * 16, ExpectedResult = true)]
    public bool ExhaustiveCheck1DTest(int start, int stop) =>
        ExhaustiveCheck1D(Check1DTest, false, start, stop);

    [Explicit("Exhaustive test")]
    [TestCase(10, 256 * 1, ExpectedResult = true)]
    [TestCase(256 * 1, 256 * 2, ExpectedResult = true)]
    [TestCase(256 * 2, 256 * 3, ExpectedResult = true)]
    [TestCase(256 * 3, 256 * 4, ExpectedResult = true)]
    [TestCase(256 * 4, 256 * 5, ExpectedResult = true)]
    [TestCase(256 * 5, 256 * 6, ExpectedResult = true)]
    [TestCase(256 * 6, 256 * 7, ExpectedResult = true)]
    [TestCase(256 * 7, 256 * 8, ExpectedResult = true)]
    [TestCase(256 * 8, 256 * 9, ExpectedResult = true)]
    [TestCase(256 * 9, 256 * 10, ExpectedResult = true)]
    [TestCase(256 * 10, 256 * 11, ExpectedResult = true)]
    [TestCase(256 * 11, 256 * 12, ExpectedResult = true)]
    [TestCase(256 * 12, 256 * 13, ExpectedResult = true)]
    [TestCase(256 * 13, 256 * 14, ExpectedResult = true)]
    [TestCase(256 * 14, 256 * 15, ExpectedResult = true)]
    [TestCase(256 * 15, 256 * 16, ExpectedResult = true)]
    public bool ExhaustiveCheck1DDownTest(int start, int stop) =>
        ExhaustiveCheck1D(Check1DTest, true, start, stop);

    private static bool ExhaustiveCheck1D(Func<int, bool, int, bool> fn, bool down, int start, int stop)
    {
        Console.WriteLine();
        Console.WriteLine($"\tTesting {start} - {stop}");
        var startStr = start.ToString();
        var stopStr = stop.ToString();
        var width = Console.BufferWidth - 16 - startStr.Length - stopStr.Length;

        Assert.Multiple(() =>
        {
            ClearAssert();

            for (var j = start; j <= stop; j++)
            {
                ProgressBar(start, stop, width, j);

                fn(j, down, 1);
            }
        });

        Console.Write($"\r{new string(' ', Console.BufferWidth - 1)}\r\t\tDone.");
        return true;
    }

    // Since prime factors of 65535 (FFFF) are,
    //
    //            0xFFFF = 3 * 5 * 17 * 257
    //
    // I test tables of 2, 4, 6, and 18 points, that will be exact.
    private static void BuildTable(int n, ref ushort[] tab, bool descending)
    {
        for (var i = 0; i < n; i++)
        {
            var v = 65535.0 * i / (n - 1);

            tab[descending ? (n - i - 1) : i] = (ushort)Math.Floor(v + 0.5);
        }
    }
}
