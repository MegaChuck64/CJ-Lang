﻿using CJLang.Lang;

namespace CJLang.Instructions;

internal class PrintInstruction : Instruction
{
    public override string Name => "print";

    public override void Run(CJProg prog, CJFunc currentFunc, string line)
    {
        //print string literals and variables

        var splt = line.Split(['(']);
        var prmpt = splt[1].Split([')'])[0];
        if (prmpt.StartsWith('"') && prmpt.EndsWith('"'))
            Console.WriteLine(prmpt.Substring(1, prmpt.Length - 2));
        else
        {
            if (!currentFunc.Locals.ContainsKey(prmpt))
                throw new Exception("Variable not found");

            //split newlines by \n
            var str = currentFunc.Locals[prmpt].Value.ToString();

            var lines = str.Split("\\n");
            foreach (var l in lines)
            {
                Console.WriteLine(l);
            }

        }
    }
}