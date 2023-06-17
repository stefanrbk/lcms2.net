namespace lcms2;

public unsafe class BoxPtr2<T>(T** ptr) where T : struct
{
    public T** Ptr = ptr;

    public static implicit operator T**(BoxPtr2<T> ptr) =>
        ptr.Ptr;
    public T* this[int index] =>
        ptr[index];
    public T* this[uint index] =>
        ptr[index];
}
