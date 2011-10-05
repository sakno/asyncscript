using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptActionContractExpression : ScriptExpression<ScriptCodeActionContractExpression, IScriptContract>
    {
        private ScriptActionContractExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptActionContractExpression(ScriptCodeActionContractExpression expression)
            : base(expression, ScriptActionContractExpressionFactory.Instance)
        {
        }

        public override IScriptContract Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression, state) as IScriptContract;
        }

        public static ScriptCodeActionContractExpression CreateExpression(IEnumerable<ScriptCodeVariableDeclaration> parameters, ScriptCodeExpression returnType)
        {
            if (parameters == null) parameters = Enumerable.Empty<ScriptCodeVariableDeclaration>();
            var result = new ScriptCodeActionContractExpression { ReturnType = returnType };
            foreach (var p in parameters)
                result.ParamList.Add(new ScriptCodeActionContractExpression.Parameter(p));
            return result;
        }

        public static ScriptCodeActionContractExpression CreateExpression(IEnumerable<IScriptObject> parameters, IScriptCodeElement<ScriptCodeExpression> returnType)
        {
            return CreateExpression(from p in parameters where p is IScriptCodeElement<ScriptCodeVariableDeclaration> select ((IScriptCodeElement<ScriptCodeVariableDeclaration>)p).CodeObject, returnType != null ? returnType.CodeObject : null);
        }

        protected override ScriptCodeActionContractExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 2 ? CreateExpression(args[0] as IEnumerable<IScriptObject>, args[1] as IScriptCodeElement<ScriptCodeExpression>) : null;
        }
    }
}
