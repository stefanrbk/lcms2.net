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

    internal Signature implements;

    private bool _disposedValue;

    private uint _inputChan;

    private uint _outputChan;

    #endregion Fields

    #region Properties

    public StageData Data
    {
        /** Original Code (cmslut.c line: 1224)
         **
         ** void* CMSEXPORT cmsStageData(const cmsStage* mpe)
         ** {
         **     return mpe -> Data;
         ** }
         **/

        get; internal set;
    }

    public uint InputChannels
    {
        /** Original Code (cmslut.c line: 1209)
         **
         ** cmsUInt32Number  CMSEXPORT cmsStageInputChannels(const cmsStage* mpe)
         ** {
         **     return mpe ->InputChannels;
         ** }
         **/

        get => _inputChan;
        internal set => _inputChan = value <= maxStageChannels ? value : _inputChan;
    }

    public Stage? Next
    {
        /** Original Code (cmslut.c line: 1234)
         **
         ** cmsStage*  CMSEXPORT cmsStageNext(const cmsStage* mpe)
         ** {
         **     return mpe -> Next;
         ** }
         **/

        get; internal set;
    }

    public uint OutputChannels
    {
        /** Original Code (cmslut.c line: 1214)
         **
         ** cmsUInt32Number  CMSEXPORT cmsStageOutputChannels(const cmsStage* mpe)
         ** {
         **     return mpe ->OutputChannels;
         ** }
         **/

        get => _outputChan;
        internal set => _outputChan = value <= maxStageChannels ? value : _outputChan;
    }

    public object? StateContainer
    {
        /** Original Code (cmslut.c line: 1229)
         **
         ** cmsContext CMSEXPORT cmsGetStageContextID(const cmsStage* mpe)
         ** {
         **     return mpe -> ContextID;
         ** }
         **/

        get; internal set;
    }

    public Signature Type
    {
        /** Original Code (cmslut.c line: 1219)
         **
         ** cmsStageSignature CMSEXPORT cmsStageType(const cmsStage* mpe)
         ** {
         **     return mpe -> Type;
         ** }
         **/

        get; internal set;
    }

    internal ToneCurve[] CurveSet =>
        /** Original Code (cmslut.c line: 159)
         **
         ** cmsToneCurve** _cmsStageGetPtrToCurveSet(const cmsStage* mpe)
         ** {
         **     _cmsStageToneCurvesData* Data = (_cmsStageToneCurvesData*) mpe ->Data;
         **
         **     return Data ->TheCurves;
         ** }
         **/

        (Data as ToneCurveData)?.TheCurves ?? throw new InvalidOperationException();

    #endregion Properties

    #region Public Methods

    public object Clone()
    {
        /** Original Code (cmslut.c line: 1240)
         **
         ** // Duplicates an MPE
         ** cmsStage* CMSEXPORT cmsStageDup(cmsStage* mpe)
         ** {
         **     cmsStage* NewMPE;
         **
         **     if (mpe == NULL) return NULL;
         **     NewMPE = _cmsStageAllocPlaceholder(mpe ->ContextID,
         **                                      mpe ->Type,
         **                                      mpe ->InputChannels,
         **                                      mpe ->OutputChannels,
         **                                      mpe ->EvalPtr,
         **                                      mpe ->DupElemPtr,
         **                                      mpe ->FreePtr,
         **                                      NULL);
         **     if (NewMPE == NULL) return NULL;
         **
         **     NewMPE ->Implements = mpe ->Implements;
         **
         **     if (mpe ->DupElemPtr) {
         **
         **         NewMPE ->Data = mpe ->DupElemPtr(mpe);
         **
         **         if (NewMPE->Data == NULL) {
         **
         **             cmsStageFree(NewMPE);
         **             return NULL;
         **         }
         **
         **     } else {
         **
         **         NewMPE ->Data       = NULL;
         **     }
         **
         **     return NewMPE;
         ** }
         **/

        var result = Alloc(
            StateContainer,
            Type,
            InputChannels,
            OutputChannels,
            Data.Duplicate(this));
        if (result is null) return null!;

        result.implements = implements;

        return result;
    }

    public void Dispose()
    {
        /** Original Code (cmslut.c line: 1199)
         **
         ** // Free a single MPE
         ** void CMSEXPORT cmsStageFree(cmsStage* mpe)
         ** {
         **     if (mpe ->FreePtr)
         **         mpe ->FreePtr(mpe);
         **
         **     _cmsFree(mpe ->ContextID, mpe);
         ** }
         **/

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
                Data?.Dispose();

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
