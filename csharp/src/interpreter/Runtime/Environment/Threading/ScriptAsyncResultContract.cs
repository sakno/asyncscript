using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IScriptContract = DynamicScript.Runtime.IScriptContract;

    /// <summary>
    /// Represents contract of task asynchronous result.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class ScriptAsyncResultContract : ScriptCompositeContract
    {
        #region Nested Types
        internal static class CancelSlot
        {
            public const string Name = "cancel";
            public const bool IsConstant = true;
            public static readonly ScriptActionContract ContractBinding = new ScriptActionContract(new ScriptActionContract.Parameter[0], Void);
        }

        [ComVisible(false)]
        internal static class ResultSlot
        {
            public const string Name = "result";
            public const bool IsConstant = true;
        }

        [ComVisible(false)]
        internal static class CompletedSlot
        {
            public const string Name = "completed";
            public static readonly ScriptBooleanContract ContractBinding = ScriptBooleanContract.Instance;
            public const bool IsConstant = true;
        }

        [ComVisible(false)]
        internal static class WaitSlot
        {
            public const string Name = "wait";
            public static readonly WaitActionContract ContractBinding = WaitActionContract.Instance;
            public const bool IsConstant = true;
        }

        [ComVisible(false)]
        internal static class NotifierSlot
        {
            public const string Name = "notifier";
            public static readonly ScriptBuiltinContract ContractBinding = ScriptSuperContract.Instance;
            public const bool IsConstant = false;
        }
        #endregion

        private static new IEnumerable<KeyValuePair<string, SlotMeta>> Slots(IScriptContract resultContract)
        {
            yield return DefineSlot(CancelSlot.Name, CancelSlot.ContractBinding, CancelSlot.IsConstant);
            yield return DefineSlot(ResultSlot.Name, resultContract ?? ScriptSuperContract.Instance, ResultSlot.IsConstant);
            yield return DefineSlot(CompletedSlot.Name, CompletedSlot.ContractBinding, CompletedSlot.IsConstant);
            yield return DefineSlot(WaitSlot.Name, WaitSlot.ContractBinding, WaitSlot.IsConstant);
            yield return DefineSlot(NotifierSlot.Name, NotifierSlot.ContractBinding, NotifierSlot.IsConstant);
        }

        /// <summary>
        /// Initializes a new asynchronous representation of the specified contract.
        /// </summary>
        /// <param name="resultContract"></param>
        public ScriptAsyncResultContract(IScriptContract resultContract = null)
            : base(Slots(resultContract))
        {
        }

        internal static NewExpression Bind(Expression contractExpr)
        {
            contractExpr = Extract(contractExpr);
            var ctor = LinqHelpers.BodyOf<IScriptContract, ScriptAsyncResultContract, NewExpression>(c => new ScriptAsyncResultContract(c));
            return ctor.Update(new[] { contractExpr });
        }
    }
}
