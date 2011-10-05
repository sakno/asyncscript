using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeVariableReference = Compiler.Ast.ScriptCodeVariableReference;

    /// <summary>
    /// Represents contract of name token runtime representation.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptNameTokenExpressionFactory : ScriptExpressionFactory<ScriptCodeVariableReference, ScriptNameTokenExpression>
    {
        #region Nested Types

        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "newname";

            public ModifyAction()
                : base(Instance,new ScriptActionContract.Parameter(SecondParamName, ScriptStringContract.Instance))
            {
            }
        }
        #endregion

        /// <summary>
        /// Represents name of this contract.
        /// </summary>
        public new const string Name = "nmtoken";

        private IRuntimeSlot m_modify;

        private ScriptNameTokenExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptNameTokenExpressionFactory Instance = new ScriptNameTokenExpressionFactory();

        /// <summary>
        /// Creates a new runtime representation of 
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public static ScriptNameTokenExpression CreateExpression(ScriptString variableName)
        {
            return new ScriptNameTokenExpression(ScriptNameTokenExpression.CreateExpression(variableName));
        }

        public override ScriptNameTokenExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            if (args.Count == 0)
                switch (args[0] is ScriptString)
                {
                    case true: return CreateExpression((ScriptString)args[0]);
                    default: throw new ContractBindingException(args[0], ScriptStringContract.Instance, state);
                }
            else throw new ActionArgumentsMistmatchException(state);
        }

        public override IScriptObject Convert(IScriptObject value, InterpreterState state)
        {
            if (value is ScriptNameTokenExpression)
                return value;
            else if (value is ScriptString)
                return CreateExpression((ScriptString)value);
            else return Void;
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
