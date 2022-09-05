﻿using lcms2.io;
using lcms2.plugins;

namespace lcms2.types.type_handlers;

public class ColorantOrderHandler: TagTypeHandler
{
    public ColorantOrderHandler(Signature sig, object? state = null)
        : base(sig, state, 0) { }

    public ColorantOrderHandler(object? state = null)
        : this(default, state) { }

    public override object? Duplicate(object value, int num) =>
        ((byte[])value).Clone();

    public override void Free(object value)
    { }

    public override object? Read(Stream io, int sizeOfTag, out int numItems)
    {
        numItems = 0;
        if (!io.ReadUInt32Number(out var count)) return null;
        if (count > maxChannels) return null;

        byte[] colorantOrder = new byte[maxChannels];

        // We use FF as end marker
        for (var i = 0; i < maxChannels; i++)
            colorantOrder[i] = 0xFF;

        if (io.Read(colorantOrder, 0, (int)count) != count) return null;

        numItems = 1;
        return colorantOrder;
    }

    public override bool Write(Stream io, object value, int numItems)
    {
        var colorantOrder = (byte[])value;
        int count;

        // Get the length
        for (var i = count = 0; i < maxChannels; i++)
            if (colorantOrder[i] != 0xFF) count++;

        if (!io.Write(count)) return false;

        var sz = count * sizeof(byte);
        io.Write(colorantOrder, 0, sz);

        return true;
    }
}
