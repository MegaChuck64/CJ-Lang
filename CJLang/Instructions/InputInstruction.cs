﻿using CJLang.Lang;

namespace CJLang.Instructions;

[Instruction("input", "Reads input from the user")]
internal class InputInstruction : Instruction
{
    public override void Run(CJFunc currentFunc, string line, int globalLineNum, int localLineNum)
    {
        var splt = line.Split(['(']);
        var prmpt = splt[1].Split([')'])[0];
        prmpt = prmpt[1..^1];
        Console.WriteLine(prmpt);
        var input = Console.ReadLine();

        if (!line.Contains("->"))
            return;

        var destVar = line.Split(["->"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[1];

        if (!currentFunc.Locals.TryGetValue(destVar, out CJVar? value))
            throw new Exception("Variable not found");

        if (value.Type != CJVarType.str)
            throw new Exception("Invalid type");

        value.Value = input;

    }
}
