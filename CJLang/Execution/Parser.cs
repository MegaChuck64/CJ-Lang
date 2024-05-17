using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CJLang.Lang;

namespace CJLang.Execution;

internal static class Parser
{
    internal static readonly string[] lineSeperator = [" ", ":", "->", ","];

    internal static CJProg Parse(List<string> lines)
    {
        Dictionary<string, CJFunc> Funcs = [];

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
                inException = false;

                var splt = lines[i].Split([' ']);
                var funcName = splt[1].Split(':')[0].Trim();
                currentFunc = funcName;
                var retType = splt[0];
                if (retType == "exception")
                {
                    //check that the function already exists
                    if (!Funcs.TryGetValue(funcName, out CJFunc? value))
                        throw new Exception("Function not found");

                    inException = true;
                    value.ExceptionInstrs = [];
                    retType = "void";
                }

                if (!Helper.TryGetType(retType, out var ret))
                    throw new Exception("Invalid return type");

                var args = new Dictionary<string, CJVar>();
                //parse args
                //splt on space, :, ->, and ,
                var argStrs = lines[i].Split(lineSeperator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                argStrs = argStrs.Skip(2).ToArray();
                var exceptionVarName = string.Empty;
                var argCount = 0;
                for (int j = 0; j < argStrs.Length; j += 2)
                {
                    //i32 testFunc: i32 age, str name
                    var typeStr = argStrs[j];
                    var argName = argStrs[j + 1];

                    var isArray = typeStr.EndsWith("[]");
                    if (isArray)
                        typeStr = typeStr[..^2];

                    if (!Helper.TryGetType(typeStr, out var argType))
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

                if (lines[i].Trim().StartsWith("if") || lines[i].Trim().StartsWith("while"))
                {
                    inBlock = true;
                    blockNum = i;
                }
                else if (lines[i].Trim().StartsWith("else") || lines[i].Trim().StartsWith("elif"))
                {
                    if (!inBlock)
                        throw new Exception("Else without if");

                    inBlock = true;
                    blockNum = i;
                }
                else if (lines[i].StartsWith("\t\t") && inBlock)
                {
                    if (Funcs[currentFunc].Blocks == null)
                        Funcs[currentFunc].Blocks = [];

                    if (!Funcs[currentFunc].Blocks.ContainsKey(blockNum))
                        Funcs[currentFunc].Blocks.Add(blockNum, []);

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

        var prog = new CJProg
        {
            Funcs = Funcs,
        };

        return prog;
    }
}