//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2023 Marti Maria Saguer
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

//namespace lcms2.tests;

//[TestFixture(TestOf = typeof(Context))]
//public unsafe class ContextTests : TestBase
//{
//    [Test]
//    public void GlobalContextDoesntHaveNullChunks()
//    {
//        var ctx = _cmsGetContext(null);

//        Assert.Multiple(() =>
//        {
//            for (int i = 1; i < (int)Chunks.Max; i++)
//            {
//                Assert.That(ctx->chunks[(Chunks)i] is not null, $"Chunk {i}");
//            }
//        });
//    }

//    [Test]
//    public void NewContextDoesntHaveNullChunks()
//    {
//        var ctx = cmsCreateContext(null, null);

//        Assert.Multiple(() =>
//        {
//            for (int i = 1; i < (int)Chunks.Max; i++)
//            {
//                Assert.That(ctx->chunks[(Chunks)i] is not null, $"Chunk {i}");
//            }
//        });

//        cmsDeleteContext(ctx);
//    }

//    private static Context* DupContext(Context* src, void* Data)
//    {
//        var cpy = cmsDupContext(src, Data);

//        DebugMemDontCheckThis(cpy);

//        return cpy;
//    }

//    [Test]
//    public void AllocationTest()
//    {
//        var c1 = cmsCreateContext(null, null);      // This creates a context by using the normal malloc
//        DebugMemDontCheckThis(c1);
//        cmsDeleteContext(c1);

//        var c2 = cmsCreateContext(DebugMemHandler, null); // This creates a context by using the debug malloc
//        DebugMemDontCheckThis(c2);
//        cmsDeleteContext(c2);

//        c1 = cmsCreateContext(null, null);
//        DebugMemDontCheckThis(c1);

//        c2 = cmsCreateContext(DebugMemHandler, null);
//        DebugMemDontCheckThis(c2);

//        cmsPluginTHR(c1, DebugMemHandler);    // Now the context has custom allocators

//        var c3 = DupContext(c1, null);
//        var c4 = DupContext(c2, null);

//        cmsDeleteContext(c1);   // Should be deleted by using normal malloc
//        cmsDeleteContext(c2);   // Should be deleted by using debug malloc
//        cmsDeleteContext(c3);   // Should be deleted by using normal malloc
//        cmsDeleteContext(c4);   // Should be deleted by using debug malloc
//    }

//[Test]
//public void VerifyUserDataPropagationOnDup()
//{
//    var a = 1;
//    var b = 32;
//    var rc = false;

//    // This function creates a context with a special
//    // memory manager that checks allocation
//    var c1 = WatchDogContext(&a);
//    cmsDeleteContext(c1);

//    c1 = WatchDogContext(&a);

//    // Let's check duplication
//    var c2 = DupContext(c1, null);
//    var c3 = DupContext(c2, null);

//    // User data should have been propagated
//    rc = (*(int*)cmsGetContextUserData(c3)) == 1;

//    // Free resources
//    cmsDeleteContext(c1);
//    cmsDeleteContext(c2);
//    cmsDeleteContext(c3);

//    if (!rc)
//        Assert.Fail("Creation of user data failed");

//    // Back to create 3 levels of inheritance
//    c1 = cmsCreateContext(null, &a);
//    DebugMemDontCheckThis(c1);

//    c2 = DupContext(c1, null);
//    c3 = DupContext(c2, &b);

//    // New user data should be applied to c3
//    rc = (*(int*)cmsGetContextUserData(c3)) == 32;

//    cmsDeleteContext(c1);
//    cmsDeleteContext(c2);
//    cmsDeleteContext(c3);

//    if (!rc)
//        Assert.Fail("Modification of user data failed");
//}

//[Test]
//public static void CheckAlarmColorsContext()
//{
//    var codes = stackalloc ushort[16] { 0x0000, 0x1111, 0x2222, 0x3333, 0x4444, 0x5555, 0x6666, 0x7777, 0x8888, 0x9999, 0xaaaa, 0xbbbb, 0xcccc, 0xdddd, 0xeeee, 0xffff };
//    var values = stackalloc ushort[16];

//    var c1 = WatchDogContext(null);

//    cmsSetAlarmCodesTHR(c1, codes);
//    var c2 = DupContext(c1, null);
//    var c3 = DupContext(c2, null);

//    cmsGetAlarmCodesTHR(c3, values);

//    Assert.Multiple(() =>
//    {
//        for (var i = 0; i < 16; i++)
//        {
//            if (values[i] != codes[i])
//                Assert.Fail($"Bad alarm code #{i}: {values[i]} != {codes[i]}");
//        }
//    });

//    cmsDeleteContext(c1);
//    cmsDeleteContext(c2);
//    cmsDeleteContext(c3);
//}

//[Test]
//public static void CheckAdaptationStateContext()
//{
//    var old1 = cmsSetAdaptationStateTHR(null, -1);

//    var c1 = WatchDogContext(null);

//    cmsSetAdaptationStateTHR(c1, 0.7);

//    var c2 = DupContext(c1, null);
//    var c3 = DupContext(c2, null);

//    IsGoodVal("Adaption state", cmsSetAdaptationStateTHR(c3, -1), 0.7, 0.001);

//    cmsDeleteContext(c1);
//    cmsDeleteContext(c2);
//    cmsDeleteContext(c3);

//    var old2 = cmsSetAdaptationStateTHR(null, -1);

//    Assert.That(old1, Is.EqualTo(old2), "Adaptation state has changed");
//}
//}
