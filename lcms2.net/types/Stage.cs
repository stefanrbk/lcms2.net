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
namespace lcms2.types;

public sealed partial class Stage : ICloneable, IDisposable
{
    #region Fields

    public Stage? Next;
    internal Signature implements;
    private bool _disposedValue;
    private uint _inputChan;
    private uint _outputChan;

    #endregion Fields

    /*  Original Code (cmslut.c line: 1229)
     *
     *  cmsContext CMSEXPORT cmsGetStageContextID(const cmsStage* mpe)
     *  {
     *      return mpe -> ContextID;
     *  }
     */

    #region Properties

    public StageData Data { get; internal set; }

    public uint InputChannels
    {
        get => _inputChan;
        internal set => _inputChan = value <= maxStageChannels ? value : _inputChan;
    }

    public uint OutputChannels
    {
        get => _outputChan;
        internal set => _outputChan = value <= maxStageChannels ? value : _outputChan;
    }

    public object? StateContainer { get; internal set; }

    /*  Original Code (cmslut.c line: 1224)
     *
     *  void* CMSEXPORT cmsStageData(const cmsStage* mpe)
     *  {
     *      return mpe -> Data;
     *  }
     */
    /*  Original Code (cmslut.c line: 1209)
     *
     *  cmsUInt32Number  CMSEXPORT cmsStageInputChannels(const cmsStage* mpe)
     *  {
     *      return mpe ->InputChannels;
     *  }
     */
    /*  Original Code (cmslut.c line: 1234)
     *
     *  cmsStage*  CMSEXPORT cmsStageNext(const cmsStage* mpe)
     *  {
     *      return mpe -> Next;
     *  }
     */
    /*  Original Code (cmslut.c line: 1214)
     *
     *  cmsUInt32Number  CMSEXPORT cmsStageOutputChannels(const cmsStage* mpe)
     *  {
     *      return mpe ->OutputChannels;
     *  }
     */
    /*  Original Code (cmslut.c line: 1219)
     *
     *  cmsStageSignature CMSEXPORT cmsStageType(const cmsStage* mpe)
     *  {
     *      return mpe -> Type;
     *  }
     */
    public Signature Type { get; internal set; }

    /*  Original Code (cmslut.c line: 159)
     *
     *  cmsToneCurve** _cmsStageGetPtrToCurveSet(const cmsStage* mpe)
     *  {
     *      _cmsStageToneCurvesData* Data = (_cmsStageToneCurvesData*) mpe ->Data;
     *
     *      return Data ->TheCurves;
     *  }
     */

    internal ToneCurve[] CurveSet =>
        (Data as ToneCurveData)?.TheCurves ?? throw new InvalidOperationException();

    #endregion Properties

    #region Public Methods

    public object Clone()
    {
        var result = Alloc(
            StateContainer,
            Type,
            InputChannels,
            OutputChannels,
            Data.Duplicate(this))!;
        result.implements = implements;

        return result;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion Public Methods

    #region Private Methods

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
            }

            Data = null!;

            _disposedValue = true;
        }
    }

    #endregion Private Methods
}

public enum StageLoc
{
    AtBegin,
    AtEnd,
}
