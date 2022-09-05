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

public struct ScreeningChannel
{
    #region Fields

    public double Frequency;
    public double ScreenAngle;
    public SpotShape SpotShape;

    #endregion Fields
}

public class Screening : ICloneable
{
    #region Fields

    public ScreeningChannel[] Channels;
    public uint Flags;

    #endregion Fields

    #region Public Constructors

    public Screening(uint flags, int numChannels)
    {
        Flags = flags;
        Channels = new ScreeningChannel[numChannels];
    }

    #endregion Public Constructors

    #region Properties

    public int NumChannels
    {
        get =>
            Channels.Length;
        set
        {
            var temp = new ScreeningChannel[value];

            if (Channels.Length > value)
                Channels[..value].CopyTo(temp.AsSpan());
            else
                Channels.CopyTo(temp[..Channels.Length].AsSpan());

            Channels = temp;
        }
    }

    #endregion Properties

    #region Public Methods

    public object Clone()
    {
        Screening result = new(Flags, NumChannels);

        Channels.CopyTo(result.Channels, 0);

        return result;
    }

    #endregion Public Methods
}

public enum SpotShape
{
    Unknown = 0,
    PrinterDefault = 1,
    Round = 2,
    Diamond = 3,
    Ellipse = 4,
    Line = 5,
    Square = 6,
    Cross = 7,
}
