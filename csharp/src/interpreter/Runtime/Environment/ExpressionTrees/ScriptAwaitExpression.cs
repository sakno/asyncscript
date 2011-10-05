using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using RuntimeSynchronizationManager = Threading.RuntimeSynchronizationManager;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptAwaitExpression: ScriptExpression<ScriptCodeAwaitExpression, ScriptFunc>
    {
        private ScriptAwaitExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptAwaitExpression(ScriptCodeAwaitExpression expression)
            : base(expression, ScriptAwaitExpressionFactory.Instance)
        {
        }

        public override ScriptFunc Compile(InterpreterState state)
        {
            return ScriptActionInvoker.Compile(new[] { Expression });
        }

        public static ScriptCodeAwaitExpression CreateExpression(IScriptExpression<ScriptCodeExpression> asyncResult, IScriptExpression<ScriptCodeExpression> synchronizer = null)
        {
            return asyncResult != null ? new ScriptCodeAwaitExpression
            {
                AsyncResult = asyncResult.CodeObject,
                Synchronizer = synchronizer != null ? synchronizer.CodeObject : null
            } : null;
        }

        protected override ScriptCodeAwaitExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 1: return CreateExpression(args[0] as IScriptExpression<ScriptCodeExpression>);
                case 2: return CreateExpression(args[0] as IScriptExpression<ScriptCodeExpression>, args[1] as IScriptExpression<ScriptCodeExpression>);
                default: return null;
            }
        }
    }
}
