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
var clr = use('D:\DynamicScript\csharp\src\modules\ClrTypes\bin\Debug\clrtypes.dll');
var clazz = clr.system.class('System.Uri');
var uri = clazz('http://www.ya.ru');
return uri;
");
            return Execute(new CommandLineParser(Console.Out, Console.In), args);
        }
    }
}
