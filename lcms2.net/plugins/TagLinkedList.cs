using lcms2.types;

namespace lcms2.plugins;

public class TagLinkedList
{
    internal Signature signature;
    internal TagDescriptor descriptor;

    internal TagLinkedList? next = null;
}
