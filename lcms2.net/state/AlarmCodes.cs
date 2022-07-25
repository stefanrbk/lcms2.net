namespace lcms2.state;

internal class AlarmCodes
{
    private ushort[] alarmCodes = new ushort[Lcms2.MaxChannels];

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        var from = src is not null ? (AlarmCodes?)src.chunks[(int)Chunks.AlarmCodesContext] : alarmCodesChunk;

        ctx.chunks[(int)Chunks.Logger] = from;
    }

    private AlarmCodes() { }

    internal static AlarmCodes global = new() { alarmCodes = (ushort[])DefaultAlarmCodes!.Clone() };
    private readonly static AlarmCodes alarmCodesChunk = new() { alarmCodes = DefaultAlarmCodes };

    internal static readonly ushort[] DefaultAlarmCodes = new ushort[Lcms2.MaxChannels] { 0x7F00, 0x7F00, 0x7F00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
}
