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

public class Sequence : ICloneable, IDisposable
{
    #region Fields

    public ProfileSequenceDescription[] Seq;
    public object? StateContainer;
    private bool _disposed;

    #endregion Fields

    #region Private Constructors

    private Sequence(object? state, int count)
    {
        StateContainer = state;
        Seq = new ProfileSequenceDescription[count];

        _disposed = false;
    }

    #endregion Private Constructors

    #region Properties

    public int SeqCount =>
        Seq.Length;

    #endregion Properties

    #region Public Methods

    public static Sequence? Alloc(object? state, int count)
    {
        /** Original Code (cmsnamed.c line: 769)
         **
         ** cmsSEQ* CMSEXPORT cmsAllocProfileSequenceDescription(cmsContext ContextID, cmsUInt32Number n)
         ** {
         **     cmsSEQ* Seq;
         **     cmsUInt32Number i;
         **
         **     if (n == 0) return NULL;
         **
         **     // In a absolutely arbitrary way, I hereby decide to allow a maxim of 255 profiles linked
         **     // in a devicelink. It makes not sense anyway and may be used for exploits, so let's close the door!
         **     if (n > 255) return NULL;
         **
         **     Seq = (cmsSEQ*) _cmsMallocZero(ContextID, sizeof(cmsSEQ));
         **     if (Seq == NULL) return NULL;
         **
         **     Seq -> ContextID = ContextID;
         **     Seq -> seq      = (cmsPSEQDESC*) _cmsCalloc(ContextID, n, sizeof(cmsPSEQDESC));
         **     Seq -> n        = n;
         **
         **     if (Seq -> seq == NULL) {
         **         _cmsFree(ContextID, Seq);
         **         return NULL;
         **     }
         **
         **     for (i=0; i < n; i++) {
         **         Seq -> seq[i].Manufacturer = NULL;
         **         Seq -> seq[i].Model        = NULL;
         **         Seq -> seq[i].Description  = NULL;
         **     }
         **
         **     return Seq;
         ** }
         **/

        if (count is 0 or > 255) return null;

        return new Sequence(state, count);
    }

    public object Clone()
    {
        /** Original Code (cmsnamed.c line: 815)
         **
         ** cmsSEQ* CMSEXPORT cmsDupProfileSequenceDescription(const cmsSEQ* pseq)
         ** {
         **     cmsSEQ *NewSeq;
         **     cmsUInt32Number i;
         **
         **     if (pseq == NULL)
         **         return NULL;
         **
         **     NewSeq = (cmsSEQ*) _cmsMalloc(pseq -> ContextID, sizeof(cmsSEQ));
         **     if (NewSeq == NULL) return NULL;
         **
         **
         **     NewSeq -> seq      = (cmsPSEQDESC*) _cmsCalloc(pseq ->ContextID, pseq ->n, sizeof(cmsPSEQDESC));
         **     if (NewSeq ->seq == NULL) goto Error;
         **
         **     NewSeq -> ContextID = pseq ->ContextID;
         **     NewSeq -> n        = pseq ->n;
         **
         **     for (i=0; i < pseq->n; i++) {
         **
         **         memmove(&NewSeq ->seq[i].attributes, &pseq ->seq[i].attributes, sizeof(cmsUInt64Number));
         **
         **         NewSeq ->seq[i].deviceMfg   = pseq ->seq[i].deviceMfg;
         **         NewSeq ->seq[i].deviceModel = pseq ->seq[i].deviceModel;
         **         memmove(&NewSeq ->seq[i].ProfileID, &pseq ->seq[i].ProfileID, sizeof(cmsProfileID));
         **         NewSeq ->seq[i].technology  = pseq ->seq[i].technology;
         **
         **         NewSeq ->seq[i].Manufacturer = cmsMLUdup(pseq ->seq[i].Manufacturer);
         **         NewSeq ->seq[i].Model        = cmsMLUdup(pseq ->seq[i].Model);
         **         NewSeq ->seq[i].Description  = cmsMLUdup(pseq ->seq[i].Description);
         **
         **     }
         **
         **     return NewSeq;
         **
         ** Error:
         **
         **     cmsFreeProfileSequenceDescription(NewSeq);
         **     return NULL;
         ** }
         **/

        Sequence result = new(StateContainer, SeqCount);

        for (var i = 0; i < SeqCount; i++)
            result.Seq[i] = (ProfileSequenceDescription)Seq[i].Clone();

        return result;
    }

    public void Dispose()
    {
        /** Original Code (cmsnamed.c line: 801)
         **
         ** void CMSEXPORT cmsFreeProfileSequenceDescription(cmsSEQ* pseq)
         ** {
         **     cmsUInt32Number i;
         **
         **     for (i=0; i < pseq ->n; i++) {
         **         if (pseq ->seq[i].Manufacturer != NULL) cmsMLUfree(pseq ->seq[i].Manufacturer);
         **         if (pseq ->seq[i].Model != NULL) cmsMLUfree(pseq ->seq[i].Model);
         **         if (pseq ->seq[i].Description != NULL) cmsMLUfree(pseq ->seq[i].Description);
         **     }
         **
         **     if (pseq ->seq != NULL) _cmsFree(pseq ->ContextID, pseq ->seq);
         **     _cmsFree(pseq -> ContextID, pseq);
         ** }
         **/

        if (!_disposed)
        {
            foreach (var seq in Seq)
                seq?.Dispose();

            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    #endregion Public Methods
}
