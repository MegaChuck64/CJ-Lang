using CJLang.Execution;
using CJLang.Lang;
namespace CJLang.Instructions
{
    internal class ThrowInstruction : Instruction
    {
        internal static readonly string[] separator = ["\\n"];

        public override void Run(CJFunc currentFunc, string line, int globalLineNum, int localLineNum)
        {
            //throw("Hello, ", userName, ". You are ", userAgeStr, " years old.\n")
            var start = line.IndexOf('(');
            var prmpt = line.Substring(start + 1, line.Length - start - 2);

            var str = Helper.GetStrFromConcat(currentFunc, prmpt, globalLineNum + 1);

            throw new ExecutorException(str, globalLineNum);
        }
    }
}
