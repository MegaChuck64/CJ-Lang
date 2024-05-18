using CJLang.Execution;
using CJLang.Lang;

namespace CJLang.Instructions;

internal class CallFunctionInstruction : Instruction
{
    public override void Run(CJFunc currentFunc, string line, int globalLineNum, int localLineNum)
    {
        //test_func(a, b, 22) -> c
        var splt = line.Split(['(']);
        var funcName = splt[0];
        var prmpt = splt[1].Split([')'])[0];

        //get return variable
        var destVar = line.Contains("->") ? 
            line.Split(["->"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[1] : 
            null;

        //make arg parsing function in Helper that provides a list of values
        //figure out how to store return line, which variable to set value to, and the value itself
        //might need to look into how the arg handling works when calling ProcessLines

        var args = Helper.ParseArgs(prmpt);

        if (!Executor.Prog.Funcs.TryGetValue(funcName, out CJFunc? func))
            throw new ExecutorException($"Function not found '{funcName}'", globalLineNum);

        if (func.Args.Count != args.Count)
            throw new ExecutorException($"Invalid number of arguments for {funcName}", globalLineNum);


        for (int i = 0; i < func.Args.Count; i++)
        {
            var arg = func.Args.Values.ElementAt(i);
            var (type, value) = args[i];
            if (type == CJVarType._void)
            {
                type = currentFunc.Locals[value?.ToString() ?? string.Empty]?.Type ?? type;
                value = currentFunc.Locals[value?.ToString() ?? string.Empty]?.Value;
            }

            //check if they are compatible types
            //ParseArgs just returns i32, f32, bool, str
            //but i32 should be compatible with i16, u32, etc
            if (arg.Type >= CJVarType.u8 && arg.Type <= CJVarType.i64 && type >= CJVarType.u8 && type <= CJVarType.i64)
            {
                //do nothing
            }
            else if (arg.Type >= CJVarType.f32 && arg.Type <= CJVarType.f64 && type >= CJVarType.f32 && type <= CJVarType.f64)
            {

            }
            else if (arg.Type != type)
            {
                throw new ExecutorException($"Invalid type for argument #{i + 1} for {funcName}. Expecting {arg.Type}", globalLineNum);
            }

            //set the value
            func.Args[arg.Name].Value = value;

        }

        //run the function

        Executor.ProcessLines(func.Instrs, func);

        //set the return value

        if (destVar == null)
            Executor.RetVal = null;
        else
        {
            if (currentFunc.Locals.TryGetValue(destVar, out CJVar? varV))
            {
                varV.Value = Executor.RetVal?.Value;
            }
            else
            {
                throw new ExecutorException($"Varaible not found '{destVar}'", globalLineNum);
            }
        }



    }
}