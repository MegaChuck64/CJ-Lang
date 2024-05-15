using CJLang.Lang;

namespace CJLang.Instructions;

[Instruction("else", "Else statement")]
internal class ElseInstruction : Instruction
{
    public override void Run(CJFunc currentFunc, string line, int lineNum)
    {
        if ((currentFunc.LastIfResult ?? false) == false)
        {
            var lines = currentFunc.Blocks[lineNum];

            CJProg.ProcessLines(lines, currentFunc, CJProg.InstructionRunners);

            currentFunc.LastIfResult = null;
        }

    }
}