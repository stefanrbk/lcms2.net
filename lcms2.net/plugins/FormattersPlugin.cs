using System.Diagnostics;
using System.Runtime.InteropServices;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     This plugin adds new handlers, replacing them if they already exist.
/// </summary>
/// <remarks>
///     Implements the <c>cmsPluginFormatters</c> typedef.</remarks>
public sealed class FormattersPlugin : Plugin
{
    public FormatterFactory FormattersFactory;

    public FormattersPlugin(Signature magic, uint expectedVersion, Signature type, FormatterFactory formatterFactory)
        : base(magic, expectedVersion, type) =>
        FormattersFactory = formatterFactory;

    internal static bool RegisterPlugin(Context? context, FormattersPlugin? plugin)
    {
        var ctx = Context.GetFormattersPlugin(context);

        if (plugin is null)
        {
            ctx.FactoryList = null;
            return true;
        }

        ctx.FactoryList = new FormattersFactoryList(plugin.FormattersFactory, ctx.FactoryList);

        return true;
    }
}

/// <summary>
///     Formatter for <see cref="ushort"/> values.
/// </summary>
/// <remarks>
///     Implements the <c>cmsFormatter16</c> typedef.</remarks>
public delegate byte[] Formatter16(ref Transform cmmCargo, ushort[] values, out byte[] buffer, int stride);

/// <summary>
///     Formatter for <see cref="float"/> values.
/// </summary>
/// <remarks>
///     Implements the <c>cmsFormatterFloat</c> typedef.</remarks>
public delegate byte[] FormatterFloat(ref Transform cmmCargo, float[] values, out byte[] buffer, int stride);

[Flags]
public enum PackFlag
{
    Ushort = 0,
    Float = 1,
}

/// <summary>
///     The requested direction of the <see cref="Formatter"/>.
/// </summary>
/// <remarks>
///     Implements the <c>cmsFormatterDirection</c> enum.</remarks>
public enum FormatterDirection
{
    Input,
    Output,
}

/// <summary>
///     The factory to build a <see cref="Formatter"/> of a specified type.
/// </summary>
/// <remarks>
///     Implements the <c>cmsFormatterFactory</c> typedef.</remarks>
public delegate Formatter FormatterFactory(Signature type, FormatterDirection dir, PackFlag flags);

/// <summary>
///     This type holds a pointer to a formatter that can be either 16 bits or cmsFloat32Number
/// </summary>
/// <remarks>
///     Implements the <c>cmsFormatter</c> union.</remarks>
[StructLayout(LayoutKind.Explicit)]
public struct Formatter
{
    [FieldOffset(0)]
    public Formatter16 Fmt16;
    [FieldOffset(0)]
    public FormatterFloat FmtFloat;
}

internal class FormattersFactoryList
{
    internal FormatterFactory? Factory;

    internal FormattersFactoryList? Next;

    public FormattersFactoryList(FormatterFactory? factory, FormattersFactoryList? next)
    {
        Factory = factory;
        Next = next;
    }
}

internal sealed class FormattersPluginChunk
{
    internal FormattersFactoryList? FactoryList;

    internal static void Alloc(ref Context ctx, in Context? src)
    {
        if (src is not null)
            DupFormatterFactoryList(ref ctx, src);
        else
            ctx.chunks[(int)Chunks.FormattersPlugin] = curvesPluginChunk;
    }

    private FormattersPluginChunk()
    { }

    internal static FormattersPluginChunk global = new();
    private static readonly FormattersPluginChunk curvesPluginChunk = new();

    private static void DupFormatterFactoryList(ref Context ctx, in Context src)
    {
        FormattersPluginChunk newHead = new();
        FormattersFactoryList? anterior = null;
        var head = (FormattersPluginChunk?)src.chunks[(int)Chunks.FormattersPlugin];

        Debug.Assert(head is not null);

        // Walk the list copying all nodes
        for (var entry = head.FactoryList; entry is not null; entry = entry.Next)
        {
            // We want to keep the linked list order, so this is a little bit tricky
            FormattersFactoryList newEntry = new(entry.Factory, null);

            if (anterior is not null)
                anterior.Next = newEntry;

            anterior = newEntry;

            if (newHead.FactoryList is null)
                newHead.FactoryList = newEntry;
        }

        ctx.chunks[(int)Chunks.FormattersPlugin] = newHead;
    }
}
