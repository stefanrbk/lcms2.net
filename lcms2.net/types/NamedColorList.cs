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

public class NamedColorList : ICloneable, IDisposable
{
    #region Fields

    internal uint colorantCount;

    internal List<NamedColor> list;
    internal string prefix;
    internal object? state;
    internal string suffix;

    private bool _disposed;

    #endregion Fields

    #region Public Constructors

    public NamedColorList(object? state, int initialCapacity, uint colorantCount, string prefix, string suffix)
    {
        /** Original Code (cmsnamed.c line: 542)
         **
         ** // Allocate a list for n elements
         ** cmsNAMEDCOLORLIST* CMSEXPORT cmsAllocNamedColorList(cmsContext ContextID, cmsUInt32Number n, cmsUInt32Number ColorantCount, const char* Prefix, const char* Suffix)
         ** {
         **     cmsNAMEDCOLORLIST* v = (cmsNAMEDCOLORLIST*) _cmsMallocZero(ContextID, sizeof(cmsNAMEDCOLORLIST));
         **
         **     if (v == NULL) return NULL;
         **
         **     v ->List      = NULL;
         **     v ->nColors   = 0;
         **     v ->ContextID  = ContextID;
         **
         **     while (v -> Allocated < n) {
         **         if (!GrowNamedColorList(v)) {
         **             cmsFreeNamedColorList(v);
         **             return NULL;
         **         }
         **     }
         **
         **     strncpy(v ->Prefix, Prefix, sizeof(v ->Prefix)-1);
         **     strncpy(v ->Suffix, Suffix, sizeof(v ->Suffix)-1);
         **     v->Prefix[32] = v->Suffix[32] = 0;
         **
         **     v -> ColorantCount = ColorantCount;
         **
         **     return v;
         ** }
         **/

        this.state = state;
        list = new List<NamedColor>(initialCapacity);

        this.prefix = prefix;
        this.suffix = suffix;

        this.colorantCount = colorantCount;
    }

    #endregion Public Constructors

    #region Properties

    public int ColorCount =>
        /** Original Code (cmsnamed.c line: 637)
         **
         ** // Returns number of elements
         ** cmsUInt32Number CMSEXPORT cmsNamedColorCount(const cmsNAMEDCOLORLIST* NamedColorList)
         ** {
         **      if (NamedColorList == NULL) return 0;
         **      return NamedColorList ->nColors;
         ** }
         **/
        list.Count;

    #endregion Properties

    #region Public Methods

    public void Append(string name, ushort[]? pcs, ushort[]? colorant)
    {
        /** Original Code (cmsnamed.c line: 604)
         **
         ** // Append a color to a list. List pointer may change if reallocated
         ** cmsBool  CMSEXPORT cmsAppendNamedColor(cmsNAMEDCOLORLIST* NamedColorList,
         **                                        const char* Name,
         **                                        cmsUInt16Number PCS[3], cmsUInt16Number Colorant[cmsMAXCHANNELS])
         ** {
         **     cmsUInt32Number i;
         **
         **     if (NamedColorList == NULL) return FALSE;
         **
         **     if (NamedColorList ->nColors + 1 > NamedColorList ->Allocated) {
         **         if (!GrowNamedColorList(NamedColorList)) return FALSE;
         **     }
         **
         **     for (i=0; i < NamedColorList ->ColorantCount; i++)
         **         NamedColorList ->List[NamedColorList ->nColors].DeviceColorant[i] = Colorant == NULL ? (cmsUInt16Number)0 : Colorant[i];
         **
         **     for (i=0; i < 3; i++)
         **         NamedColorList ->List[NamedColorList ->nColors].PCS[i] = PCS == NULL ? (cmsUInt16Number) 0 : PCS[i];
         **
         **     if (Name != NULL) {
         **
         **         strncpy(NamedColorList ->List[NamedColorList ->nColors].Name, Name, cmsMAX_PATH-1);
         **         NamedColorList ->List[NamedColorList ->nColors].Name[cmsMAX_PATH-1] = 0;
         **
         **     }
         **     else
         **         NamedColorList ->List[NamedColorList ->nColors].Name[0] = 0;
         **
         **
         **     NamedColorList ->nColors++;
         **     return TRUE;
         ** }
         **/

        var color = new NamedColor(name);

        for (var i = 0; i < colorantCount; i++)
            color.deviceColorant[i] = colorant?[i] ?? 0;

        for (var i = 0; i < 3; i++)
            color.pcs[i] = pcs?[i] ?? 0;

        list.Add(color);
    }

    public object Clone()
    {
        /** Original Code (cmsnamed.c line: 577)
         **
         ** cmsNAMEDCOLORLIST* CMSEXPORT cmsDupNamedColorList(const cmsNAMEDCOLORLIST* v)
         ** {
         **     cmsNAMEDCOLORLIST* NewNC;
         **
         **     if (v == NULL) return NULL;
         **
         **     NewNC= cmsAllocNamedColorList(v ->ContextID, v -> nColors, v ->ColorantCount, v ->Prefix, v ->Suffix);
         **     if (NewNC == NULL) return NULL;
         **
         **     // For really large tables we need this
         **     while (NewNC ->Allocated < v ->Allocated){
         **         if (!GrowNamedColorList(NewNC))
         **         {
         **             cmsFreeNamedColorList(NewNC);
         **             return NULL;
         **         }
         **     }
         **
         **     memmove(NewNC ->Prefix, v ->Prefix, sizeof(v ->Prefix));
         **     memmove(NewNC ->Suffix, v ->Suffix, sizeof(v ->Suffix));
         **     NewNC ->ColorantCount = v ->ColorantCount;
         **     memmove(NewNC->List, v ->List, v->nColors * sizeof(_cmsNAMEDCOLOR));
         **     NewNC ->nColors = v ->nColors;
         **     return NewNC;
         ** }
         **/

        var newNcl = new NamedColorList(state, list.Count, colorantCount, prefix, suffix);

        foreach (var item in list)
            newNcl.list.Add((NamedColor)item.Clone());

        return newNcl;
    }

    public void Dispose()
    {
        /** Original Code (cmsnamed.c line: 569)
         **
         ** // Free a list
         ** void CMSEXPORT cmsFreeNamedColorList(cmsNAMEDCOLORLIST* v)
         ** {
         **     if (v == NULL) return;
         **     if (v ->List) _cmsFree(v ->ContextID, v ->List);
         **     _cmsFree(v ->ContextID, v);
         ** }
         **/

        if (!_disposed)
        {
            list.Clear();
            GC.SuppressFinalize(this);
        }
        _disposed = true;
    }

    public int Index(string name) =>
        /** Original Code (cmsnamed.c line: 671)
         **
         ** // Search for a given color name (no prefix or suffix)
         ** cmsInt32Number CMSEXPORT cmsNamedColorIndex(const cmsNAMEDCOLORLIST* NamedColorList, const char* Name)
         ** {
         **     cmsUInt32Number i;
         **     cmsUInt32Number n;
         **
         **     if (NamedColorList == NULL) return -1;
         **     n = cmsNamedColorCount(NamedColorList);
         **     for (i=0; i < n; i++) {
         **         if (cmsstrcasecmp(Name,  NamedColorList->List[i].Name) == 0)
         **             return (cmsInt32Number) i;
         **     }
         **
         **     return -1;
         ** }
         **/
        list.FindIndex(nc => nc.name == name);

    public bool Info(int numColor,
                     out string name,
                     out string prefix,
                     out string suffix,
                     out ushort[] pcs,
                     out ushort[] colorant)
    {
        /** Original Code (cmsnamed.c line: 644)
         **
         ** // Info about a given color
         ** cmsBool  CMSEXPORT cmsNamedColorInfo(const cmsNAMEDCOLORLIST* NamedColorList, cmsUInt32Number nColor,
         **                                      char* Name,
         **                                      char* Prefix,
         **                                      char* Suffix,
         **                                      cmsUInt16Number* PCS,
         **                                      cmsUInt16Number* Colorant)
         ** {
         **     if (NamedColorList == NULL) return FALSE;
         **
         **     if (nColor >= cmsNamedColorCount(NamedColorList)) return FALSE;
         **
         **     // strcpy instead of strncpy because many apps are using small buffers
         **     if (Name) strcpy(Name, NamedColorList->List[nColor].Name);
         **     if (Prefix) strcpy(Prefix, NamedColorList->Prefix);
         **     if (Suffix) strcpy(Suffix, NamedColorList->Suffix);
         **     if (PCS)
         **         memmove(PCS, NamedColorList ->List[nColor].PCS, 3*sizeof(cmsUInt16Number));
         **
         **     if (Colorant)
         **         memmove(Colorant, NamedColorList ->List[nColor].DeviceColorant,
         **                                 sizeof(cmsUInt16Number) * NamedColorList ->ColorantCount);
         **
         **
         **     return TRUE;
         ** }
         **/

        name = prefix = suffix = String.Empty;
        pcs = colorant = Array.Empty<ushort>();

        if (numColor >= list.Count) return false;

        name = list[numColor].name;
        prefix = this.prefix;
        suffix = this.suffix;
        pcs = (ushort[])list[numColor].pcs.Clone();
        colorant = (ushort[])list[numColor].deviceColorant.Clone();

        return true;
    }

    #endregion Public Methods
}
