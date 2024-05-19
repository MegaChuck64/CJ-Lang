namespace CJLang.Lang;

internal class CJClass
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, CJVar> Fields { get; set; } = [];
    public Dictionary<string, CJFunc> Methods { get; set; } = [];
    public CJFunc? Constructor { get; set; } = null;
    public CJFunc? ExceptionHandler { get; set; } = null;
}