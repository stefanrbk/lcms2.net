namespace lcms2.types;

public sealed partial class Stage: ICloneable, IDisposable
{
    private uint _inputChan;
    private uint _outputChan;
    private bool _disposedValue;

    /*  Original Code (cmslut.c line: 1229)
     *  
     *  cmsContext CMSEXPORT cmsGetStageContextID(const cmsStage* mpe)
     *  {
     *      return mpe -> ContextID;
     *  }
     */
    public object? StateContainer { get; internal set; }

    /*  Original Code (cmslut.c line: 1224)
     *  
     *  void* CMSEXPORT cmsStageData(const cmsStage* mpe)
     *  {
     *      return mpe -> Data;
     *  }
     */
    public StageData Data { get; internal set; }

    internal Signature implements;

    /*  Original Code (cmslut.c line: 1209)
     *  
     *  cmsUInt32Number  CMSEXPORT cmsStageInputChannels(const cmsStage* mpe)
     *  {
     *      return mpe ->InputChannels;
     *  }
     */
    public uint InputChannels
    {
        get => _inputChan;
        internal set => _inputChan = value <= maxStageChannels ? value : _inputChan;
    }

    /*  Original Code (cmslut.c line: 1234)
     *  
     *  cmsStage*  CMSEXPORT cmsStageNext(const cmsStage* mpe)
     *  {
     *      return mpe -> Next;
     *  }
     */
    public Stage? Next;

    /*  Original Code (cmslut.c line: 1214)
     *  
     *  cmsUInt32Number  CMSEXPORT cmsStageOutputChannels(const cmsStage* mpe)
     *  {
     *      return mpe ->OutputChannels;
     *  }
     */
    public uint OutputChannels
    {
        get => _outputChan;
        internal set => _outputChan = value <= maxStageChannels ? value : _outputChan;
    }

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

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public enum StageLoc
{
    AtBegin,
    AtEnd,
}
