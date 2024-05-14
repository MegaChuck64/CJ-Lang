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