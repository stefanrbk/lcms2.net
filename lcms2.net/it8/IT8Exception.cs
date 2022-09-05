namespace lcms2.it8;

[Serializable]
public class IT8Exception: Exception
{
    protected IT8Exception(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    public IT8Exception(string? message)
        : base(message) { }

    public IT8Exception(Stack<StreamReader> fileStack, int lineNo)
        : base($"{(fileStack.Peek().BaseStream is FileStream fs ? fs.Name : "Memory")}: Line {lineNo}, An error has occurred") { }

    public IT8Exception(Stack<StreamReader> fileStack, int lineNo, string? message)
        : base($"{(fileStack.Peek().BaseStream is FileStream fs ? fs.Name : "Memory")}: Line {lineNo}, {message}") { }

    public IT8Exception(Stack<StreamReader> fileStack, int lineNo, string? message, Exception? innerException)
        : base($"{(fileStack.Peek().BaseStream is FileStream fs ? fs.Name : "Memory")}: Line {lineNo}, {message}", innerException) { }
}
