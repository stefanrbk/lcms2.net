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
using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

public sealed class InterpolationPlugin
    : Plugin
{
    #region Fields

    public InterpFnFactory? InterpolatorsFactory;

    #endregion Fields

    #region Public Constructors

    public InterpolationPlugin(Signature magic, uint expectedVersion, Signature type, InterpFnFactory? interpolatorsFactory)
        : base(magic, expectedVersion, type) =>
        InterpolatorsFactory = interpolatorsFactory;

    #endregion Public Constructors

    #region Internal Methods

    internal static bool RegisterPlugin(object? state, InterpolationPlugin? plugin)
    {
        /**  Original Code (cmsintrp.c line: 66)
         **
         **  // Main plug-in entry
         **  cmsBool  _cmsRegisterInterpPlugin(cmsContext ContextID, cmsPluginBase* Data)
         **  {
         **      cmsPluginInterpolation* Plugin = (cmsPluginInterpolation*) Data;
         **      _cmsInterpPluginChunkType* ptr = (_cmsInterpPluginChunkType*) _cmsContextGetClientChunk(ContextID, InterpPlugin);
         **
         **      if (Data == NULL) {
         **
         **          ptr ->Interpolators = NULL;
         **          return TRUE;
         **      }
         **
         **      // Set replacement functions
         **      ptr ->Interpolators = Plugin ->InterpolatorsFactory;
         **      return TRUE;
         **  }
         **/

        var ptr = State.GetInterpolationPlugin(state);

        if (plugin is null)
        {
            ptr.interpolators = null;
            return true;
        }

        // Set replacement functions
        ptr.interpolators = plugin.InterpolatorsFactory;
        return true;
    }

    #endregion Internal Methods
}

internal sealed class InterpolationPluginChunk
{
    #region Fields

    internal static InterpolationPluginChunk global = new()
    {
        /**  Original Code (cmsintrp.c line: 43)
         **
         **  // This is the default factory
         **  _cmsInterpPluginChunkType _cmsInterpPluginChunk = { NULL };
         **/

        interpolators = null
    };
    internal InterpFnFactory? interpolators;

    #endregion Fields

    #region Private Constructors

    private InterpolationPluginChunk()
    { }

    #endregion Private Constructors

    #region Properties

    internal static InterpolationPluginChunk Default => new() { interpolators = null };

    #endregion Properties
}
