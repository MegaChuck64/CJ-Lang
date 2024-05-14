using CJLang.Instructions;
namespace CJLang.Lang;

internal class CJProg
{
    public Dictionary<string, CJFunc> Funcs { get; set; }
    public (object val, int line, string func) RetVal { get; set; }

    private List<Instruction> _instructionRunners = [];
    public CJProg(List<string> lines)
    {
        Funcs = [];
        _instructionRunners.Add(new SetVarInstruction());
        _instructionRunners.Add(new NewVarInstruction());
        _instructionRunners.Add(new PrintInstruction());
        _instructionRunners.Add(new InputInstruction());
        _instructionRunners.Add(new StrConcatInstruction());
        _instructionRunners.Add(new ClearInstruction());



        var currentFunc = string.Empty;
        bool inException = false;
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
            ProcessLines(currentFunc.Instrs, currentFunc);
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
                ProcessLines(currentFunc.ExceptionInstrs, currentFunc);
            }
        }
    }

    private void ProcessLines(List<(string line, int globalLineNum)> instrs, CJFunc func)
    {
        //add args to locals
        foreach (var arg in func.Args)
        {
            func.Locals.Add(arg.Key, arg.Value);
        }

        for (int localLinNum = 0; localLinNum < instrs.Count; localLinNum++)
        {
            (string line, int globalLineNum) = instrs[localLinNum];
            foreach (var runner in _instructionRunners)
            {
                if (line.StartsWith(runner.Name))
                {
                    runner.Run(this, func, line);
                    break;
                }
            }
        }
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

}