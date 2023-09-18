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
//global using unsafe Context = lcms2.state.Context_struct*;

//using lcms2.state;
//using lcms2.types;

//using System.Runtime.InteropServices;

//namespace lcms2.tests;

//public static unsafe class Globals
//{
//    public const string FixedTest = "Fixed Test";
//    public const double FloatPrecision = 1E-5;
//    public const string RandomTest = "Random Test";
//    public const double S15Fixed16Precision = 1.0 / 65535.0;
//    public const double U8Fixed8Precision = 1.0 / 255.0;
//    public static readonly PluginMemHandler* DebugMemHandler;
//    internal static uint thread;
//    private static object memVarsMutex = new();
//    internal static uint TotalMemory;
//    internal static uint MaxAllocated;
//    internal static uint SingleHit;

//    static Globals()
//    {
//        DebugMemHandler = (PluginMemHandler*)allocZeroed(sizeof(PluginMemHandler));

//        DebugMemHandler->@base.Magic = cmsPluginMagicNumber;
//        DebugMemHandler->@base.ExpectedVersion = 2060;
//        DebugMemHandler->@base.Type = cmsPluginMemHandlerSig;

//        DebugMemHandler->MallocPtr = DebugMalloc;
//        DebugMemHandler->FreePtr = DebugFree;
//        DebugMemHandler->ReallocPtr = DebugRealloc;
//    }

//    public static Context DbgThread() =>
//        (Context)(void*)((byte*)null + (Interlocked.Increment(ref thread) % 0xff0));

//    private static void* DebugMalloc(Context ContextID, uint size)
//    {
//        if (size <= 0)
//            Assert.Fail("malloc requested with zero bytes");

//        lock (memVarsMutex)
//        {
//            TotalMemory += size;

//            if (TotalMemory > MaxAllocated)
//                MaxAllocated = TotalMemory;

//            if (size > SingleHit) SingleHit = size;
//        }

//        try
//        {
//            var blk = (MemoryBlock*)alloc(size + (uint)sizeof(MemoryBlock));

//            blk->KeepSize = size;
//            blk->WhoAllocated = ContextID;
//            blk->DontCheck = 0;

//            return (byte*)blk + (uint)sizeof(MemoryBlock);
//        }
//        catch
//        {
//            return null;
//        }
//    }

//    private static void DebugFree(Context ContextID, void* Ptr)
//    {
//        if (Ptr is null)
//            Assert.Fail("NULL free (which is a no-op in C, but may be a clue of something going wrong)");

//        var blk = (MemoryBlock*)((byte*)Ptr - (uint)sizeof(MemoryBlock));
//        TotalMemory -= blk->KeepSize;

//        if (blk->WhoAllocated != ContextID && blk->DontCheck is 0)
//            Assert.Fail($"Trying to free memory allocated by a different thread\nAllocated by Context at\t{(ulong)blk->WhoAllocated}\nFreed by Context at\t{(ulong)ContextID}");

//        free(blk);
//    }

//    private static void* DebugRealloc(Context ContextID, void* Ptr, uint NewSize)
//    {
//        var NewPtr = DebugMalloc(ContextID, NewSize);
//        if (Ptr is null) return NewPtr;

//        var blk = (MemoryBlock*)((byte*)Ptr - (uint)sizeof(MemoryBlock));
//        var max_sz = blk->KeepSize > NewSize ? NewSize : blk->KeepSize;
//        NativeMemory.Copy(Ptr, NewPtr, max_sz);
//        DebugFree(ContextID, Ptr);

//        return NewPtr;
//    }

//    private static string MemStr(uint size) =>
//        size switch
//        {
//            > 1024 * 1024 => $"{size / (1024 * 1024)} Mb",
//            > 1024 => $"{size / 1024} Kb",
//            _ => $"{size} bytes",
//        };

//    public static void TestMemoryLeaks(bool ok)
//    {
//        if (TotalMemory > 0)
//            Console.WriteLine($"Ok, but {MemStr(TotalMemory)} are left!");
//    }

//    public static void DebugMemDontCheckThis(void* Ptr)
//    {
//        var blk = (MemoryBlock*)((byte*)Ptr - (uint)sizeof(MemoryBlock));

//        blk->DontCheck = 1;
//    }

//    public static Context WatchDogContext(void* usr)
//    {
//        var ctx = cmsCreateContext(DebugMemHandler, usr);

//        if (ctx is null)
//            Assert.Fail("Unable to create memory managed context");

//        DebugMemDontCheckThis(ctx);
//        return ctx;
//    }

//    public static void IsGoodDouble(string message, double actual, double expected, double delta) =>
//            Assert.That(actual, Is.EqualTo(expected).Within(delta), message);

//    public static void IsGoodFixed15_16(string message, double @in, double @out, object @lock, ref double maxErr) =>
//        IsGoodVal(message, @in, @out, S15Fixed16Precision, @lock, ref maxErr);

//    public static void IsGoodFixed15_16(string message, double @in, double @out) =>
//        IsGoodVal(message, @in, @out, S15Fixed16Precision);

//    public static void IsGoodFixed8_8(string message, double @in, double @out, object @lock, ref double maxErr) =>
//        IsGoodVal(message, @in, @out, U8Fixed8Precision, @lock, ref maxErr);

//    public static void IsGoodVal(string message, double @in, double @out, double max, object @lock, ref double maxErr)
//    {
//        var err = Math.Abs(@in - @out);

//        lock (@lock)
//            if (err > maxErr) maxErr = err;

//        Assert.That(@in, Is.EqualTo(@out).Within(max), message);
//    }

//    public static void IsGoodVal(string message, double @in, double @out, double max)
//    {
//        var err = Math.Abs(@in - @out);

//        Assert.That(@in, Is.EqualTo(@out).Within(max), message);
//    }

//    public static void IsGoodWord(string message, ushort @in, ushort @out, ushort maxErr = 0) =>
//        Assert.That(@in, Is.EqualTo(@out).Within(maxErr), message);

//    public static double Sqr(double v) =>
//        v * v;
//}
