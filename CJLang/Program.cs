
using CJLang.Lang;

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
                var prog = new CJProg(lines);
                prog.Execute();
            }
            catch(Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
        }
    }

 



  







}
