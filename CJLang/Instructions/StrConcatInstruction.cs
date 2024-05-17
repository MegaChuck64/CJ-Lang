using CJLang.Execution;
using CJLang.Lang;
namespace CJLang.Instructions;


[Instruction("str_concat", "Concatenates literals and variables and sets a string variable as ouput")]
internal class StrConcatInstruction : Instruction
{
    public override void Run(CJFunc currentFunc, string line, int globalLineNum, int localLineNum)
    {
        //str_concat("Hello, ", userName, ". You are ", userAgeStr, " years old.\n") -> dest
        var splt = line.Split(['(']);
        var prmpt = splt[1].Split([')'])[0];
        var destVar = line.Split(["->"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[1];
        if (!currentFunc.Locals.TryGetValue(destVar, out CJVar? value))
            throw new Exception("Variable not found");

        var str = Helper.GetStrFromConcat(currentFunc, prmpt);
        value.Value = str;

    }
}