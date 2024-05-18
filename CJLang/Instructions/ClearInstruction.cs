using CJLang.Execution;
using CJLang.Lang;

namespace CJLang.Instructions;


//clear terminal
[Instruction("clear", "Clears the terminal")]
internal class ClearInstruction : Instruction
{
    internal static readonly char[] separator = ['(', ' ', ')'];

    public override void Run(CJFunc currentFunc, string line, int globalLineNum, int localLineNum)
    {
        var splt = line.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        //clear()
        if (splt.Length == 1 && splt[0] == "clear")
        {
            Console.Clear();
        }
        else
            throw new ExecutorException("Invalid usage of clear. Expected 'clear()'", globalLineNum);
    }
}