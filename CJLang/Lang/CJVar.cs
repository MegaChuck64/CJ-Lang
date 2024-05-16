
namespace CJLang.Lang;

public class CJVar
{
    public string Name { get; set; } = string.Empty;
    public CJVarType Type { get; set; }
    public bool IsArray { get; set; }
    public int ArraySize { get; set; }
    public object? Value { get; set; }
}

