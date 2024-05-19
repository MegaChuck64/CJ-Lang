
namespace CJLang.Lang;

internal class CJProg
{
    public Dictionary<string, CJClass> Classes { get; private set; } = [];

    public CJClass MainClass { get; private set; } = null!;

    public CJProg(params CJClass[] classes)
    {
        foreach (var cls in classes)
        {
            if (cls.Methods.Any(x => x.Key == "main"))
            {
                MainClass = cls;
            }
            else
            {
                Classes.Add(cls.Name, cls);
            }
        }

        if (MainClass == null)
            throw new Exception("No main method found");
    }
}