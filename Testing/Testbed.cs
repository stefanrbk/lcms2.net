//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022-2023 Stefan Kewatt
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//---------------------------------------------------------------------------------
//
global using unsafe Context = lcms2.state.Context_struct*;
global using unsafe HPROFILE = void*;

using lcms2.state;
using lcms2.types;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace lcms2.testbed;

internal static unsafe partial class Testbed
{
    public static string ReasonToFailBuffer = String.Empty;
    public static string SubTestBuffer = String.Empty;
    public static int TotalFail = 0;
    public static int TotalTests = 0;
    public static bool TrappedError;
    public static int SimultaneousErrors = 0;
    public static uint SingleHit, MaxAllocated, TotalMemory;

    public const double FIXED_PRECISION_15_16 = 1.0 / 65535;
    public const double FIXED_PRECISION_8_8 = 1.0 / 255;
    public const double FLOAT_PRECISION = 0.00001;

    public static double MaxErr = 0;
    public static readonly object MaxErrLock = new();
    public static double AllowedErr = FIXED_PRECISION_15_16;

    public static readonly int SIZE_OF_MEM_HEADER = sizeof(MemoryBlock);

    public static readonly PluginMemHandler DebugMemHandler = new()
    {
        @base = new()
        {
            ExpectedVersion = 2060,
            Magic = cmsPluginMagicNumber,
            Type = cmsPluginMemHandlerSig,
        },
        MallocPtr = DebugMalloc,
        FreePtr = DebugFree,
        ReallocPtr = DebugRealloc,
    };
    public static bool HasConsole = !Console.IsInputRedirected;
    private static uint thread = 1;

    static Testbed()
    {
        Rec709Plugin.FunctionTypes[0] = TYPE_709;
        Rec709Plugin.ParameterCount[0] = 5;

        CurvePluginSample.FunctionTypes[0] = TYPE_SIN;
        CurvePluginSample.FunctionTypes[1] = TYPE_COS;
        CurvePluginSample.ParameterCount[0] = 1;
        CurvePluginSample.ParameterCount[1] = 1;

        CurvePluginSample2.FunctionTypes[0] = TYPE_TAN;
        CurvePluginSample2.ParameterCount[0] = 1;
    }

    public static T cmsmin<T>(T a, T b) where T : IComparisonOperators<T, T, bool> =>
        (a < b) ? a : b;

    [DoesNotReturn]
    public static void Die(string reason)
    {
        ErrorWriteLine();
        ErrorWriteLine($"{{red:{reason}}}");
        Environment.Exit(1);
    }

    [DebuggerStepThrough]
    public static Context DbgThread()
    {
        return (Context)(void*)((byte*)null + (thread++ % 0xff0));
    }

    public static void* DebugMalloc(Context ContextID, uint size)
    {
        if (size <= 0)
            Die("malloc requested with zero bytes");

        TotalMemory += size;

        if (TotalMemory > MaxAllocated)
            MaxAllocated = TotalMemory;

        if (size > SingleHit) SingleHit = size;

        try
        {
            var blk = (MemoryBlock*)alloc(size + (uint)sizeof(MemoryBlock));

            blk->KeepSize = size;
            blk->WhoAllocated = ContextID;
            blk->DontCheck = 0;

            return (byte*)blk + (uint)sizeof(MemoryBlock);
        }
        catch
        {
            return null;
        }
    }

    public static void DebugFree(Context ContextID, void* Ptr)
    {
        if (Ptr is null)
            Die("NULL free (which is a no-op in C, but may be a clue of something going wrong)");

        var blk = (MemoryBlock*)((byte*)Ptr - (uint)sizeof(MemoryBlock));
        TotalMemory -= blk->KeepSize;

        if (blk->WhoAllocated != ContextID && blk->DontCheck is 0)
            Die($"Trying to free memory allocated by a different thread\nAllocated by Context at\t{(ulong)blk->WhoAllocated:x16}\nFreed by Context at\t{(ulong)ContextID:x16}");
        try
        {
            free(blk);
        }
        catch { }
    }

    public static void* DebugRealloc(Context ContextID, void* Ptr, uint NewSize)
    {
        var NewPtr = DebugMalloc(ContextID, NewSize);
        if (Ptr is null) return NewPtr;

        var blk = (MemoryBlock*)((byte*)Ptr - (uint)sizeof(MemoryBlock));
        var max_sz = blk->KeepSize > NewSize ? NewSize : blk->KeepSize;
        NativeMemory.Copy(Ptr, NewPtr, max_sz);
        DebugFree(ContextID, Ptr);

        return NewPtr;
    }

    public static void DebugMemPrintTotals()
    {
        ConsoleWriteLine("[Memory statistics]");
        ConsoleWriteLine($"Allocated = {TotalMemory} MaxAlloc = {MaxAllocated} Single block hit = {SingleHit}");
    }

    public static void DebugMemDontCheckThis(void* Ptr)
    {
        var blk = (MemoryBlock*)((byte*)Ptr - (uint)sizeof(MemoryBlock));

        blk->DontCheck = 1;
    }

    public static string MemStr(uint size) =>
        size switch
        {
            > 1024 * 1024 => $"{size / (1024 * 1024)} Mb",
            > 1024 => $"{size / 1024} Kb",
            _ => $"{size} bytes",
        };

    public static void TestMemoryLeaks(bool ok)
    {
        if (TotalMemory > 0)
            ConsoleWriteLine($"Ok, but {{red:{MemStr(TotalMemory)}}}, are left!");
        else if (ok)
            ConsoleWriteLine("{green:Ok.}");
    }

    public static Context WatchDogContext(void* usr)
    {
        fixed (void* handler = &DebugMemHandler)
        {
            var ctx = cmsCreateContext(handler, usr);

            if (ctx is null)
                Die("Unable to create memory managed context");

            DebugMemDontCheckThis(ctx);
            return ctx;
        }
    }

    public static void FatalErrorQuit(Context _1, ErrorCode _2, string text) =>
        Die(text);

    public static void ResetFatalError() =>
        cmsSetLogErrorHandler(FatalErrorQuit);

    public static void Dot() =>
        ConsoleWrite(".");

    public static void Say(string str) =>
        ConsoleWrite(str);

    public static bool Fail(string text)
    {
        lock (ReasonToFailBuffer)
            ReasonToFailBuffer = text;

        return false;
    }

    public static void SubTest(string frm)
    {
        Dot();
        SubTestBuffer = frm;
    }

    public static void Check(string title, Func<bool> test)
    {
        if (HasConsole)
            ConsoleWrite($"Checking {title} ...");

        ReasonToFailBuffer = String.Empty;
        SubTestBuffer = String.Empty;
        TrappedError = false;
        SimultaneousErrors = 0;
        TotalTests++;

        if (test() && !TrappedError)
        {
            // It is a good place to check memory
            TestMemoryLeaks(true);
        }
        else
        {
            ErrorWriteLine("{red:FAIL!}");

            if (!String.IsNullOrEmpty(SubTestBuffer))
                ErrorWriteLine($"{title}: [{SubTestBuffer}]\n\t{ReasonToFailBuffer}");
            else
                ErrorWriteLine($"{title}:\n\t{ReasonToFailBuffer}");

            if (SimultaneousErrors > 1)
                ErrorWriteLine($"\tMore than one ({{red:{SimultaneousErrors}}}) errors were reported");

            TotalFail++;
        }
    }

    public static bool CheckExhaustive()
    {
        ConsoleWrite("Run exhaustive tests? (y/N) (N in 5 sec) ");
        var key = WaitForKey(5000);
        if (key.HasValue)
        {
            if (key.Value.Key is ConsoleKey.Enter or ConsoleKey.N)
            {
                if (key.Value.Key is ConsoleKey.Enter)
                    ConsoleWriteLine("N");
                else
                    ConsoleWriteLine(key.Value.KeyChar.ToString());
                return false;
            }
            else if (key.Value.Key is ConsoleKey.Y)
            {
                ConsoleWrite(key.Value.KeyChar.ToString());
                return true;
            }
        }
        else
        {
            ConsoleWriteLine();
        }

        return false;
    }

    public static ConsoleKeyInfo? WaitForKey(int ms)
    {
        var cancel = new CancellationTokenSource(ms);
        var token = cancel.Token;
        var task = Task.Run(() => Console.ReadKey(true), token);
        try
        {
            task.Wait(token);
            var read = task.IsCompletedSuccessfully;
            if (read) return task.Result;
        }
        catch
        {
        }
        return null;
    }

    public static void PrintSupportedIntents()
    {
        var codes = stackalloc uint[200];
        var descriptions = new string?[200];
        var intents = cmsGetSupportedIntents(200, codes, descriptions);

        ConsoleWriteLine("Supported intents:");
        for (var i = 0; i < intents; i++)
        {
            ConsoleWriteLine($"\t{codes[i]} - {descriptions[i]}");
        }

        ConsoleWriteLine();
    }

    public static bool IsGoodVal(string title, double @in, double @out, double max)
    {
        var err = Math.Abs(@in - @out);

        lock (MaxErrLock)
            if (err > MaxErr) MaxErr = err;

        if (err > max)
        {
            Fail($"({title}): Must be {@in}, but was {@out} ");
            return false;
        }
        return true;
    }

    public static bool IsGoodFixed15_16(string title, double @in, double @out) =>
        IsGoodVal(title, @in, @out, FIXED_PRECISION_15_16);

    public static bool IsGoodFixed8_8(string title, double @in, double @out) =>
        IsGoodVal(title, @in, @out, FIXED_PRECISION_8_8);

    public static bool IsGoodWord(string title, ushort @in, ushort @out) =>
        IsGoodWordPrec(title, @in, @out, 0);

    public static bool IsGoodWordPrec(string title, ushort @in, ushort @out, ushort maxErr)
    {
        if (Math.Abs(@in - @out) > maxErr)
        {
            Fail($"({title}): Must be {@in}, but was {@out} ");
            return false;
        }

        return true;
    }

    internal static void ConsoleWrite(string str) =>
        _write(str, Console.Out);

    internal static void ConsoleWriteLine(string str = "") =>
        _write(str + '\n', Console.Out);

    internal static void ErrorWrite(string str) =>
        _write(str, Console.Error);

    internal static void ErrorWriteLine(string str = "") =>
        _write(str + '\n', Console.Error);

    private static void _write(string str, TextWriter w)
    {
        var value = new Queue<char>(str.AsSpan().ToArray());
        var color = new StringBuilder();

        while (value.Count > 0)
        {
            var ch = value.Dequeue();
            if (ch is '{')
            {
                color.Clear();
                do
                {
                    color.Append(value.Peek());
                } while (value.Dequeue() is not ':');

                switch (color.ToString().ToLower())
                {
                    case "green:":
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;

                    case "red:":
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }
            }
            else if (ch is '}')
            {
                Console.ResetColor();
            }
            else
            {
                w.Write(ch);
            }
        }
    }
}
