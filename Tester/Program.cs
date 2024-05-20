namespace Tester;

internal class Program
{
    public static void Main(string[] args)
    {
#if DEBUG
        args = [ "TestProject" ];

#endif

        if (args.Length == 0)
        {
            Console.WriteLine("No file specified");
            return;
        }
        
        var prog = Parser.Parser.Parse(args[0]);

    }
}