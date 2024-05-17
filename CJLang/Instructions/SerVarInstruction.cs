﻿using CJLang.Execution;
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
            throw new Exception("Invalid type");

        var destVar = line.Split(["->", " "], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Last();

        if (!currentFunc.Locals.TryGetValue(destVar, out CJVar? value))
            throw new Exception("Variable not found");

        //check if initialization between ()
        splt = line.Split(['(']);
        splt = splt[1].Split(closeParenSeperator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);


        //set u8(age + 1) -> age
        if (splt.Length == 0)
            throw new Exception("Invalid initialization");


        object? initVal;
        //evaluating expression
        //check if contains operator
        if (splt[0].Contains('+') || splt[0].Contains('-') || splt[0].Contains('*') || splt[0].Contains('/'))
        {
            var val = Helper.EvaluateArithmatic(currentFunc, splt[0]) ?? 
                throw new Exception("Invalid expression");
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
                        throw new Exception("Variable not found");

                    if (varVal.Type != type && varVal.Type != CJVarType.str)
                        throw new Exception("Invalid type");

                    initVal =
                        varVal.Type == CJVarType.str ?
                        Helper.GetValFromStr(type, varVal.Value as string ?? Helper.DefaultVal(type)?.ToString() ?? "0") : varVal.Value;
                }
            }
        }

        value.Value = initVal;

    }
}