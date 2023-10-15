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

using Microsoft.Extensions.Logging;

namespace lcms2.state;

internal class LogErrorChunkType(ILoggerFactory? factory = null) : ICloneable
{
    private ILoggerFactory factory = factory ?? DefaultLogErrorHandlerFunction();

    public ILoggerFactory Factory
    {
        get => factory; set
        {
            factory = value;
            FactoryChanged?.Invoke(this, new(value));
        }
    }

    internal event EventHandler<FactoryChangedEventArgs>? FactoryChanged;

    public object Clone() =>
        new LogErrorChunkType()
        {
            Factory = Factory,
            FactoryChanged = FactoryChanged?.Clone() as EventHandler<FactoryChangedEventArgs>
        };
}
internal class FactoryChangedEventArgs(ILoggerFactory factory)
{
    public ILoggerFactory Factory = factory;
}