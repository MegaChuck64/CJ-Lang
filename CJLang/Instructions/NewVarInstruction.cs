using CJLang.Lang;

namespace CJLang.Instructions;

[Instruction("new", "Creates a new variable")]
internal class NewVarInstruction : Instruction
{
    public override void Run(CJProg prog, CJFunc currentFunc, string line)
    {
        //new str() -> userInput
        //new i32(5) -> userAge

        var splt = line.Split([' ']);
        var varType = splt[1].Split(new char[] { '(' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[0];


        if (!CJProg.TryGetType(varType, out var type))
            throw new Exception("Invalid type");

        var destVar = line.Split(["->", " "], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Last();

        if (currentFunc.Locals.ContainsKey(destVar))
            throw new Exception("Variable already exists");

        //check if initialization between ()
        splt = line.Split(['(']);
        splt = splt[1].Split(new char[] { ')', '"' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        object? initVal;
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

        currentFunc.Locals.Add(destVar, new CJVar
        {
            Name = destVar,
            Type = type,
            Value = initVal,
        });
    }


}