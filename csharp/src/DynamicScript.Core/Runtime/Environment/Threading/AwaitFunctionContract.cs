using System;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class AwaitFunctionContract: ScriptFunctionContract
    {
        internal const string FirstParamName = "timeout";
        internal const string SecondParamName = "failure";

        private AwaitFunctionContract()
            : base(new[] { new Parameter(FirstParamName, ScriptRealContract.Instance), new Parameter(SecondParamName, ScriptSuperContract.Instance) }, ScriptSuperContract.Instance)
        {
        }

        public static readonly AwaitFunctionContract Instance = new AwaitFunctionContract();
    }
}
