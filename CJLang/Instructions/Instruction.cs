
using CJLang.Lang;

namespace CJLang.Instructions;

internal abstract class Instruction
{
    public abstract void Run(CJFunc currentFunc, string line, int lineNum);
}


internal class InstructionAttribute : Attribute
{
    public string Name { get; set; }
    public string Description { get; set; }
    public InstructionAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}