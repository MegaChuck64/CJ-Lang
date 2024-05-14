
using CJLang.Lang;

namespace CJLang.Instructions;

internal abstract class Instruction
{
    public abstract string Name { get; }
    public abstract void Run(CJProg prog, CJFunc currentFunc, string line);
}
