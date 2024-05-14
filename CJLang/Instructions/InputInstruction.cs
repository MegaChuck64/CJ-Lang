using CJLang.Lang;

namespace CJLang.Instructions;

internal class InputInstruction : Instruction
{
    public override string Name => "input";

    public override void Run(CJProg prog, CJFunc currentFunc, string line)
    {
        var splt = line.Split(['(']);
        var prmpt = splt[1].Split([')'])[0];
        prmpt = prmpt.Substring(1, prmpt.Length - 2);
        Console.WriteLine(prmpt);
        var input = Console.ReadLine();

        if (!line.Contains("->"))
            return;

        var destVar = line.Split(["->"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[1];

        if (!currentFunc.Locals.ContainsKey(destVar))
            throw new Exception("Variable not found");

        if (currentFunc.Locals[destVar].Type != CJVarType.str)
            throw new Exception("Invalid type");

        currentFunc.Locals[destVar].Value = input;

    }
}
