﻿using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

public delegate Signature TagTypeDecider(double iccVersion, ref object data);

/// <summary>
///     Tag identification plugin
/// </summary>
/// <remarks>
///     Plugin implements a single tag<br />
///     Implements the <c>cmsPluginTag</c> struct.</remarks>
public sealed class PluginTag
    : Plugin
{
    public Signature Signature;
    public TagDescriptor Descriptor;

    public PluginTag(Signature magic, uint expectedVersion, Signature type, Signature signature, TagDescriptor descriptor)
        : base(magic, expectedVersion, type)
    {
        Signature = signature;
        Descriptor = descriptor;
    }

    internal static bool RegisterPlugin(Context? context, PluginTag? plugin)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
///     Tag identification plugin descriptor
/// </summary>
/// <remarks>
///     Implements the <c>cmsTagDescriptor</c> struct.</remarks>
public class TagDescriptor
{
    /// <summary>
    ///     If this tag needs an array, how many elements should be kept
    /// </summary>
    public int ElementCount;
    /// <summary>
    ///     For reading
    /// </summary>
    public Signature[] SupportedTypes;
    /// <summary>
    ///     For writing
    /// </summary>
    public TagTypeDecider DecideType;

    public TagDescriptor(int elementCount, int numSupportedTypes, TagTypeDecider decider)
    {
        ElementCount = elementCount;
        SupportedTypes = new Signature[numSupportedTypes];
        DecideType = decider;
    }
}
public class TagLinkedList
{
    internal Signature signature;
    internal TagDescriptor descriptor;

    internal TagLinkedList? next = null;
}
