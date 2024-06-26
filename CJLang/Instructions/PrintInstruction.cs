﻿using CJLang.Execution;
using CJLang.Lang;
using System.Linq;

namespace CJLang.Instructions;

[Instruction("print", "Prints a variable or literal")]
internal class PrintInstruction : Instruction
{
    internal static readonly string[] separator = ["\\n"];

    public override void Run(CJFunc currentFunc, string line, int globalLineNum, int localLineNum)
    {
        //print("Hello, ", userName, ". You are ", userAgeStr, " years old.\n")
        var start = line.IndexOf('(');
        var prmpt = line.Substring(start + 1, line.Length - start - 2);

        var str = Helper.GetStrFromConcat(currentFunc, prmpt, globalLineNum + 1);

        var lines = str.Split(separator, StringSplitOptions.None);
        foreach (var l in lines)
            Console.WriteLine(l);

    }
}