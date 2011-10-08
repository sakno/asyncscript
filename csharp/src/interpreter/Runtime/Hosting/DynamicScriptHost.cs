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
            using (var p = new Compiler.Ast.SyntaxAnalyzer("checked: 2+3;"))
                while (p.MoveNext())
                    Console.WriteLine(p.Current.ToString());
            return Execute(new CommandLineParser(Console.Out, Console.In), args);
        }
    }
}
