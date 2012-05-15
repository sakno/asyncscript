using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodePlaceholderExpression = Compiler.Ast.ScriptCodePlaceholderExpression;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptPlaceholderExpression: ScriptExpression<ScriptCodePlaceholderExpression, IScriptObject>
    {
        private ScriptPlaceholderExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptPlaceholderExpression(ScriptCodePlaceholderExpression expression)
            : base(expression, ScriptPlaceholderExpressionFactory.Instance)
        {
        }

        public ScriptPlaceholderExpression(long id)
            : this(new ScriptCodePlaceholderExpression(id))
        {
        }

        public override IScriptObject Compile(InterpreterState state)
        {
            return Void;
        }

        public static ScriptCodePlaceholderExpression CreateExpression(ScriptInteger id)
        {
            return new ScriptCodePlaceholderExpression(id);
        }

        protected override ScriptCodePlaceholderExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateExpression(args[0] as ScriptInteger) : null;
        }
    }
}
