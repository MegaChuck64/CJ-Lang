using System.Security.Cryptography.X509Certificates;

namespace CJLang
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            args = ["Test.cj"];
#endif

            if (args.Length == 0)
            {
                Console.WriteLine("No file specified");
                return;
            }
            try
            {
                var lines = File.ReadAllLines(args[0]).ToList();
                var prog = new CJProg(lines);
                prog.Execute();
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
    }

    internal class CJProg
    {
        public Dictionary<string, CJFunc> Funcs { get; set; }
        public (object val, int line, string func) RetVal { get; set; }

        private List<InstructionRunner> _instructionRunners = [];
        public CJProg(List<string> lines)
        {
            Funcs = [];
            _instructionRunners.Add(new SetVarRunner());
            _instructionRunners.Add(new NewVarRunner());
            _instructionRunners.Add(new PrintRunner());
            _instructionRunners.Add(new InputRunner());
            _instructionRunners.Add(new StrConcatRunner());



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

    internal class StrConcatRunner : InstructionRunner
    {
        public override string Name => "str_concat";

        public override void Run(CJProg prog, CJFunc currentFunc, string line)
        {
            //str_concat("Hello, ", userName, ". You are ", userAgeStr, " years old.\n") -> dest
            var splt = line.Split(['(']);
            var prmpt = splt[1].Split([')'])[0];
            var destVar = line.Split(["->"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[1];
            if (!currentFunc.Locals.ContainsKey(destVar))
                throw new Exception("Variable not found");

            //split by commas if not surrounded by quotes
            var str = string.Empty;
            var inQuote = false;
            var strStart = 0;

            for (int i = 0; i < prmpt.Length; i++)
            {
                //vars divided by commas, except for commas in quotes
                //in quotes are literals 

                if (prmpt[i] == '"')
                {
                    inQuote = !inQuote;
                    continue;
                }

                if (prmpt[i] == ',' && !inQuote)
                {
                    var subStr = prmpt.Substring(strStart, i - strStart).Trim();
                    if (subStr.StartsWith('"') && subStr.EndsWith('"'))
                        str += subStr.Substring(1, subStr.Length - 2);
                    else
                    {
                        if (!currentFunc.Locals.ContainsKey(subStr))
                            throw new Exception("Variable not found");

                        str += currentFunc.Locals[subStr].Value.ToString();
                    }

                    strStart = i + 1;
                }

            }

            if (strStart < prmpt.Length)
            {
                var end = prmpt.Substring(strStart).Trim();
                if (end.StartsWith('"') && end.EndsWith('"'))
                    str += end.Substring(1, end.Length - 2);
                else
                {
                    if (!currentFunc.Locals.ContainsKey(end))
                        throw new Exception("Variable not found");

                    str += currentFunc.Locals[end].Value.ToString();
                }
            }
            currentFunc.Locals[destVar].Value = str;

        }
    }
    internal class PrintRunner : InstructionRunner
    {
        public override string Name => "print";

        public override void Run(CJProg prog, CJFunc currentFunc, string line)
        {
            //print string literals and variables

            var splt = line.Split(['(']);
            var prmpt = splt[1].Split([')'])[0];
            if (prmpt.StartsWith('"') && prmpt.EndsWith('"'))
                Console.WriteLine(prmpt.Substring(1, prmpt.Length - 2));
            else
            {
                if (!currentFunc.Locals.ContainsKey(prmpt))
                    throw new Exception("Variable not found");

                //split newlines by \n
                var str = currentFunc.Locals[prmpt].Value.ToString();

                var lines = str.Split("\\n");
                foreach (var l in lines)
                {
                    Console.WriteLine(l);
                }

            }
        }
    }

    internal class InputRunner : InstructionRunner
    {
        public override string Name => "input";

        public override void Run(CJProg prog, CJFunc currentFunc, string line)
        {
            var splt = line.Split(['(']);
            var prmpt = splt[1].Split([')'])[0];
            prmpt = prmpt.Substring(1, prmpt.Length - 2);
            Console.WriteLine(prmpt);
            var input = Console.ReadLine();
            var destVar = line.Split(["->"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[1];
            
            if (!currentFunc.Locals.ContainsKey(destVar))
                throw new Exception("Variable not found");

            if (currentFunc.Locals[destVar].Type != CJVarType.str)
                throw new Exception("Invalid type");

            currentFunc.Locals[destVar].Value = input;

        }
    }

    internal class SetVarRunner : InstructionRunner
    {
        public override string Name => "set";
        public override void Run(CJProg prog, CJFunc currentFunc, string line)
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
                else if (val == "null" || val == "true" || val == "false" || int.TryParse(val, out _) || double.TryParse(val, out _))
                {
                    initVal = CJProg.GetValFromStr(type, val);
                }
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

            currentFunc.Locals[destVar].Value = initVal;

        }
    }

    internal class NewVarRunner : InstructionRunner
    {
        public override string Name => "new";

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
                var val= splt[0];

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

    internal abstract class InstructionRunner
    {
        public abstract string Name { get; }
        public abstract void Run(CJProg prog, CJFunc currentFunc, string line);
    }

    internal class  CJFunc
    {
        public string Name { get; set; }
        public Dictionary<string, CJVar> Args { get; set; } = [];
        public Dictionary<string, CJVar> Locals { get; set; } = [];
        public CJVar Ret { get; set; }
        public List<(string line, int globalLineNum)> Instrs { get; set; }

        public string ErrorMessage { get; set; }
        public string ErrorVarName { get; set; }
        public List<(string line, int globalLineNum)> ExceptionInstrs { get; set; }
    }


    public class CJVar
    {
        public string Name { get; set; }
        public CJVarType Type { get; set; }
        public bool IsArray { get; set; }
        public int ArraySize { get; set; }
        public object? Value { get; set; }
    }



    public enum CJVarType
    {
        i8,
        i16,
        i32,
        i64,
        u8,
        u16,
        u32,
        u64,
        f32,
        f64,
        str,
        _bool,
        _null,        
    }
}
