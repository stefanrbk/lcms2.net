using System.Runtime.InteropServices;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

public class TagTypeLinkedList
{
    internal TagTypeHandler? factory;

    internal TagTypeLinkedList? next = null;
}
