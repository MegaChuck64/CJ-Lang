using CJLang.Execution;
using CJLang.Lang;
namespace CJLang.Instructions;

internal class ReturnInstruction : Instruction
{
    public override void Run(CJFunc currentFunc, string line, int globalLineNum, int localLineNum)
    {
        //return
        //return <value>
        var splt = line.Split([' ']);
        if (splt.Length == 1)
        {
            Executor.RetVal = null;
        }
        else
        {
            var value = splt[1];
            if (currentFunc.Locals.TryGetValue(value, out CJVar? var))
            {
                Executor.RetVal = var;
                //Executor.NextLine = -1;
            }
            else
            {
                throw new ExecutorException($"Variable '{value}' not found", globalLineNum);
            }
        }
    }
}