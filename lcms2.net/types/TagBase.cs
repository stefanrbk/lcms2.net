namespace lcms2.types;

#if PLUGIN
public
#else
internal
#endif
    record TagBase(Signature Sig, byte[] Reserved)
{
}
