
namespace CJLang.Lang;

public class CJVar
{
    public string Name { get; set; } = string.Empty;
    public CJVarType Type { get; set; }
    public bool IsArray { get; set; }
    public object? Value { get; set; }
    public object?[]? ArrayValue { get; set; }
}

