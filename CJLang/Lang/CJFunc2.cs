using CJLang.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CJLang.Lang;

internal class CJFunc2
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, CJVar> Args { get; set; } = [];
    public Dictionary<string, CJVar> Locals { get; set; } = [];
    public CJVar? Ret { get; set; } = null;
    public List<(string line, int globalLineNum)> Instrs { get; set; } = [];
    public Dictionary<int, List<(string line, int globalLineNum)>> Blocks { get; set; } = [];    
    public List<(string line, int globalLineNum)> ExceptionInstrs { get; set; } = [];
}