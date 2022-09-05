namespace lcms2.state;

internal sealed class AlarmCodes
{
    internal static readonly ushort[] defaultAlarmCodes = new ushort[maxChannels] { 0x7F00, 0x7F00, 0x7F00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    internal static AlarmCodes global = new() { alarmCodes = (ushort[])defaultAlarmCodes!.Clone() };
    internal ushort[] alarmCodes = new ushort[maxChannels];

    internal static AlarmCodes Default => new() { alarmCodes = defaultAlarmCodes };

    private AlarmCodes()
    { }
}
