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
using lcms2.state;
using lcms2.types;

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

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

    public readonly static PluginMemHandler* DebugMemHandler;
    public static bool HasConsole = !Console.IsInputRedirected;

    private readonly static Destructor Finalizer = new();
    private static uint thread = 1;

    public static T cmsmin<T>(T a, T b) where T : IComparisonOperators<T, T, bool> =>
        (a < b) ? a : b;

    [DoesNotReturn]
    public static void Die(string reason)
    {
        Err.WriteLine(new TextRed($"\n{reason}"));
        Environment.Exit(1);
    }

    private class Destructor
    {
        ~Destructor()
        {
            NativeMemory.Free(DebugMemHandler);
        }
    }

    static Testbed()
    {
        DebugMemHandler = (PluginMemHandler*)NativeMemory.AllocZeroed((uint)sizeof(PluginMemHandler));

        DebugMemHandler->@base.Magic = Signature.Plugin.MagicNumber;
        DebugMemHandler->@base.ExpectedVersion = 2060;
        DebugMemHandler->@base.Type = Signature.Plugin.MemHandler;

        DebugMemHandler->MallocPtr = &DebugMalloc;
        DebugMemHandler->FreePtr = &DebugFree;
        DebugMemHandler->ReallocPtr = &DebugRealloc;
    }

    public static Context* DbgThread()
    {
        return (Context*)(void*)((byte*)null + (thread++ % 0xff0));
    }

    public static void* DebugMalloc(Context* ContextID, uint size)
    {
        if (size <= 0)
            Die("malloc requested with zero bytes");

        TotalMemory += size;

        if (TotalMemory > MaxAllocated)
            MaxAllocated = TotalMemory;

        if (size > SingleHit) SingleHit = size;

        try
        {
            var blk = (MemoryBlock*)NativeMemory.Alloc(size + (uint)sizeof(MemoryBlock));

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

    public static void DebugFree(Context* ContextID, void* Ptr)
    {
        if (Ptr is null)
            Die("NULL free (which is a no-op in C, but may be a clue of something going wrong)");

        var blk = (MemoryBlock*)((byte*)Ptr - (uint)sizeof(MemoryBlock));
        TotalMemory -= blk->KeepSize;

        if (blk->WhoAllocated != ContextID && blk->DontCheck is 0)
            Die($"Trying to free memory allocated by a different thread\nAllocated by Context at\t{(ulong)blk->WhoAllocated}\nFreed by Context at\t{(ulong)ContextID}");

        NativeMemory.Free(blk);
    }

    public static void* DebugRealloc(Context* ContextID, void* Ptr, uint NewSize)
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
        Con.WriteLine("[Memory statistics]");
        Con.WriteLine($"Allocated = {TotalMemory} MaxAlloc = {MaxAllocated} Single block hit = {SingleHit}");
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
            Con.WriteLine("Ok, but ", new TextRed(MemStr(TotalMemory)), " are left!");
        else if (ok)
            Con.WriteLine(new TextGreen("Ok."));
    }

    public static void* PluginMemHander() =>
        DebugMemHandler;

    public static Context* WatchDogContext(void* usr)
    {
        var ctx = cmsCreateContext(DebugMemHandler, usr);

        if (ctx is null)
            Die("Unable to create memory managed context");

        DebugMemDontCheckThis(ctx);
        return ctx;
    }

    public static void FatalErrorQuit(Context* _1, ErrorCode _2, string text) =>
        Die(text);

    public static void ResetFatalError() =>
        cmsSetLogErrorHandler(&FatalErrorQuit);

    public static void Dot() =>
        Con.Write(".");

    public static void Say(string str) =>
        Con.Write(str);

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
            Con.Write($"Checking {title} ...");

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
            Err.WriteLine(new TextRed("FAIL!"));

            if (!String.IsNullOrEmpty(SubTestBuffer))
                Err.WriteLine($"{title}: [{SubTestBuffer}]\n\t{ReasonToFailBuffer}");
            else
                Err.WriteLine($"{title}:\n\t{ReasonToFailBuffer}");

            if (SimultaneousErrors > 1)
                Err.WriteLine("\tMore than one (", new TextRed(SimultaneousErrors), ") errors were reported");

            TotalFail++;
        }
    }

    public static bool CheckExhaustive()
    {
        Con.Write("Run exhaustive tests? (y/N) (N in 5 sec) ");
        var key = WaitForKey(5000);
        if (key.HasValue)
        {
            if (key.Value.Key is ConsoleKey.Enter or ConsoleKey.N)
            {
                if (key.Value.Key is ConsoleKey.Enter)
                    Con.WriteLine("N");
                else
                    Con.WriteLine(key.Value.KeyChar.ToString());
                return false;
            }
            else if (key.Value.Key is ConsoleKey.Y)
            {
                Con.WriteLine(key.Value.KeyChar.ToString());
                return true;
            }
        }
        else
        {
            Con.WriteLine();
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

        Con.WriteLine("Supported intents:");
        for (var i = 0; i < intents; i++)
        {
            Con.WriteLine($"\t{codes[i]} - {descriptions[i]}");
        }

        Con.WriteLine();
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

    internal static void _write(TextWriter w, params object[] args)
    {
        foreach (var arg in args)
        {
            switch (arg)
            {
                case Text t:
                    if (t.fColor is not null)
                        Console.ForegroundColor = t.fColor.Value;

                    if (t.bColor is not null)
                        Console.BackgroundColor = t.bColor.Value;

                    w.Write(t.value);

                    Console.ResetColor();
                    break;

                case string s:
                    w.Write(s);
                    break;

                default:
                    w.Write(arg);
                    break;
            }
        }
    }
}
