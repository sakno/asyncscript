﻿using System;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Hosting;

namespace DynamicScript.Runtime.Hosting
{
    using ScriptDebugger = Debugging.ScriptDebugger;
    using InteractiveDebugger = Debugging.Interaction.InteractiveDebugger;
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
            Compiler.Punctuation.HashCodes.PrintPunctuationValues(Console.Out);
            ScriptDebugger.Debugging += InteractiveDebugger.Hook;
            return Execute(new CommandLineParser(Console.Out, Console.In), args);
        }
    }
}
