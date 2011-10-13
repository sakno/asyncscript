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
const a = @i: real -> real: i + 10;
return $a;
");
            var s = Equals(new DynamicScript.Runtime.Environment.ScriptActionContract(new[] { new Runtime.Environment.ScriptActionContract.Parameter("i", DynamicScript.Runtime.Environment.ScriptRealContract.Instance) }, DynamicScript.Runtime.Environment.ScriptRealContract.Instance), r);
            return Execute(new CommandLineParser(Console.Out, Console.In), args);
        }
    }
}
