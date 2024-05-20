namespace Lang;

public class CJProg
{
    public List<(string line, string fileName, int lineNum)> RawLines { get; set; } = new();
    public Dictionary<string, CJType> TypeMap { get; set; } = new();
    public Dictionary<string, CJClass> Classes { get; set; } = new();
    public Dictionary<string, CJFunc> Funcs { get; set; } = new();

    public CJFunc? Main { get; set; } 

}

public class CJClass
{
    public int LineNum { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, CJVar> Vars { get; set; } = new();
    public Dictionary<string, CJFunc> Funcs { get; set; } = new();
    public CJFunc? Constructor { get; set; }
}

public class CJFunc
{
    public int LineNum { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, CJVar> Args { get; set; } = new();
    public Dictionary<string, CJVar> Locals { get; set; } = new();
    public CJVar? Ret { get; set; }
    public List<CJInstruction> Instrs { get; set; } = new();
    public Dictionary<int, List<CJInstruction>> Blocks { get; set; } = new();
}

public class  CJVar
{
    public string Name { get; set; } = string.Empty;
    public CJType Type { get; set; }
    public bool IsArray { get; set; }
    public object? Value { get; set; }
    public object? ArrayValue { get; set; }
}

public class CJInstruction
{
    public string Name { get; set; } = string.Empty;
    public string Line { get; set; } = string.Empty;
    public int GlobalLineNum { get; set; }

    public int LineNum { get; set; }
    public bool IsBlock { get; set; }
}

public enum CJType
{
    u8,
    u16,
    u32,
    u64,
    i8,
    i16,
    i32,
    i64,
    f32,
    f64,
    str,
    _bool,
    _void,
    _class,    
}