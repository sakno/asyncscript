using System;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Hosting;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents DynamicScript host.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class DynamicScriptHost : ScriptHost
    {
        private static int Execute(CommandLineParser cmd, string[] args)
        {
            return cmd.Run(args);
        }

        [LoaderOptimization(LoaderOptimization.SingleDomain)]
        private static int Main(string[] args)
        {
            using (var an = new Compiler.Ast.SyntaxAnalyzer("throw 2;"))
                while (an.MoveNext())
                    Console.WriteLine(an.Current);
            var r = DynamicScriptInterpreter.Run(@"
            throw 2;
");
            return Execute(new CommandLineParser(Console.Out, Console.In), args);
        }
    }
}
