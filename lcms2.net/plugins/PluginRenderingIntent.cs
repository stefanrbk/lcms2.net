//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
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
using lcms2.types;

namespace lcms2.plugins;

public class PluginRenderingIntent : PluginBase
{
    public uint Intent { get; internal set; }
    public CmsIntentFn Link { get; internal set; }
    public string Description { get; internal set; }

    public PluginRenderingIntent(
        uint expectedVersion,
        Signature magic,
        Signature type,
        uint intent,
        CmsIntentFn link,
        string desc)

        : base(expectedVersion, magic, type)
    {
        Link = link;
        Description = desc.Length > 255
            ? desc.Remove(255)
            : desc;
        Intent = intent;
    }
}

public class CmsIntentsList
{
    public uint Intent;
    public string Description;
    public CmsIntentFn Link;
    public CmsIntentsList? Next;

    public CmsIntentsList(uint intent, string description, CmsIntentFn link, CmsIntentsList? next = null)
    {
        Intent = intent;
        Description = description;
        Link = link;
        Next = next;
    }
}

public delegate Pipeline CmsIntentFn(
    Context? ContextID, uint nProfiles, uint[] Intents,
    IccProfile[] hProfiles, bool[] BPC,
    double[] AdaptationStates, uint dwFlags);
