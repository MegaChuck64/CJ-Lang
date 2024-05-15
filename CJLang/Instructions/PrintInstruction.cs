using CJLang.Lang;

namespace CJLang.Instructions;

[Instruction("print", "Prints a variable or literal")]
internal class PrintInstruction : Instruction
{
    public override void Run(CJFunc currentFunc, string line, int globalLineNum, int localLineNum)
    {
        //print("Hello, ", userName, ". You are ", userAgeStr, " years old.\n")
        var splt = line.Split(['(']);
        var prmpt = splt[1].Split([')'])[0];

        var str = CJProg.GetStrFromConcat(currentFunc, prmpt);

        var lines = str.Split(new string[] { "\\n" }, StringSplitOptions.None);
        foreach (var l in lines)
            Console.WriteLine(l);

    }
}