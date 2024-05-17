using CJLang.Execution;
using CJLang.Lang;

namespace CJLang.Instructions;

[Instruction("else", "Else statement")]
internal class ElseInstruction : Instruction
{
    public override void Run(CJFunc currentFunc, string line, int globalLineNum, int localLineNum)
    {
        if ((currentFunc.LastBlockConditionResult ?? false) == false)
        {
            var lines = currentFunc.Blocks[globalLineNum];

            Executor.ProcessLines(lines, currentFunc);

            currentFunc.LastBlockConditionResult = null;
        }

    }
}