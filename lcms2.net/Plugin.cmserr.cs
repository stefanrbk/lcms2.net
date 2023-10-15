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
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using lcms2.state;

namespace lcms2;
public static partial class Plugin
{
    /// <summary>
    ///     Log an error
    /// </summary>
    /// <param name="text">English description of the error in String.Format format</param>
    [DebuggerStepThrough]
    public static void cmsSignalError(Context? ContextID, EventId errorCode, string text, params object?[] args)
    {
        // Check for the context, if specified go there. If not, go for the global
        var lhg = GetLogger(ContextID);
        text = String.Format(text, args);
        if (text.Length > MaxErrorMessageLen)
            text = text.Remove(MaxErrorMessageLen);

        lhg.LogError(errorCode, "{ErrorText}", text);
    }
}
