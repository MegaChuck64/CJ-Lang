﻿using CJLang.Instructions;
using CJLang.Lang;

namespace CJLang.Execution;

internal static class Executor
{
    public static CJVar? RetVal { get; set; }

    public static List<(string name, Instruction instr)> InstructionRunners = [];

    public static int? NextLine { get; set; } = null;

    public static CJProg Prog { get; private set; } = null!;

    public static void Execute(CJProg prog)
    {
        Prog = prog;

        InstructionRunners =
        [
            ("clear", new ClearInstruction()),
            ("elif", new ElifInstruction()),
            ("else", new ElseInstruction()),
            ("if", new IfInstruction()),
            ("input", new InputInstruction()),
            ("new", new NewVarInstruction()),
            ("print", new PrintInstruction()),
            ("set", new SetVarInstruction()),
            ("str_concat", new StrConcatInstruction()),
            ("while", new WhileInstruction()),
            ("return", new ReturnInstruction()),
        ];

        //find main
        if (!prog.Funcs.TryGetValue("main", out CJFunc? value))
            throw new Exception("No main function found");

        var currentFunc = value;

        try
        {
            ProcessLines(currentFunc.Instrs, currentFunc);
        }
        catch (Exception e)
        {
            currentFunc.ErrorMessage = e.Message;
            if (currentFunc.ExceptionInstrs.Count != 0)
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

    public static void ProcessLines(List<(string line, int globalLineNum)> instrs, CJFunc func)
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
            foreach (var (name, instr) in InstructionRunners)
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
                else
                {
                    bool called = false;
                    foreach (var (fnName, fn) in Prog.Funcs)
                    {
                        if (line.StartsWith(fnName))
                        {
                            new CallFunctionInstruction().Run(func, line, globalLineNum, localLineNum);
                            called = true;
                            break;
                        }
                    }

                    if (called)
                        break;
                }
            }
        }
    }

}
