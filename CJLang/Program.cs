
using CJLang.Execution;

namespace CJLang
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            args = ["Test.cj"];
#endif

            if (args.Length == 0)
            {
                Console.WriteLine("No file specified");
                return;
            }
            try
            {
                var lines = File.ReadAllLines(args[0]).ToList();
                var prog = Parser.Parse(lines);
                Executor.Execute(prog);
            }
            catch(Exception e)
            {
                var msg = e.Message;//e.Message.Replace("->", "\n\t->");
                Console.Write("Unhandled Exception: ");
                Console.WriteLine(msg);
            }
        }
    }

 



  







}
