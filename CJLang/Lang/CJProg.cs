using CJLang.Instructions;
using System.Reflection;
namespace CJLang.Lang;

internal class CJProg
{
    public Dictionary<string, CJFunc> Funcs { get; set; }
    public (object val, int line, string func) RetVal { get; set; }

    public static List<(string name, Instruction instr)> InstructionRunners = [];

    public static int? NextLine { get; set; } = null;
    public CJProg(List<string> lines)
    {
        Funcs = [];

        //populate instruction runners based on attributes
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            if (type.IsSubclassOf(typeof(Instruction)))
            {
                var attr = type.GetCustomAttribute<InstructionAttribute>();
                if (attr != null)
                {
                    var inst = (Instruction)Activator.CreateInstance(type);
                    InstructionRunners.Add((attr.Name, inst));
                }
            }
        }

        var currentFunc = string.Empty;
        bool inException = false;
        var inBlock = false;
        var blockNum = 0;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].StartsWith("//") || string.IsNullOrWhiteSpace(lines[i]))
                continue;

            //only function declarations start with no tabs
            if (!lines[i].StartsWith('\t'))
            {
                var splt = lines[i].Split([' ']);
                var funcName = splt[0].Split([':'])[0];
                currentFunc = funcName;
                var retType = lines[i].Split("->")[1].Trim();
                if (retType == "exception")
                {
                    //check that the function already exists
                    if (!Funcs.ContainsKey(funcName))
                        throw new Exception("Function not found");

                    inException = true;
                    Funcs[funcName].ExceptionInstrs = new List<(string line, int globalLineNum)>();
                }

                if (TryGetType(retType, out var ret))
                    throw new Exception("Invalid return type");

                var args = new Dictionary<string, CJVar>();
                //parse args
                //splt on space, :, ->, and ,
                var argStrs = lines[i].Split(new string[] { " ", ":", "->", "," }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                argStrs = argStrs.Skip(1).Take(argStrs.Length - 2).ToArray();
                var exceptionVarName = string.Empty;
                var argCount = 0;
                for (int j = 0; j < argStrs.Length; j += 2)
                {
                    //testFunc: str name, i8 age -> i32
                    var argName = argStrs[j + 1];
                    var typeStr = argStrs[j];
                    var isArray = typeStr.EndsWith("[]");
                    if (isArray)
                        typeStr = typeStr.Substring(0, typeStr.Length - 2);

                    if (!TryGetType(typeStr, out var argType))
                        throw new Exception("Invalid type");

                    argCount++;

                    if (inException)
                    {
                        if (argCount > 1)
                            throw new Exception("Exception functions can only have one argument");
                        if (argType != CJVarType.str)
                            throw new Exception("Exception functions can only have a string argument");
                        if (isArray)
                            throw new Exception("Exception functions can only have a single string argument");

                        exceptionVarName = argName;
                        //args.Add(argName, new CJVar
                        //{
                        //    Name = argName,
                        //    Type = CJVarType.str,
                        //    IsArray = false,
                        //});
                    }
                    else
                    {
                        args.Add(argName, new CJVar
                        {
                            Name = argName,
                            Type = argType,
                            IsArray = isArray,
                        });
                    }
                }
                if (!inException)
                {
                    Funcs.Add(funcName, new CJFunc
                    {
                        Name = funcName,
                        Args = args,
                        Ret = new CJVar
                        {
                            Name = "ret",
                            Type = ret,
                        },
                        Instrs = [],
                        ExceptionInstrs = [],
                        Locals = [],
                    });
                }
                else
                {
                    Funcs[funcName].ErrorVarName = exceptionVarName;
                }

            }
            else if (lines[i].StartsWith('\t'))
            {
                
                if (lines[i].Trim().StartsWith("if") || lines[i].Trim().StartsWith("elif"))
                {
                    inBlock = true;
                    blockNum = i;
                }
                else if (lines[i].Trim().StartsWith("else"))
                {
                    if (!inBlock)
                        throw new Exception("Else without if");
                    
                    inBlock = true;
                    blockNum = i;
                }
                else if (lines[i].StartsWith("\t\t") && inBlock)
                {
                    if (Funcs[currentFunc].Blocks == null)
                        Funcs[currentFunc].Blocks = new Dictionary<int, List<(string line, int globalLineNum)>>();

                    if (!Funcs[currentFunc].Blocks.ContainsKey(blockNum))
                        Funcs[currentFunc].Blocks.Add(blockNum, new List<(string line, int globalLineNum)>());

                    Funcs[currentFunc].Blocks[blockNum].Add((lines[i].Trim(), i));
                    
                    continue;
                }
                else
                {
                    inBlock = false;
                }

                if (inException)
                {
                    Funcs[currentFunc].ExceptionInstrs.Add((lines[i].Trim(), i));
                }
                else
                    Funcs[currentFunc].Instrs.Add((lines[i].Trim(), i));
            }
        }
    }

    public void Execute()
    {
        //find main
        if (!Funcs.ContainsKey("main"))
            throw new Exception("No main function found");

        var currentFunc = Funcs["main"];

        try
        {
            ProcessLines(currentFunc.Instrs, currentFunc, InstructionRunners);
        }
        catch (Exception e)
        {
            currentFunc.ErrorMessage = e.Message;
            if (currentFunc.ExceptionInstrs.Any())
            {
                currentFunc.Args.Add(currentFunc.ErrorVarName, new CJVar
                {
                    Name = currentFunc.ErrorVarName,
                    Type = CJVarType.str,
                    Value = e.Message,
                });
                ProcessLines(currentFunc.ExceptionInstrs, currentFunc, InstructionRunners);
            }
        }
    }

    public static void ProcessLines(List<(string line, int globalLineNum)> instrs, CJFunc func, List<(string name, Instruction instr)> instructionRunners)
    {
        //add args to locals
        foreach (var arg in func.Args)
        {
            func.Locals.Add(arg.Key, arg.Value);
        }

        for (int localLineNum = 0; localLineNum < instrs.Count; localLineNum++)
        {
            if (NextLine != null && NextLine != localLineNum)
            {
                localLineNum = NextLine.Value;
                NextLine = null;                
            }

            (string line, int globalLineNum) = instrs[localLineNum];
            foreach (var (name, instr) in instructionRunners)
            {
                if (line.StartsWith(name))
                {
                    try
                    {
                        instr.Run(func, line, globalLineNum, localLineNum);
                    }
                    catch (Exception e)
                    {
                        func.ErrorMessage = e.Message;
                        
                        throw new Exception($"Error on line {globalLineNum + 1}: {e.Message}");
                    }
                }
            }
        }
    }

    public static bool EvaluateCondition(CJVarType type, string line)
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
                throw new Exception("Invalid operator");

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
                throw new Exception("Invalid operator");

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
                throw new Exception("Invalid operands");

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
                throw new Exception("Invalid operands");

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
            throw new Exception("Invalid type");

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
            CJVarType._null => null,
            _ => null,
        };
    }
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
            case "null":
                type = CJVarType._null;
                return true;
            default:
                type = CJVarType._null;
                return false;
        }
    }

    public static object? DefaultVal(CJVarType type)
    {
        return type switch
        {
            CJVarType.i8 => (sbyte)0,
            CJVarType.i16 => (short)0,
            CJVarType.i32 => (int)0,
            CJVarType.i64 => (long)0,
            CJVarType.u8 => (byte)0,
            CJVarType.u16 => (ushort)0,
            CJVarType.u32 => (uint)0,
            CJVarType.u64 => (ulong)0,
            CJVarType.f32 => (float)0,
            CJVarType.f64 => (double)0,
            CJVarType.str => string.Empty,
            CJVarType._bool => false,
            CJVarType._null => null,
            _ => null,
        };
    }

    public static string GetStrFromConcat(CJFunc currentFunc, string line)
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
                    if (!currentFunc.Locals.ContainsKey(varName))
                        throw new Exception("Variable not found");

                    str += currentFunc.Locals[varName].Value;
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
                if (!currentFunc.Locals.ContainsKey(varName))
                    throw new Exception("Variable not found");

                str += currentFunc.Locals[varName].Value;
            }
        }

        return str;
    }

}