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
            IScriptObject r = DynamicScriptInterpreter.Run(@"
return split({{a = 1, b = 2}});
");
            var objects = DynamicScript.Runtime.Environment.ScriptIterator.AsEnumerable(r, InterpreterState.Current);
            var count = System.Linq.Enumerable.LongCount(objects);
            return Execute(new CommandLineParser(Console.Out, Console.In), args);
        }
    }
}
