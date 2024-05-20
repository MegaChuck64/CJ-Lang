using Lang;
using System.ComponentModel;

namespace Parser;

public class Parser
{
    public static CJProg Parse(string path)
    {
        //check if file or directory
        var progDir = new ProgDirectory(path);
        var prog = new CJProg();
        var lines = progDir.GetCombinedLines();
        prog.RawLines = lines;
        
        //get zero indent lines, which are global functions and class declarations
        var zeroIndentLines = GetLineNumsOfIndentLevel(lines.Select(t=>t.line).ToList(), 0);

        //parse class definitions 
        var classNames = new List<(string className, int lineNum)>();
        foreach (var lineNum in zeroIndentLines)
        {
            var line = lines.ElementAt(lineNum).line;
            if (IsClassDef(line))
            {
                //class MyClass:
                var className = line.Split(' ')[1].TrimEnd(':').Trim();
                if (classNames.Any(t=>t.className == className))
                    throw new Exception($"Class '{className}' already defined. ");

                classNames.Add((className, lineNum));
            }
        }

        var typeMap = GenerateTypeMap(classNames);

        prog.TypeMap = typeMap;

        //parse global functions

        foreach (var lineNum in zeroIndentLines)
        {
            var line = lines.ElementAt(lineNum).line;
            if (IsClassDef(line))
                continue;

            if (!TryGetFuncType(line, typeMap, out var funcType))
                throw new Exception($"Invalid function type '{line.Split(' ')[0]}'");

            //str welcomeMessage(str name, u8 age):

            var splt = line.Split(' ');
            var funcName = splt[1].Split('(')[0];
            var argSplt = line.Split('(')[1].Split("):", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            //if no args
            var args = new Dictionary<string, CJVar>();
            if (argSplt.Length == 1)
            {
                var argInfo = new List<(string type, string name)>();
                var argStrs = argSplt[0].Split(',', StringSplitOptions.TrimEntries  | StringSplitOptions.RemoveEmptyEntries);

                foreach (var ag in argStrs)
                {
                    var s = ag.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    argInfo.Add((s[0], s[1]));
                }

                foreach (var (type, name) in argInfo)
                {
                    if (!typeMap.ContainsKey(type))
                        throw new Exception($"Invalid argument type '{type}'");

                    args[name] = new CJVar
                    {
                        Name = name,
                        Type = typeMap[type],
                    };
                }
            }

            var func = new CJFunc
            {
                Name = funcName,
                LineNum = lineNum,
                Args = args,
                Ret = new CJVar
                {
                    Type = funcType,
                },
            };
            
            if (funcName == "main" || funcName == "Main")
                prog.Main = func;
            else
                prog.Funcs[funcName] = func;


        }

        //add classes
        var classes = new Dictionary<string, CJClass>();
        foreach ((string className, int linNum) in classNames)
        {
            var cls = new CJClass
            {
                Name = className,
                LineNum = linNum,
            };

            classes[className] = cls;
        }

        prog.Classes = classes;

        foreach (var cls in prog.Classes)
        {
            var className = cls.Key;
            var lineNum = cls.Value.LineNum;
            var currentLine = lineNum + 1;
            while (true)
            {
                var line = lines[currentLine].line;
                if (!line.StartsWith('\t'))
                    break;

                //if line starts with one tab, it is either a class variable or a function declaration
                if (line.StartsWith('\t') && !line.StartsWith("\t\t"))
                {
                    //func declaration
                    if (line.Trim().EndsWith(':'))
                    {
                        var splt = line.Split(' ');
                        var typ = splt[0];
                        var funcName = splt[1].Split('(')[0];
                        var argSplt = line.Split('(')[1].Split("):", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                        //if no args
                        var args = new Dictionary<string, CJVar>();

                    }
                }
            }
        }


        return prog;
    }

    private static CJFunc ParseFunc(List<string> lines, string definitionLine, Dictionary<string, CJType> typeMap, int lineNum, int indentLevel)
    {

        if (!TryGetFuncType(definitionLine, typeMap, out var funcType))
            throw new Exception($"Invalid function type '{definitionLine.Split(' ')[0]}'");

        //str welcomeMessage(str name, u8 age):

        var splt = definitionLine.Split(' ');
        var funcName = splt[1].Split('(')[0];
        var argSplt = definitionLine.Split('(')[1].Split("):", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        //if no args
        var args = new Dictionary<string, CJVar>();
        if (argSplt.Length == 1)
        {
            var argInfo = new List<(string type, string name)>();
            var argStrs = argSplt[0].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            foreach (var ag in argStrs)
            {
                var s = ag.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                argInfo.Add((s[0], s[1]));
            }

            foreach (var (type, name) in argInfo)
            {
                if (!typeMap.ContainsKey(type))
                    throw new Exception($"Invalid argument type '{type}'");

                args[name] = new CJVar
                {
                    Name = name,
                    Type = typeMap[type],
                };
            }
        }
        
        int i = 0;
        foreach (var line in lines)
        {
            //if func indent level is 1, body should be 2, or +1 more for each block defined with the ':' colon
            var lineIndent = line.TakeWhile(c => c == '\t').Count();

            //if (testA == testB):
            //  print("test")
            //  print("test2")

            if (lineIndent == indentLevel + 1)
            {
                ///
            }

        }



        var func = new CJFunc
        {
            Name = funcName,
            LineNum = lineNum,
            Args = args,
            Ret = new CJVar
            {
                Type = funcType,
            },
        };

        return func;
    }

    private static Dictionary<string, CJType> GenerateTypeMap (List<(string className, int lineNum)> classNames)
    {
        var typeMap = new Dictionary<string, CJType>();

        foreach (var typ in Enum.GetValues(typeof(CJType)))
        {
            var type = (CJType)typ;

            var name = type.ToString();
            if (type == CJType._class)
                continue;

            if (type == CJType._void)
                name = "void";
            else if (type == CJType._bool)
                name = "bool";

            typeMap[name] = type;
        }

        foreach (var (className, lineNum) in classNames)
        {
            var classType = CJType._class;
            typeMap[className] = classType;
        }
        return typeMap;
    }

    private static bool IsClassDef(string line) => line.StartsWith("class") && line.TrimEnd().EndsWith(":");

    private static bool TryGetFuncType(string line, Dictionary<string, CJType> types, out CJType type)
    {
        line = line.Trim();
        var splt = line.Split(' ');
        var typeStr = splt[0];

        if (types.TryGetValue(typeStr, out type))
            return true;

        type = CJType._void;
        return false;
    }

    private static List<int> GetLineNumsOfIndentLevel(List<string> lines, int indentLevel)
    {
        var lineNums = new List<int>();
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var indent = line.TakeWhile(c => c == '\t').Count();
            if (indent == indentLevel)
                lineNums.Add(i);
        }
        return lineNums;
    }
}

public class ProgDirectory
{
    public string Path { get; set; }
    public Dictionary<string, List<string>> Files { get; set; }

    public ProgDirectory(string path)
    {
        Path = path;
        Files = [];
        if (File.Exists(path))
        {
            Path = System.IO.Path.GetDirectoryName(path) ?? 
                throw new Exception("Possible file in root directory");

            var lines = File.ReadAllLines(path).ToList();
            var fileName = System.IO.Path.GetFileName(path);
            Files[fileName] = lines;
        }
        else if (Directory.Exists(path))
        {
            var files = Directory.GetFiles(path, "*.cj");
            foreach (var file in files)
            {
                var lines = File.ReadAllLines(file).ToList();
                Files[file] = lines;
            }
        }
        else
        {
            throw new Exception("File or directory not found");
        }
    }

    public List<(string line, string fileName, int lineNum)> GetCombinedLines()
    {
        var lines = new List<(string line, string fileName, int lineNum)>();

        foreach (var (fileName, fileLines) in Files)
        {
            for (int i = 0; i < fileLines.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(fileLines[i]))
                    continue;

                if (fileLines[i].StartsWith("//"))
                    continue;

                lines.Add((fileLines[i], fileName, i));
            }
        }


        return lines;
    }
}

