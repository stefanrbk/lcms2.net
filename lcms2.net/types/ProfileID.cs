using System.Runtime.InteropServices;

namespace lcms2.types;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ProfileID
{
    [FieldOffset(0)]
    internal fixed byte ID8[16];
    [FieldOffset(0)]
    internal fixed ushort ID16[8];
    [FieldOffset(0)]
    internal fixed uint ID32[4];
}