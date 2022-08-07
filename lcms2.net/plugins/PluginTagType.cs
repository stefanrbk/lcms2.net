using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Tag type handler
/// </summary>
/// <remarks>
///     Implements the <c>cmsPluginTagType</c> struct.</remarks>
public sealed class PluginTagType : Plugin
{
    public ITagTypeHandler handler;
    public PluginTagType(Signature magic, uint expectedVersion, Signature type, ITagTypeHandler handler)
        : base(magic, expectedVersion, type)
    {
        this.handler = handler;
    }

    internal static bool RegisterPlugin(Context? context, PluginTagType? plugin)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
///     Tag type handler
/// </summary>
/// <remarks>
///     Each type is free to return anything it wants, and it is up to the caller to
///     know in advance what is the type contained in the tag.<br />
///     Implements the <c>cmsTagTypeHandler</c> struct.</remarks>
public interface ITagTypeHandler
{
    /// <summary>
    ///     Signature of the type
    /// </summary>
    Signature Signature { get; }

    /// <summary>
    ///     Additional parameter used by the calling thread
    /// </summary>
    Context Context { get; }

    /// <summary>
    ///     Additional parameter used by the calling thread
    /// </summary>
    uint ICCVersion { get; }

    /// <summary>
    ///     Allocates and reads items.
    /// </summary>
    (object Value, int Count)? Read(ITagTypeHandler handler, Stream io, int sizeOfTag);

    /// <summary>
    ///     Writes n Items
    /// </summary>
    bool Write(ITagTypeHandler handler, Stream io, object value, int numItems);

    /// <summary>
    ///     Duplicate an item or array of items
    /// </summary>
    object? Duplicate(ITagTypeHandler handler, object value, int num);

    /// <summary>
    ///     Free all resources
    /// </summary>
    void Free(ITagTypeHandler handler, object value);
}

public class TagTypeLinkedList
{
    internal ITagTypeHandler? factory;

    internal TagTypeLinkedList? next = null;
}