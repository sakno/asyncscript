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
    sealed class ScriptObjectExpression : ScriptExpression<ScriptCodeObjectExpression, IScriptCompositeObject>
    {
        private ScriptObjectExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptObjectExpression(ScriptCodeObjectExpression expression)
            : base(expression, ScriptObjectExpressionFactory.Instance)
        {
        }

        public override IScriptCompositeObject Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression, state) as IScriptCompositeObject;
        }

        public static ScriptCodeObjectExpression CreateExpression(IEnumerable<ScriptCodeVariableDeclaration> slots)
        {
            return new ScriptCodeObjectExpression(slots ?? Enumerable.Empty<ScriptCodeVariableDeclaration>());
        }

        public static ScriptCodeObjectExpression CreateExpression(IEnumerable<IScriptCodeElement<ScriptCodeVariableDeclaration>> slots)
        { 
            return CreateExpression(Enumerable.Select(slots, s => s.CodeObject));
        }

        public static ScriptCodeObjectExpression CreateExpression(IEnumerable<IScriptObject> slots)
        {
            if (slots == null) slots = Enumerable.Empty<IScriptObject>();
            return CreateExpression(from s in slots where s is IScriptCodeElement<ScriptCodeVariableDeclaration> select (IScriptCodeElement<ScriptCodeVariableDeclaration>)s);
        }

        protected override ScriptCodeObjectExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return CreateExpression(Enumerable.Empty<IScriptObject>());
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject> ?? args);
                default: return CreateExpression(args);
            }
        }
    }
}
