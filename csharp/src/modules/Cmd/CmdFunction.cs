using System;
using DynamicScript.Runtime;
using DynamicScript.Runtime.Environment;

namespace DynamicScript.Modules.Cmd
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Process = System.Diagnostics.Process;

    [ComVisible(false)]
    public sealed class CmdFunction : ScriptFunc<ScriptString, ScriptString, ScriptInteger>
    {
        public const string Name = "cmd";
        private const string FirstParamName = "command";
        private const string SecondParamName = "arguments";
        private const string ThirdParamName = "timeout";

        public CmdFunction()
            : base(new ScriptFunctionContract.Parameter(FirstParamName, ScriptStringContract.Instance), 
            new ScriptFunctionContract.Parameter(SecondParamName, ScriptStringContract.Instance), 
            new ScriptFunctionContract.Parameter(ThirdParamName, ScriptIntegerContract.Instance), 
            ScriptIntegerContract.Instance)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        /// <param name="timeout"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private static ScriptInteger Cmd(ScriptString command, ScriptString arguments, ScriptInteger timeout, InterpreterState state)
        {
            if (command == null || arguments == null || timeout == null) throw new ArgumentException();
            using (var p = Process.Start(command, arguments))
                return p.WaitForExit((int)timeout) ? p.ExitCode : long.MinValue;
        }

        public override IScriptObject Invoke(ScriptString command, ScriptString arguments, ScriptInteger timeout, InterpreterState state)
        {
            return Cmd(command, arguments, timeout, state);
        }

        /// <summary>
        /// Executes an entry point of the module.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IScriptObject Run(InterpreterState state)
        {
            return new CmdFunction();
        }
    }
}
