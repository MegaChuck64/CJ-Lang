using CJLang.Execution;
using CJLang.Lang;

namespace CJLang.Instructions;

internal class ThrowInstruction : Instruction
{
    public override void Run(CJFunc currentFunc, string line, int globalLineNum, int localLineNum)
    {
        var start = line.IndexOf('(');
        var prmpt = line.Substring(start + 1, line.Length - start - 2);

        var str = Helper.GetStrFromConcat(currentFunc, prmpt, globalLineNum + 1);

        currentFunc.ErrorMessage = string.Empty;
        throw new ExecutorException(str, globalLineNum);
    }
}