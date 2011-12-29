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

        private static Environment.ScriptInteger Add1(long left, long right, InterpreterState state)
        {
            return state.Context == Compiler.Ast.InterpretationContext.Unchecked ? unchecked(left + right) : checked(left + right);
        }

        private static IScriptObject Add2(long left, IScriptObject right, InterpreterState state)
        {
            switch (System.Convert.GetTypeCode(right))
            {
                case TypeCode.Double: return new Environment.ScriptReal(left + System.Convert.ToDouble(right));
                case TypeCode.Single: return new Environment.ScriptReal(left + System.Convert.ToSingle(right));
                case TypeCode.Int64: return new Environment.ScriptInteger(left + System.Convert.ToInt64(right));
                default:
                    if (state.Context == Compiler.Ast.InterpretationContext.Unchecked)
                        return Environment.ScriptObject.Void;
                    else throw new Environment.UnsupportedOperationException(state);
            }
        }

        [LoaderOptimization(LoaderOptimization.SingleDomain)]
        private static int Main(string[] args)
        {
            return Execute(new CommandLineParser(Console.Out, Console.In), args);
        }
    }
}
