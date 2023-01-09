﻿//---------------------------------------------------------------------------------
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
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Tag type handler
/// </summary>
/// <remarks>
///     Each type is free to return anything it wants, and it is up to the caller to know in advance
///     what is the type contained in the tag. <br/> Implements the <c>cmsTagTypeHandler</c> struct.
/// </remarks>
public abstract partial class TagTypeHandler
{
    #region Protected Constructors

    protected TagTypeHandler(Signature signature, object? state, uint iCCVersion)
    {
        Signature = signature;
        StateContainer = state;
        ICCVersion = iCCVersion;
    }

    #endregion Protected Constructors

    #region Properties

    /// <summary>
    ///     Additional parameter used by the calling thread
    /// </summary>
    public virtual uint ICCVersion { get; internal set; }

    /// <summary>
    ///     Signature of the type
    /// </summary>
    public virtual Signature Signature { get; }

    /// <summary>
    ///     Additional parameter used by the calling thread
    /// </summary>
    public virtual object? StateContainer { get; internal set; }

    #endregion Properties

    #region Public Methods

    /// <summary>
    ///     Duplicate an item or array of items
    /// </summary>
    public abstract object? Duplicate(object value, int num);

    /// <summary>
    ///     Free all resources
    /// </summary>
    public abstract void Free(object value);

    /// <summary>
    ///     Allocates and reads items.
    /// </summary>
    public abstract unsafe object? Read(Stream io, int sizeOfTag, out int numItems);

    /// <summary>
    ///     Writes n Items
    /// </summary>
    public abstract unsafe bool Write(Stream io, object value, int numItems);

    #endregion Public Methods
}
