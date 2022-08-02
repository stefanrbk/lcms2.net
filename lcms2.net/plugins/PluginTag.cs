using lcms2.types;

namespace lcms2.plugins;

public delegate Signature TagTypeDecider(double iccVersion, ref object data);

public sealed class PluginTag
    : PluginBase
{
    public Signature Signature;
    public TagDescriptor Descriptor;

    public PluginTag(Signature magic, uint expectedVersion, Signature type, Signature signature, TagDescriptor descriptor)
        : base(magic, expectedVersion, type)
    {
        Signature = signature;
        Descriptor = descriptor;
    }
}
public class TagDescriptor
{
    public int ElementCount;
    public Signature[] SupportedTypes;
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
