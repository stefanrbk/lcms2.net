namespace lcms2.state;

internal sealed class AlarmCodes
{
    internal static readonly ushort[] defaultAlarmCodes = new ushort[Lcms2.MaxChannels] { 0x7F00, 0x7F00, 0x7F00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    internal static AlarmCodes global = new() { alarmCodes = (ushort[])defaultAlarmCodes!.Clone() };
    internal ushort[] alarmCodes = new ushort[Lcms2.MaxChannels];

    private static readonly AlarmCodes _alarmCodesChunk = new() { alarmCodes = defaultAlarmCodes };

    private AlarmCodes()
    { }

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        AlarmCodes from = (AlarmCodes?)src?.chunks[(int)Chunks.AlarmCodesContext] ?? _alarmCodesChunk;

        ctx.chunks[(int)Chunks.AlarmCodesContext] = from;
    }
}
