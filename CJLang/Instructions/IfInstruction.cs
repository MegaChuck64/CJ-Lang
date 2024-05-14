using CJLang.Lang;

namespace CJLang.Instructions;

[Instruction("if", "if condition evaluates to true, execute body")]
internal class IfInstruction : Instruction
{
    public override void Run(CJFunc currentFunc, string line, int lineNum)
    {
        var splt = line.Split(['(']);
        var condition = splt[1].Split([')'])[0];
        
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
            else if (currentFunc.Locals.ContainsKey(splt2[0]))
            {
                cond = (bool)currentFunc.Locals[splt2[0]].Value;
            }
            else
            {
                throw new Exception("Invalid condition");
            }

            if (cond)
            {
                var lines = currentFunc.IfBlocks[lineNum];
                CJProg.ProcessLines(lines, currentFunc, CJProg.InstructionRunners);
            }
        }
        else if (splt2.Length == 3)
        {
            var left = splt2[0];
            var op = splt2[1];
            var right = splt2[2];

            var leftType = CJVarType._null;
            var rightType = CJVarType._null;
            if (currentFunc.Locals.TryGetValue(left, out var varLeft))
            {
                left = varLeft.Value.ToString();
                leftType = varLeft.Type;
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
                    left = left.Substring(1, left.Length - 2);
                }
            }

            if (currentFunc.Locals.TryGetValue(right, out var varRight))
            {
                right = varRight.Value.ToString();
                rightType = varRight.Type;
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
                    right = right.Substring(1, right.Length - 2);
                }
            }

            if (leftType != rightType && 
                !(leftType <= CJVarType.f64 && 
                leftType >= CJVarType.i8 && 
                rightType <= CJVarType.f64 && 
                rightType >= CJVarType.i8))
                throw new Exception("Invalid type");

            var str = $"{left} {op} {right}";
            var type = leftType == CJVarType.f32 || leftType == CJVarType.f64 || rightType == CJVarType.f32 || rightType == CJVarType.f64 ? CJVarType.f64 : leftType;

            bool cond = CJProg.EvaluateCondition(type, str);
            if (cond)
            {
                var lines = currentFunc.IfBlocks[lineNum];
                CJProg.ProcessLines(lines, currentFunc, CJProg.InstructionRunners);
            }
        }
        else
        {
            throw new Exception("Invalid condition");
        }
    }
}