//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022      Stefan Kewatt
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
namespace lcms2.it8;

[Serializable]
public class IT8Exception : Exception
{
    #region Public Constructors

    public IT8Exception(string? message)
        : base(message) { }

    public IT8Exception(Stack<StreamReader> fileStack, int lineNo)
        : base($"{(fileStack.Peek().BaseStream is FileStream fs ? fs.Name : "Memory")}: Line {lineNo}, An error has occurred") { }

    public IT8Exception(Stack<StreamReader> fileStack, int lineNo, string? message)
        : base($"{(fileStack.Peek().BaseStream is FileStream fs ? fs.Name : "Memory")}: Line {lineNo}, {message}") { }

    public IT8Exception(Stack<StreamReader> fileStack, int lineNo, string? message, Exception? innerException)
        : base($"{(fileStack.Peek().BaseStream is FileStream fs ? fs.Name : "Memory")}: Line {lineNo}, {message}", innerException) { }

    #endregion Public Constructors

    #region Protected Constructors

    protected IT8Exception(
                      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    #endregion Protected Constructors
}
