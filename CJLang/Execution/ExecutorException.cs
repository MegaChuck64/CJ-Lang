
namespace CJLang.Execution;

public class ExecutorException(string message, int line = -1) : Exception(message)
{
    public int Line { get; set; } = line;

    public override string ToString() =>
        "Error occurred on line " + (Line + 1) + ": " + Message;
    
}