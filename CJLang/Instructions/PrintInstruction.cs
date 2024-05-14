using CJLang.Lang;

namespace CJLang.Instructions;

[Instruction("print", "Prints a variable or literal")]
internal class PrintInstruction : Instruction
{
    public override void Run(CJFunc currentFunc, string line, int lineNum)
    {
        //print string literals and variables

        var splt = line.Split(['(']);
        var prmpt = splt[1].Split([')'])[0];
        if (prmpt.StartsWith('"') && prmpt.EndsWith('"'))
            Console.WriteLine(prmpt.Substring(1, prmpt.Length - 2));
        else if (prmpt != null && prmpt.Length > 0)
        {
            if (!currentFunc.Locals.TryGetValue(prmpt, out CJVar? value))
                throw new Exception("Variable not found");

            //split newlines by \n
            var str = value.Value?.ToString();

            var lines = str?.Split("\\n") ?? [];
            foreach (var l in lines)
            {
                Console.WriteLine(l);
            }

        }
        else
        {
            throw new Exception("Invalid print statement");
        }
    }
}