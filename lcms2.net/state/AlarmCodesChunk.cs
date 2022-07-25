namespace lcms2.state;

internal class AlarmCodesChunk
{
    private ushort[] alarmCodes = new ushort[Lcms2.MaxChannels];

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        var from = src is not null ? (AlarmCodesChunk?)src.chunks[(int)Chunks.AlarmCodesContext] : alarmCodesChunk;

        ctx.chunks[(int)Chunks.Logger] = from;
    }

    private AlarmCodesChunk() { }

    internal static AlarmCodesChunk global = new() { alarmCodes = (ushort[])DefaultAlarmCodes!.Clone() };
    private readonly static AlarmCodesChunk alarmCodesChunk = new() { alarmCodes = DefaultAlarmCodes };

    internal static readonly ushort[] DefaultAlarmCodes = new ushort[Lcms2.MaxChannels] { 0x7F00, 0x7F00, 0x7F00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
}
