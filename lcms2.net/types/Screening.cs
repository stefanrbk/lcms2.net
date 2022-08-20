namespace lcms2.types;

public struct ScreeningChannel
{
    public double Frequency;
    public double ScreenAngle;
    public SpotShape SpotShape;
}

public class Screening: ICloneable
{
    public ScreeningChannel[] Channels;
    public uint Flags;

    public Screening(uint flags, int numChannels)
    {
        Flags = flags;
        Channels = new ScreeningChannel[numChannels];
    }

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

    public object Clone()
    {
        Screening result = new(Flags, NumChannels);

        Channels.CopyTo(result.Channels, 0);

        return result;
    }
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
