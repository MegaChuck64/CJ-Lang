using CJLang.Execution;
using CJLang.Lang;

namespace CJLang.Instructions;

[Instruction("new", "Creates a new variable")]
internal class NewVarInstruction : Instruction
{
    internal static readonly char[] openParenSeperator = ['('];
    internal static readonly char[] closeParenSeperator = [')', '"'];

    public override void Run(CJFunc currentFunc, string line, int globalLineNum, int localLineNum)
    {
        //new str() -> userInput
        //new i32(5) -> userAge

        var splt = line.Split([' ']);
        var varType = splt[1].Split(openParenSeperator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[0];


        if (!Helper.TryGetType(varType, out var type))
            throw new Exception("Invalid type");

        var destVar = line.Split(["->", " "], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Last();

        if (currentFunc.Locals.ContainsKey(destVar))
            throw new Exception("Variable already exists");

        //check if initialization between ()
        splt = line.Split(['(']);
        splt = splt[1].Split(closeParenSeperator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        object? initVal;
        //no initialization
        if (splt[0].StartsWith("->"))
        {
            initVal = Helper.DefaultVal(type);
        }
        else
        {
            //var or literal
            var val = splt[0];


            if (val.StartsWith('"') && val.EndsWith('"'))
                initVal = val[1..^1];
            else
            {
                var obj = Helper.GetObjectFromVarOrLiteral(type, val, currentFunc) ?? 
                    throw new Exception("Invalid initialization");

                initVal = obj;
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