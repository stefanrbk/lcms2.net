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

namespace MoreSpans;

public static class SpanExtensions
{
    #region Delegates

    public delegate T FuncTo<T>(ReadOnlySpan<byte> span)
        where T : unmanaged;

    #endregion Delegates

    #region Public Methods

    public static ConvertingSpan<Tfrom, Tto> Converter<Tfrom, Tto>(this Span<Tfrom> span, ConvertFunc<Tfrom, Tto> funcTo, ConvertFunc<Tto, Tfrom> funcFrom)
        where Tfrom : unmanaged
        where Tto : unmanaged =>
        new(span, funcTo, funcFrom);

    public static ConvertingReadOnlySpan<Tfrom, Tto> Converter<Tfrom, Tto>(this ReadOnlySpan<Tfrom> span, ConvertFunc<Tfrom, Tto> func)
        where Tfrom : unmanaged
        where Tto : unmanaged =>
        new(span, func);

    public static void CopyTo<T, Tto>(this ReadOnlySpan<Tto> span, UpCastingSpan<T, Tto> destination)
        where T : unmanaged
        where Tto : unmanaged
    {
        var length = span.Length;
        for (var i = 0; i < length; i++)
            destination[i] = span[i];
    }

    public static void CopyTo<T, Tto>(this Tto[] span, UpCastingSpan<T, Tto> destination)
        where T : unmanaged
        where Tto : unmanaged
    {
        var length = span.Length;
        for (var i = 0; i < length; i++)
            destination[i] = span[i];
    }

    public static DownCastingReadOnlySpan<Tfrom, Tto> DownCaster<Tfrom, Tto>(this ReadOnlySpan<Tfrom> span, DownCastFunc<Tto, Tfrom> func)
            where Tfrom : unmanaged
        where Tto : unmanaged =>
        new(span, func);

    public static DownCastingSpan<Tfrom, Tto> DownCaster<Tfrom, Tto>(this Span<Tfrom> span, DownCastFunc<Tto, Tfrom> funcTo, UpCastFunc<Tto, Tfrom> funcFrom)
        where Tfrom : unmanaged
        where Tto : unmanaged =>
        new(span, funcTo, funcFrom);

    public static UpCastingReadOnlySpan<Tfrom, Tto> UpCaster<Tfrom, Tto>(this ReadOnlySpan<Tfrom> span, UpCastFunc<Tfrom, Tto> func)
                    where Tfrom : unmanaged
        where Tto : unmanaged =>
        new(span, func);

    public static UpCastingSpan<Tfrom, Tto> UpCaster<Tfrom, Tto>(this Span<Tfrom> span, UpCastFunc<Tfrom, Tto> funcTo, DownCastFunc<Tfrom, Tto> funcFrom)
                where Tfrom : unmanaged
        where Tto : unmanaged =>
        new(span, funcTo, funcFrom);

    #endregion Public Methods

    //public static void CopyTo<T, Tto>(this ReadOnlySpan<Tto> span, DownCastingSpan<T, Tto> destination)
    //    where T : unmanaged
    //    where Tto : unmanaged
    //{
    //    var length = span.Length;
    //    for (var i = 0; i < length; i++)
    //        destination[i] = span[i];
    //}
}
