using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptVariableDeclaration: ScriptStatement<ScriptCodeVariableDeclaration>
    {
        private ScriptVariableDeclaration(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptVariableDeclaration(ScriptCodeVariableDeclaration statement)
            : base(statement, ScriptVariableDeclarationFactory.Instance)
        {
        }

        internal ScriptVariableDeclaration(ISlot slotdef)
            : this(new ScriptCodeVariableDeclaration(slotdef))
        {
        }

        public static ScriptCodeVariableDeclaration CreateStatement(string variableName, bool constant, ScriptCodeExpression initExpr, ScriptCodeExpression contractBinding)
        {
            return new ScriptCodeVariableDeclaration
            {
                Name = variableName,
                IsConst = constant,
                InitExpression = initExpr,
                ContractBinding = contractBinding
            };
        }

        public static ScriptCodeVariableDeclaration CreateStatement(string variableName, bool constant, IScriptObject initExpr, IScriptObject contractBinding)
        {
            return CreateStatement(variableName, constant,
                initExpr is IScriptExpression<ScriptCodeExpression> ? ((IScriptExpression<ScriptCodeExpression>)initExpr).CodeObject : ScriptConstantExpression.CreateExpression(initExpr),
                contractBinding is IScriptExpression<ScriptCodeExpression> ? ((IScriptExpression<ScriptCodeExpression>)contractBinding).CodeObject : ScriptConstantExpression.CreateExpression(initExpr));
        }

        public static ScriptCodeVariableDeclaration CreateStatement(IScriptExpression<ScriptCodeVariableReference> variableName, ScriptBoolean constant, IScriptObject initExpr, IScriptObject contractBinding)
        {
            return variableName != null ? CreateStatement(variableName.CodeObject.VariableName, constant, initExpr, contractBinding) : null;
        }

        public static ScriptCodeVariableDeclaration CreateStatement(IScriptExpression<ScriptCodePrimitiveExpression> variableName, ScriptBoolean constant, IScriptObject initExpr, IScriptObject contractBinding)
        {
            return variableName != null && variableName.CodeObject is ScriptCodeStringContractExpression ? CreateStatement(((ScriptCodeStringExpression)variableName.CodeObject).Value, constant, initExpr, contractBinding) : null;
        }

        public static ScriptCodeVariableDeclaration CreateStatement(IScriptObject variableName, ScriptBoolean constant, IScriptObject initExpr, IScriptObject contractBinding)
        {
            if (variableName is ScriptString)
                return CreateStatement((ScriptString)variableName, constant, initExpr, contractBinding);
            else if (variableName is IScriptExpression<ScriptCodeVariableReference>)
                return CreateStatement((IScriptExpression<ScriptCodeVariableReference>)variableName, constant, initExpr, contractBinding);
            else if (variableName is IScriptExpression<ScriptCodePrimitiveExpression>)
                return CreateStatement((IScriptExpression<ScriptCodePrimitiveExpression>)variableName, constant, initExpr, contractBinding);
            else return null;
        }

        protected override ScriptCodeVariableDeclaration CreateStatement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 4 ? CreateStatement(args[0], args[1] as ScriptBoolean, args[2], args[3]) : null;
        }
    }
}
