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

public sealed class Dictionary : ICloneable, IDisposable, IEnumerable<DictionaryEntry>
{
    #region Public Constructors

    public Dictionary(object? state = null)
    {
        /** Original Code (cmsnamed.c line: 867)
         **
         ** // Allocate an empty dictionary
         ** cmsHANDLE CMSEXPORT cmsDictAlloc(cmsContext ContextID)
         ** {
         **     _cmsDICT* dict = (_cmsDICT*) _cmsMallocZero(ContextID, sizeof(_cmsDICT));
         **     if (dict == NULL) return NULL;
         **
         **     dict ->ContextID = ContextID;
         **     return (cmsHANDLE) dict;
         **
         ** }
         **/

        Head = null;
        StateContainer = state;
    }

    #endregion Public Constructors

    #region Properties

    public DictionaryEntry? Head { get; internal set; }
    public object? StateContainer { get; internal set; }

    #endregion Properties

    #region Public Methods

    public void AddEntry(string name, string value, in Mlu? displayName, in Mlu? displayValue) =>
        /** Original Code (cmsnamed.c line: 914)
         **
         ** // Add a new entry to the linked list
         ** cmsBool CMSEXPORT cmsDictAddEntry(cmsHANDLE hDict, const wchar_t* Name, const wchar_t* Value, const cmsMLU *DisplayName, const cmsMLU *DisplayValue)
         ** {
         **     _cmsDICT* dict = (_cmsDICT*) hDict;
         **     cmsDICTentry *entry;
         **
         **     _cmsAssert(dict != NULL);
         **     _cmsAssert(Name != NULL);
         **
         **     entry = (cmsDICTentry*) _cmsMallocZero(dict ->ContextID, sizeof(cmsDICTentry));
         **     if (entry == NULL) return FALSE;
         **
         **     entry ->DisplayName  = cmsMLUdup(DisplayName);
         **     entry ->DisplayValue = cmsMLUdup(DisplayValue);
         **     entry ->Name         = DupWcs(dict ->ContextID, Name);
         **     entry ->Value        = DupWcs(dict ->ContextID, Value);
         **
         **     entry ->Next = dict ->head;
         **     dict ->head = entry;
         **
         **     return TRUE;
         ** }
         **/
        Head = new DictionaryEntry(name, value)
        {
            DisplayName = (Mlu?)displayName?.Clone(),
            DisplayValue = (Mlu?)displayValue?.Clone(),

            Next = Head,
        };

    public object Clone()
    {
        /** Original Code (cmsnamed.c line: 938)
         **
         ** // Duplicates an existing dictionary
         ** cmsHANDLE CMSEXPORT cmsDictDup(cmsHANDLE hDict)
         ** {
         **     _cmsDICT* old_dict = (_cmsDICT*) hDict;
         **     cmsHANDLE hNew;
         **     cmsDICTentry *entry;
         **
         **     _cmsAssert(old_dict != NULL);
         **
         **     hNew  = cmsDictAlloc(old_dict ->ContextID);
         **     if (hNew == NULL) return NULL;
         **
         **     // Walk the list freeing all nodes
         **     entry = old_dict ->head;
         **     while (entry != NULL) {
         **
         **         if (!cmsDictAddEntry(hNew, entry ->Name, entry ->Value, entry ->DisplayName, entry ->DisplayValue)) {
         **
         **             cmsDictFree(hNew);
         **             return NULL;
         **         }
         **
         **         entry = entry -> Next;
         **     }
         **
         **     return hNew;
         ** }
         **/

        var hNew = new Dictionary(StateContainer);
        var entry = Head;
        while (entry is not null)
        {
            hNew.AddEntry(entry.Name, entry.Value, entry.DisplayName, entry.DisplayValue);
            entry = entry.Next;
        }

        return hNew;
    }

    public void Dispose()
    {
        /** Original Code (cmsnamed.c line: 878)
         **
         ** // Dispose resources
         ** void CMSEXPORT cmsDictFree(cmsHANDLE hDict)
         ** {
         **     _cmsDICT* dict = (_cmsDICT*) hDict;
         **     cmsDICTentry *entry, *next;
         **
         **     _cmsAssert(dict != NULL);
         **
         **     // Walk the list freeing all nodes
         **     entry = dict ->head;
         **     while (entry != NULL) {
         **
         **             if (entry ->DisplayName  != NULL) cmsMLUfree(entry ->DisplayName);
         **             if (entry ->DisplayValue != NULL) cmsMLUfree(entry ->DisplayValue);
         **             if (entry ->Name != NULL) _cmsFree(dict ->ContextID, entry -> Name);
         **             if (entry ->Value != NULL) _cmsFree(dict ->ContextID, entry -> Value);
         **
         **             // Don't fall in the habitual trap...
         **             next = entry ->Next;
         **             _cmsFree(dict ->ContextID, entry);
         **
         **             entry = next;
         **     }
         **
         **     _cmsFree(dict ->ContextID, dict);
         ** }
         **/

        var entry = Head;
        while (entry is not null)
        {
            entry.DisplayName?.Dispose();
            entry.DisplayValue?.Dispose();
            entry = entry.Next;
        }
    }

    public IEnumerator<DictionaryEntry> GetEnumerator()
    {
        /** Original Code (cmsnamed.c line: 966)
         **
         ** // Get a pointer to the linked list
         ** const cmsDICTentry* CMSEXPORT cmsDictGetEntryList(cmsHANDLE hDict)
         ** {
         **     _cmsDICT* dict = (_cmsDICT*) hDict;
         **
         **     if (dict == NULL) return NULL;
         **     return dict ->head;
         ** }
         **
         ** // Helper For external languages
         ** const cmsDICTentry* CMSEXPORT cmsDictNextEntry(const cmsDICTentry* e)
         ** {
         **      if (e == NULL) return NULL;
         **      return e ->Next;
         ** }
         **/

        var entry = Head;
        while (entry is not null)
        {
            yield return entry;
            entry = entry.Next;
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
        GetEnumerator();

    #endregion Public Methods
}
