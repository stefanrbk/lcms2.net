using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

#if PLUGIN
public sealed class PluginTagType
#else
internal sealed class PluginTagType
#endif
    : PluginBase
{
    public ITagTypeHandler handler;
    public PluginTagType(Signature magic, uint expectedVersion, Signature type, ITagTypeHandler handler)
        : base(magic, expectedVersion, type)
    {
        this.handler = handler;
    }
}

#if PLUGIN
public interface ITagTypeHandler
#else
internal interface ITagTypeHandler
#endif
{
    Signature Signature { get; }
    Context Context { get; }
    uint ICCVersion { get; }

    (object Value, int Count)? Read(ITagTypeHandler handler, Stream io, int sizeOfTag);
    bool Write(ITagTypeHandler handler, Stream io, object value, int numItems);
    object? Duplicate(ITagTypeHandler handler, object value, int num);
    void Free(ITagTypeHandler handler, object value);
}

public class TagTypeLinkedList
{
    internal ITagTypeHandler? factory;

    internal TagTypeLinkedList? next = null;
}