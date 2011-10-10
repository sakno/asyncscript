using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    abstract class ScriptLoopExpressionFactory<TLoopKind, TRuntimeExpression> : ScriptExpressionFactory<TLoopKind, TRuntimeExpression>, ILoopExpressionFactorySlots
        where TLoopKind : ScriptCodeLoopExpression
        where TRuntimeExpression : ScriptObject, IScriptExpression<TLoopKind>
    {
        #region Nested Types
        [ComVisible(false)]
        protected abstract class GetBodyActionBase : CodeElementPartProvider<IScriptCodeElement<ScriptCodeExpression>>
        {
            protected GetBodyActionBase(ScriptCodeElementFactory<TLoopKind, TRuntimeExpression> firstParam)
                : base(firstParam, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected sealed override IScriptCodeElement<ScriptCodeExpression> Invoke(TLoopKind element, InterpreterState state)
            {
                return Convert(element.Body.Expression) as IScriptCodeElement<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        protected abstract class GetGroupingActionBase : CodeElementPartProvider<IScriptObject>
        {
            protected GetGroupingActionBase(ScriptCodeElementFactory<TLoopKind, TRuntimeExpression> firstParam)
                : base(firstParam)
            {
            }

            protected sealed override IScriptObject Invoke(TLoopKind element, InterpreterState state)
            {
                if (element.Grouping is ScriptCodeForEachLoopExpression.OperatorGrouping)
                    return (ScriptString)ScriptCodeBinaryOperatorExpression.ToString(((ScriptCodeForEachLoopExpression.OperatorGrouping)element.Grouping).Operator);
                else if (element.Grouping is ScriptCodeForEachLoopExpression.CustomGrouping)
                    return Convert(((ScriptCodeForEachLoopExpression.CustomGrouping)element.Grouping).GroupingAction);
                else return Void;
            }
        }
        #endregion
        protected ScriptLoopExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        protected ScriptLoopExpressionFactory(string contractName)
            : base(contractName)
        {
        }

        IRuntimeSlot ILoopExpressionFactorySlots.Grouping
        {
            get { return Grouping; }
        }

        protected abstract IRuntimeSlot Grouping
        {
            get;
        }

        IRuntimeSlot IComplexExpressionFactorySlots.Body
        {
            get { return Body; }
        }

        protected abstract IRuntimeSlot Body
        {
            get;
        }
    }
}
