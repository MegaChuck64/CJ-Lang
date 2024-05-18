using CJLang.Execution;
using CJLang.Lang;

namespace CJLang.Instructions;

[Instruction("set", "Set a variable's value")]
internal class SetVarInstruction : Instruction
{
    internal static readonly char[] closeParenSeperator = [')', '"'];
    internal static readonly char[] openParenSeperator = ['('];

    public override void Run(CJFunc currentFunc, string line, int globalLineNum, int localLineNum)
    {
        //set i8(5) -> userAge

        var splt = line.Split([' ']);
        var varType = splt[1].Split(openParenSeperator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[0];
        if (!Helper.TryGetType(varType, out var type))
            throw new ExecutorException($"Invalid type '{varType}'", globalLineNum);

        var destVar = line.Split(["->", " "], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Last();

        if (!currentFunc.Locals.TryGetValue(destVar, out CJVar? value))
            throw new ExecutorException($"Variable '{destVar}' not found", globalLineNum);


        //check if initialization between ()
        splt = line.Split(['(']);
        splt = splt[1].Split(closeParenSeperator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);


        //set u8(age + 1) -> age
        if (splt.Length == 0)
            throw new ExecutorException($"No variable assigned to returned value of construction", globalLineNum);


        object? initVal;
        //evaluating expression
        //check if contains operator
        if (splt[0].Contains('+') || splt[0].Contains('-') || splt[0].Contains('*') || splt[0].Contains('/'))
        {
            var val = Helper.EvaluateArithmatic(currentFunc, splt[0], globalLineNum) ?? 
                throw new ExecutorException($"Invalid arithmatic expression '{splt[0]}'", globalLineNum);
            initVal = val;
        }
        else
        {
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
                    if (!currentFunc.Locals.TryGetValue(val, out CJVar? varVal))
                        throw new ExecutorException($"Variable '{val}' not found", globalLineNum);

                    if (varVal.Type != type && varVal.Type != CJVarType.str)
                        throw new ExecutorException($"Invalid type '{varVal.Type}' passed to set(). Expecting '{type}'. ", globalLineNum);

                    initVal =
                        varVal.Type == CJVarType.str ?
                        Helper.GetValFromStr(type, varVal.Value as string ?? Helper.DefaultVal(type)?.ToString() ?? "0") : varVal.Value;
                }
            }
        }

        value.Value = initVal;

    }
}