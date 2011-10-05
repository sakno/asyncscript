using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptConstantExpressionFactory : ScriptExpressionFactory<ScriptCodePrimitiveExpression, ScriptConstantExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "value";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, ScriptSuperContract.Instance))
            {
            }
        }
        #endregion
        public new const string Name = "constant";

        private IRuntimeSlot m_modify;
        
        private ScriptConstantExpressionFactory()
            : base(Name)
        {
        }

        public static ScriptConstantExpressionFactory Instance = new ScriptConstantExpressionFactory();

        public static ScriptConstantExpression CreateExpression(IScriptObject value, bool emitDefault = true)
        {
            var expression = ScriptConstantExpression.CreateExpression(value);
            return expression != null || emitDefault ?
                new ScriptConstantExpression(expression ?? ScriptCodeVoidExpression.Instance) : null;
        }

        public override ScriptConstantExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 1: return CreateExpression(args[0]);
                default: throw new ActionArgumentsMistmatchException(state);
            }
        }

        protected override bool Mapping(ref IScriptObject value)
        {
            return value is ScriptConstantExpression;
        }

        public override IScriptObject Convert(IScriptObject value, InterpreterState state)
        {
            switch (value is ScriptConstantExpression)
            {
                case true: return value;
                default:
                    value = CreateExpression(value, false);
                    return value ?? Void;
            }
        }

        public override void Clear()
        {
            m_modify = null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }
    }
}
