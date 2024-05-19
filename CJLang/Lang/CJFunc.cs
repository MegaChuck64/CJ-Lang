namespace CJLang.Lang;

internal class CJFunc
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, CJVar> Args { get; set; } = [];
    public Dictionary<string, CJVar> Locals { get; set; } = [];
    public CJVar? Ret { get; set; } = null;
    public List<(string line, int globalLineNum)> Instrs { get; set; } = [];
    public Dictionary<int, List<(string line, int globalLineNum)>> Blocks { get; set; } = [];
    public bool? LastBlockConditionResult { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorVarName { get; set; } = string.Empty;
    public bool ErrorHandled { get; set; } = false;
    public List<(string line, int globalLineNum)> ExceptionInstrs { get; set; } = [];
}
