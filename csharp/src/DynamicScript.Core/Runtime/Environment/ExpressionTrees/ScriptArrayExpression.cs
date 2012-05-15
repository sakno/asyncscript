using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Enumerable = System.Linq.Enumerable;
    
    [ComVisible(false)]
    [Serializable]
    sealed class ScriptArrayExpression: ScriptExpression<ScriptCodeArrayExpression, IScriptArray>
    {
        private ScriptArrayExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptArrayExpression(ScriptCodeArrayExpression expression)
            : base(expression, ScriptArrayExpressionFactory.Instance)
        {
        }

        public override IScriptArray Compile(InterpreterState state)
        {
            var elements = new ScriptCodeExpression[Expression.Elements.Count];
            Expression.Elements.CopyTo(elements, 0);
            return new ScriptArray(ScriptExpressionFactory.Compile(elements, state));
        }

        public static ScriptCodeArrayExpression CreateExpression(IEnumerable<IScriptObject> args)
        {
            var result = new ScriptCodeArrayExpression();
            foreach (var a in args ?? Enumerable.Empty<IScriptObject>())
                if (a is IScriptExpression<ScriptCodeObjectExpression>)
                    result.Elements.Add(((IScriptExpression<ScriptCodeObjectExpression>)a).CodeObject);
                else result.Elements.Add(ScriptConstantExpression.CreateExpression(a) ?? ScriptCodeVoidExpression.Instance);
            return result;
        }

        protected override ScriptCodeArrayExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return CreateExpression(null);
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject>);
                default: return null;
            }
        }
    }
}
