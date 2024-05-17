using CJLang.Execution;
using CJLang.Lang;

namespace CJLang.Instructions;

[Instruction("print", "Prints a variable or literal")]
internal class PrintInstruction : Instruction
{
    internal static readonly string[] separator = ["\\n"];

    public override void Run(CJFunc currentFunc, string line, int globalLineNum, int localLineNum)
    {
        //print("Hello, ", userName, ". You are ", userAgeStr, " years old.\n")
        var splt = line.Split(['(']);
        var prmpt = splt[1].Split([')'])[0];

        var str = Helper.GetStrFromConcat(currentFunc, prmpt);

        var lines = str.Split(separator, StringSplitOptions.None);
        foreach (var l in lines)
            Console.WriteLine(l);

    }
}