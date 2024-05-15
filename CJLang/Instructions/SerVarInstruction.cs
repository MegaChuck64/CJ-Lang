using CJLang.Lang;

namespace CJLang.Instructions;

[Instruction("set", "Set a variable's value")]
internal class SetVarInstruction : Instruction
{
    public override void Run(CJFunc currentFunc, string line, int globalLineNum, int localLineNum)
    {
        //set i8(5) -> userAge

        var splt = line.Split([' ']);
        var varType = splt[1].Split(new char[] { '(' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[0];
        if (!CJProg.TryGetType(varType, out var type))
            throw new Exception("Invalid type");

        var destVar = line.Split(["->", " "], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Last();

        if (!currentFunc.Locals.ContainsKey(destVar))
            throw new Exception("Variable not found");

        //check if initialization between ()
        splt = line.Split(['(']);
        splt = splt[1].Split(new char[] { ')', '"' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        object? initVal = null; 

        //set u8(age + 1) -> age
        if (splt.Length == 0)
            throw new Exception("Invalid initialization");

        //evaluating expression
        //check if contains operator
        if (splt[0].Contains('+') || splt[0].Contains('-') || splt[0].Contains('*') || splt[0].Contains('/'))
        {
            var val = CJProg.EvaluateArithmatic(currentFunc, splt[0]);
            if (val == null)
                throw new Exception("Invalid expression");

            initVal = val;
        }
        else
        {
            //no initialization
            if (splt[0].StartsWith("->"))
            {
                initVal = CJProg.DefaultVal(type);
            }
            else
            {
                //var or literal
                var val = splt[0];

                if (val.StartsWith('"') && val.EndsWith('"'))
                    initVal = val.Substring(1, val.Length - 2);
                else
                {
                    if (!currentFunc.Locals.ContainsKey(val))
                        throw new Exception("Variable not found");

                    if (currentFunc.Locals[val].Type != type && currentFunc.Locals[val].Type != CJVarType.str)
                        throw new Exception("Invalid type");

                    initVal =
                        currentFunc.Locals[val].Type == CJVarType.str ?
                        CJProg.GetValFromStr(type, currentFunc.Locals[val].Value as string ?? CJProg.DefaultVal(type)?.ToString() ?? "0") :
                        currentFunc.Locals[val].Value;
                }
            }
        }


        currentFunc.Locals[destVar].Value = initVal;

    }
}