namespace CJLang.Lang;

internal class CJFunc
{
    public string Name { get; set; }
    public Dictionary<string, CJVar> Args { get; set; } = [];
    public Dictionary<string, CJVar> Locals { get; set; } = [];
    public CJVar Ret { get; set; }
    public List<(string line, int globalLineNum)> Instrs { get; set; }
    public Dictionary<int, List<(string line, int globalLineNum)>> Blocks { get; set; }
    public bool? LastBlockConditionResult { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorVarName { get; set; }
    public List<(string line, int globalLineNum)> ExceptionInstrs { get; set; }
}
