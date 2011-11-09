using System;
using System.Linq.Expressions;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents contract of the action that is used to synchronize access
    /// to the scheduled result.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class ScriptAwaitContract: ScriptActionContract
    {
        //@timeout: real, failure: object -> object;
        private ScriptAwaitContract()
            : base(new[] { new Parameter("timeout", ScriptRealContract.Instance), new Parameter("failure", ScriptSuperContract.Instance) }, ScriptSuperContract.Instance)
        {
        }

        /// <summary>
        /// Represents singleton instance of this contract.
        /// </summary>
        public static readonly ScriptAwaitContract Instance = new ScriptAwaitContract();

        internal static MemberExpression New()
        {
            return LinqHelpers.BodyOf<Func<ScriptAwaitContract>, MemberExpression>(() => Instance);
        }
    }
}
