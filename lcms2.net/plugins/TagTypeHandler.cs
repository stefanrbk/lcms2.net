using lcms2.state;
using lcms2.types;

namespace lcms2.plugins;

/// <summary>
///     Tag type handler
/// </summary>
/// <remarks>
///     Each type is free to return anything it wants, and it is up to the caller to know in advance
///     what is the type contained in the tag. <br/> Implements the <c>cmsTagTypeHandler</c> struct.
/// </remarks>
public abstract partial class TagTypeHandler
{
    protected TagTypeHandler(Signature signature, Context? context, uint iCCVersion)
    {
        Signature = signature;
        Context = context;
        ICCVersion = iCCVersion;
    }

    /// <summary>
    ///     Additional parameter used by the calling thread
    /// </summary>
    public virtual Context? Context { get; }

    /// <summary>
    ///     Additional parameter used by the calling thread
    /// </summary>
    public virtual uint ICCVersion { get; }

    /// <summary>
    ///     Signature of the type
    /// </summary>
    public virtual Signature Signature { get; }

    /// <summary>
    ///     Duplicate an item or array of items
    /// </summary>
    public abstract object? Duplicate(object value, int num);

    /// <summary>
    ///     Free all resources
    /// </summary>
    public abstract void Free(object value);

    /// <summary>
    ///     Allocates and reads items.
    /// </summary>
    public abstract unsafe object? Read(Stream io, int sizeOfTag, out int numItems);

    /// <summary>
    ///     Writes n Items
    /// </summary>
    public abstract unsafe bool Write(Stream io, object value, int numItems);
}
