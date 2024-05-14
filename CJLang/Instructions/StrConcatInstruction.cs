using CJLang.Lang;
namespace CJLang.Instructions;


internal class StrConcatInstruction : Instruction
{
    public override string Name => "str_concat";

    public override void Run(CJProg prog, CJFunc currentFunc, string line)
    {
        //str_concat("Hello, ", userName, ". You are ", userAgeStr, " years old.\n") -> dest
        var splt = line.Split(['(']);
        var prmpt = splt[1].Split([')'])[0];
        var destVar = line.Split(["->"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[1];
        if (!currentFunc.Locals.ContainsKey(destVar))
            throw new Exception("Variable not found");

        //split by commas if not surrounded by quotes
        var str = string.Empty;
        var inQuote = false;
        var strStart = 0;

        for (int i = 0; i < prmpt.Length; i++)
        {
            //vars divided by commas, except for commas in quotes
            //in quotes are literals 

            if (prmpt[i] == '"')
            {
                inQuote = !inQuote;
                continue;
            }

            if (prmpt[i] == ',' && !inQuote)
            {
                var subStr = prmpt.Substring(strStart, i - strStart).Trim();
                if (subStr.StartsWith('"') && subStr.EndsWith('"'))
                    str += subStr.Substring(1, subStr.Length - 2);
                else
                {
                    if (!currentFunc.Locals.ContainsKey(subStr))
                        throw new Exception("Variable not found");

                    str += currentFunc.Locals[subStr].Value.ToString();
                }

                strStart = i + 1;
            }

        }

        if (strStart < prmpt.Length)
        {
            var end = prmpt.Substring(strStart).Trim();
            if (end.StartsWith('"') && end.EndsWith('"'))
                str += end.Substring(1, end.Length - 2);
            else
            {
                if (!currentFunc.Locals.ContainsKey(end))
                    throw new Exception("Variable not found");

                str += currentFunc.Locals[end].Value.ToString();
            }
        }
        currentFunc.Locals[destVar].Value = str;

    }
}