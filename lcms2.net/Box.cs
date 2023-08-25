namespace lcms2;

public class Box<T>(T value) where T : struct
{
    public T Value = value;

    public static implicit operator T(Box<T> box) =>
        box.Value;
}