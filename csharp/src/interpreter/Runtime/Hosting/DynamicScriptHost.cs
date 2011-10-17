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
            var r = DynamicScriptInterpreter.Run(@"
const z = {{a = 10, b = 20}} to finset;
return 10 is z;
");
            return Execute(new CommandLineParser(Console.Out, Console.In), args);
        }
    }
}
