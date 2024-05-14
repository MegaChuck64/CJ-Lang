using CJLang.Lang;

namespace CJLang.Instructions;


//clear terminal
[Instruction("clear", "Clears the terminal")]
internal class ClearInstruction : Instruction
{
    public override void Run(CJProg prog, CJFunc currentFunc, string line)
    {
        var splt = line.Split(new[] { '(', ' ', ')' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        //clear()
        if (splt.Length == 1 && splt[0] == "clear")
        {
            Console.Clear();
        }
        else
            throw new Exception("Invalid usage of clear. Expected 'clear()'");
    }
}