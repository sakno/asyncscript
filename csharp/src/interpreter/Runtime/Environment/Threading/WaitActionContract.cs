using System;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class WaitActionContract: ScriptActionContract
    {
        private const string FirstParamName = "timeout";

        private WaitActionContract()
            : base(new[] { new Parameter(FirstParamName, ScriptRealContract.Instance) }, ScriptBooleanContract.Instance)
        {
        }

        public static readonly WaitActionContract Instance = new WaitActionContract();
    }
}
