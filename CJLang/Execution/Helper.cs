using CJLang.Lang;

namespace CJLang.Execution;

public static class Helper
{
    internal static readonly char[] operationSeperator = ['+', '-', '*', '/', '%'];

    public static bool TryGetType(string s, out CJVarType type)
    {
        switch (s)
        {
            case "i8":
                type = CJVarType.i8;
                return true;
            case "i16":
                type = CJVarType.i16;
                return true;
            case "i32":
                type = CJVarType.i32;
                return true;
            case "i64":
                type = CJVarType.i64;
                return true;
            case "u8":
                type = CJVarType.u8;
                return true;
            case "u16":
                type = CJVarType.u16;
                return true;
            case "u32":
                type = CJVarType.u32;
                return true;
            case "u64":
                type = CJVarType.u64;
                return true;
            case "f32":
                type = CJVarType.f32;
                return true;
            case "f64":
                type = CJVarType.f64;
                return true;
            case "str":
                type = CJVarType.str;
                return true;
            case "bool":
                type = CJVarType._bool;
                return true;
            case "void":
                type = CJVarType._void;
                return true;
            default:
                type = CJVarType._void;
                return false;
        }
    }


    internal static object? EvaluateArithmatic(CJFunc func, string line, int lineNum)
    {
        //age + 1 - 4 + testInt
        object? val = null;
        //split on operators
        var splt = line.Split(operationSeperator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var operators = line.Where(c => c == '+' || c == '-' || c == '*' || c == '/' || c == '%').ToArray();

        if (splt.Length == 1)
        {
            if (func.Locals.TryGetValue(splt[0], out CJVar? value))
            {
                val = value.Value;
                return true;
            }
            else
            {
                if (int.TryParse(splt[0], out var i))
                {
                    val = i;
                    return true;
                }
                else if (double.TryParse(splt[0], out var d))
                {
                    val = d;
                    return true;
                }
                else
                {
                    val = null;
                    return false;
                }
            }
        }
        else
        {
            var total = 0;
            var totalD = 0.0;
            var isInt = true;
            for (int i = 0; i < splt.Length; i++)
            {
                if (func.Locals.TryGetValue(splt[i], out CJVar? value))
                {
                    if (value.Type == CJVarType.f32 || value.Type == CJVarType.f64)
                        isInt = false;

                    if (isInt)
                        total += Convert.ToInt32(value.Value);
                    else
                        totalD += Convert.ToDouble(value.Value);
                }
                else
                {
                    if (int.TryParse(splt[i], out var v))
                    {
                        total += v;
                    }
                    else if (double.TryParse(splt[i], out var d))
                    {
                        isInt = false;
                        totalD += d;
                    }
                    else
                    {
                        throw new ExecutorException($"Invalid value or variable name '{splt[i]}'", lineNum);
                    }
                }
            }

            if (isInt)
            {
                val = total;
            }
            else
            {
                val = totalD;
            }
        }

        return val;

    }

    public static bool EvaluateCondition(CJVarType type, string line, int lineNum)
    {
        if (type == CJVarType._bool)
        {
            //true == false
            //true

            if (line == "true")
                return true;
            if (line == "false")
                return false;

            var splt = line.Split([' ']);
            var op = splt[1];
            var left = splt[0];
            var right = splt[2];

            if (op != "==" && op != "!=")
                throw new ExecutorException($"Invalid operator '{op}'", lineNum);

            if (op == "==")
                return left == right;
            else if (op == "!=")
                return left != right;

            return false;
        }
        else if (type == CJVarType.str)
        {
            var splt = line.Split([' ']);
            var op = splt[1];
            var left = splt[0];
            var right = splt[2];

            if (op != "==" && op != "!=")
                throw new ExecutorException($"Invalid operator '{op}'", lineNum);

            if (op == "==")
                return left == right;
            else if (op == "!=")
                return left != right;

            return false;
        }
        else if (type >= CJVarType.i8 && type < CJVarType.u64)
        {
            var splt = line.Split([' ']);
            var op = splt[1];
            var left = splt[0];
            var right = splt[2];

            if (!int.TryParse(left, out var l) || !int.TryParse(right, out var r))
                throw new ExecutorException($"Invalid operands left: '{left}' or right: '{right}'", lineNum);

            return op switch
            {
                "==" => l == r,
                "!=" => l != r,
                "<" => l < r,
                ">" => l > r,
                "<=" => l <= r,
                ">=" => l >= r,
                _ => false,
            };
        }
        else if (type == CJVarType.f32 || type == CJVarType.f64)
        {
            var splt = line.Split([' ']);
            var op = splt[1];
            var left = splt[0];
            var right = splt[2];

            if (!double.TryParse(left, out var l) || !double.TryParse(right, out var r))
                throw new ExecutorException($"Invalid operands left: '{left}' or right: '{right}'", lineNum);

            return op switch
            {
                "==" => l == r,
                "!=" => l != r,
                "<" => l < r,
                ">" => l > r,
                "<=" => l <= r,
                ">=" => l >= r,
                _ => false,
            };
        }
        else
            throw new ExecutorException($"Invalid type '{type}'", lineNum);

    }

    public static object? GetValFromStr(CJVarType type, string str)
    {
        return type switch
        {
            CJVarType.i8 => sbyte.Parse(str),
            CJVarType.i16 => short.Parse(str),
            CJVarType.i32 => int.Parse(str),
            CJVarType.i64 => long.Parse(str),
            CJVarType.u8 => byte.Parse(str),
            CJVarType.u16 => ushort.Parse(str),
            CJVarType.u32 => uint.Parse(str),
            CJVarType.u64 => ulong.Parse(str),
            CJVarType.f32 => float.Parse(str),
            CJVarType.f64 => double.Parse(str),
            CJVarType.str => str,
            CJVarType._bool => bool.Parse(str),
            CJVarType._void => null,
            _ => null,
        };
    }

    public static object? DefaultVal(CJVarType type)
    {
        return type switch
        {
            CJVarType.i8 => (sbyte)0,
            CJVarType.i16 => (short)0,
            CJVarType.i32 => 0,
            CJVarType.i64 => (long)0,
            CJVarType.u8 => (byte)0,
            CJVarType.u16 => (ushort)0,
            CJVarType.u32 => (uint)0,
            CJVarType.u64 => (ulong)0,
            CJVarType.f32 => (float)0,
            CJVarType.f64 => (double)0,
            CJVarType.str => string.Empty,
            CJVarType._bool => false,
            CJVarType._void => null,
            _ => null,
        };
    }

    internal static string GetStrFromConcat(CJFunc currentFunc, string line, int lineNum)
    {
        //"Hello, ", userName, ". You are ", userAgeStr, " years old.\n"
        var str = string.Empty;

        var inQuote = false;
        var varName = string.Empty;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                inQuote = !inQuote;
                continue;
            }

            if (inQuote)
            {
                str += line[i];
                continue;
            }

            if (line[i] == ',')
            {
                if (varName != string.Empty)
                {
                    if (!currentFunc.Locals.TryGetValue(varName, out CJVar? value))
                        throw new ExecutorException($"Variable '{varName}' not found", lineNum);

                    str += value.Value;
                    varName = string.Empty;
                }
                continue;
            }

            if (line[i] == ' ')
                continue;

            if (line[i] == '\n')
            {
                str += '\n';
                continue;
            }

            //variable
            varName += line[i];

            if (i == line.Length - 1)
            {
                if (!currentFunc.Locals.TryGetValue(varName, out CJVar? value))
                    throw new ExecutorException($"Variable '{varName}' not found", lineNum);

                str += value.Value;
            }
        }

        return str;
    }

    internal static object? GetObjectFromVarOrLiteral(CJVarType type, string txt, CJFunc func)
    {
        //check if string literal
        if (txt.StartsWith('"') && txt.EndsWith('"'))
        {
            if (type != CJVarType.str)
                return null;

            return txt[1..^1];
        }

        //check if variable
        if (func.Locals.TryGetValue(txt, out CJVar? value))
        {
            if (value.Type != type && value.Type != CJVarType.str)
                return null;

            return value.Type == CJVarType.str ?
                GetValFromStr(type, value.Value as string ?? DefaultVal(type)?.ToString() ?? "0") : value.Value;
        }

        //check if bool
        if (txt == "true")
        {
            if (type != CJVarType._bool)
                return null;

            return true;
        }
        else if (txt == "false")
        {
            if (type != CJVarType._bool)
                return null;

            return false;
        }

        switch (type)
        {
            case CJVarType.i8:
                if (!sbyte.TryParse(txt, out var i8))
                    return null;
                return i8;
            case CJVarType.i16:
                if (!short.TryParse(txt, out var i16))
                    return null;
                return i16;
            case CJVarType.i32:
                if (!int.TryParse(txt, out var i32))
                    return null;
                return i32;
            case CJVarType.i64:
                if (!long.TryParse(txt, out var i64))
                    return null;
                return i64;
            case CJVarType.u8:
                if (!byte.TryParse(txt, out var u8))
                    return null;
                return u8;
            case CJVarType.u16:
                if (!ushort.TryParse(txt, out var u16))
                    return null;
                return u16;
            case CJVarType.u32:
                if (!uint.TryParse(txt, out var u32))
                    return null;
                return u32;
            case CJVarType.u64:
                if (!ulong.TryParse(txt, out var u64))
                    return null;
                return u64;
            case CJVarType.f32:
                if (!float.TryParse(txt, out var f32))
                    return null;
                return f32;
            case CJVarType.f64:
                if (!double.TryParse(txt, out var f64))
                    return null;
                return f64;
            default:
                return null;
        }
    }

    /// <summary>
    /// Pass a raw string and receive split on commas except in quotes
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    internal static List<(CJVarType type, object? value)> ParseArgs(string line)
    {
        //split by commas except in quotes

        //10, "Hello, ", userName, 10.5
       
        var args = new List<(CJVarType type, object? value)>();

        var inQuote = false;
        var arg = string.Empty;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                inQuote = !inQuote;
                continue;
            }

            if (inQuote)
            {
                arg += line[i];
                continue;
            }

            if (line[i] == ',')
            {
                if (arg != string.Empty)
                {
                    if (arg == "true" || arg == "false")
                    {
                        args.Add((CJVarType._bool, bool.Parse(arg)));
                    }
                    else if (int.TryParse(arg, out var nt))
                    {
                        args.Add((CJVarType.i32, nt));
                    }
                    else if (double.TryParse(arg, out var d))
                    {
                        args.Add((CJVarType.f64, d));
                    }
                    else if (arg.StartsWith('"') && arg.EndsWith('"'))
                    {
                        args.Add((CJVarType.str, arg[1..^1]));
                    }
                    else
                    {
                        //variable
                        args.Add((CJVarType._void, arg));
                    }
                }
                arg = string.Empty;
                continue;
            }

            if (line[i] == ' ')
                continue;

            arg += line[i];

            if (i == line.Length - 1)
            {
                if (arg == "true" || arg == "false")
                {
                    args.Add((CJVarType._bool, bool.Parse(arg)));
                }
                else if (int.TryParse(arg, out var nt))
                {
                    args.Add((CJVarType.i32, nt));
                }
                else if (double.TryParse(arg, out var d))
                {
                    args.Add((CJVarType.f64, d));
                }
                else if (arg.StartsWith('"') && arg.EndsWith('"'))
                {
                    args.Add((CJVarType.str, arg[1..^1]));
                }
                else
                {
                    args.Add((CJVarType._void, arg));
                }
            }
        }

        return args;
    }


}