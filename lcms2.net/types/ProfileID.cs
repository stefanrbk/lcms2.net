using System.Runtime.InteropServices;

namespace lcms2.types;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ProfileID
{
    [FieldOffset(0)]
    internal fixed byte id8[16];

    [FieldOffset(0)]
    internal fixed ushort id16[8];

    [FieldOffset(0)]
    internal fixed uint id32[4];
}
