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
namespace lcms2.plugins;

public delegate IMutex CreateMutexFn(Context? context);

public delegate void DestroyMutexFn(Context? context, IMutex mutex);

public delegate bool LockMutexFn(Context? context, IMutex mutex);

public delegate void UnlockMutexFn(Context? context, IMutex mutex);

internal class MutexPluginChunkType
{
    public CreateMutexFn CreateFn;
    public DestroyMutexFn DestroyFn;
    public LockMutexFn LockFn;
    public UnlockMutexFn UnlockFn;

    public MutexPluginChunkType(CreateMutexFn createFn, DestroyMutexFn destroyFn, LockMutexFn lockFn, UnlockMutexFn unlockFn)
    {
        CreateFn = createFn;
        DestroyFn = destroyFn;
        LockFn = lockFn;
        UnlockFn = unlockFn;
    }
}
