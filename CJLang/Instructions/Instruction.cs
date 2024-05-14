
using CJLang.Lang;

namespace CJLang.Instructions;

internal abstract class Instruction
{
    public abstract void Run(CJProg prog, CJFunc currentFunc, string line);
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