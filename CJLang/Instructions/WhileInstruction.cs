﻿using CJLang.Execution;
using CJLang.Lang;

namespace CJLang.Instructions;

[Instruction("while", "While loop")]
internal class WhileInstruction : Instruction
{
    public override void Run(CJFunc currentFunc, string line, int globalLinNum, int localLineNum)
    {
        var splt = line.Split(['(']);
        var condition = splt[1].Split([')'])[0];
        currentFunc.LastBlockConditionResult = null;
        //get type and convert to literals 
        var splt2 = condition.Split([' ']);
        if (splt2.Length == 1)
        {
            bool cond;
            if (splt2[0] == "true")
            {
                cond = true;
            }
            else if (splt2[0] == "false")
            {
                cond = false;
            }
            else if (currentFunc.Locals.TryGetValue(splt2[0], out CJVar? value))
            {
                cond = (bool)(value.Value ?? false);
            }
            else
            {
                throw new ExecutorException($"Invalid format for conditional", globalLinNum);
            }
            currentFunc.LastBlockConditionResult = cond;
            if (cond)
            {
                var lines = currentFunc.Blocks[globalLinNum];
                Executor.ProcessLines(lines, currentFunc);
            }
        }
        else if (splt2.Length == 3)
        {
            var left = splt2[0];
            var op = splt2[1];
            var right = splt2[2];

            var leftType = CJVarType._void;
            var rightType = CJVarType._void;
            if (currentFunc.Locals.TryGetValue(left, out var varLeft))
            {
                left = varLeft?.Value?.ToString() ?? "0";
                leftType = varLeft?.Type ?? CJVarType.u8;
            }
            else
            {
                if (left == "true")
                {
                    left = "true";
                    leftType = CJVarType._bool;
                }
                else if (left == "false")
                {
                    left = "false";
                    leftType = CJVarType._bool;
                }
                else if (int.TryParse(left, out _))
                {
                    leftType = CJVarType.i32;
                }
                else if (double.TryParse(left, out _))
                {
                    leftType = CJVarType.f64;
                }
                else if (left.StartsWith('"') && left.EndsWith('"'))
                {
                    leftType = CJVarType.str;
                    left = left[1..^1];
                }
            }

            if (currentFunc.Locals.TryGetValue(right, out var varRight))
            {
                right = varRight?.Value?.ToString() ?? "0";
                rightType = varRight?.Type ?? CJVarType.u8;
            }
            else
            {
                if (right == "true")
                {
                    right = "true";
                    rightType = CJVarType._bool;
                }
                else if (right == "false")
                {
                    right = "false";
                    rightType = CJVarType._bool;
                }
                else if (int.TryParse(right, out _))
                {
                    rightType = CJVarType.i32;
                }
                else if (double.TryParse(right, out _))
                {
                    rightType = CJVarType.f64;
                }
                else if (right.StartsWith('"') && right.EndsWith('"'))
                {
                    rightType = CJVarType.str;
                    right = right[1..^1];
                }
            }

            if (leftType != rightType &&
                !(leftType <= CJVarType.f64 &&
                leftType >= CJVarType.i8 &&
                rightType <= CJVarType.f64 &&
                rightType >= CJVarType.i8))
                throw new ExecutorException($"Invalid type left: '{leftType}' or right: {rightType}", globalLinNum);

            var str = $"{left} {op} {right}";
            var type = leftType == CJVarType.f32 || leftType == CJVarType.f64 || rightType == CJVarType.f32 || rightType == CJVarType.f64 ? CJVarType.f64 : leftType;


            bool cond = Helper.EvaluateCondition(type, str, globalLinNum + 1);
            currentFunc.LastBlockConditionResult = cond;

            if (cond)
            {
                var lines = currentFunc.Blocks[globalLinNum];

                var localsCopy = new Dictionary<string, CJVar>(currentFunc.Locals);

                Executor.ProcessLines(lines, currentFunc);

                //restore locals
                currentFunc.Locals = localsCopy;

                Executor.NextLine = localLineNum;

            }
        }
        else
        {
            throw new ExecutorException($"Invalid format for conditional", globalLinNum);
        }
    }
}