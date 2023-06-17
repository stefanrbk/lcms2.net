namespace lcms2;

public unsafe class BoxPtr<T>(T* ptr) where T : struct
{
    public T* Ptr = ptr;

    public static implicit operator T*(BoxPtr<T>? ptr) =>
        ptr is not null
            ? ptr.Ptr
            : null;
    public T this[int index] =>
        ptr[index];
    public T this[uint index] =>
        ptr[index];
}
