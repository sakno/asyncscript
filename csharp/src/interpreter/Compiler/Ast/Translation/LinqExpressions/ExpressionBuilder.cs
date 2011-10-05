using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IScriptObject = Runtime.IScriptObject;

    /// <summary>
    /// Represents LINQ expression builder.
    /// </summary>
    [ComVisible(false)]
    static class ExpressionBuilder
    {
        public static void Label(this ICollection<Expression> expressions, LabelTarget target)
        {
            expressions.Add(Expression.Label(target));
        }

        public static void Label(this ICollection<Expression> expressions, LabelTarget target, Expression defaultValue)
        {
            expressions.Add(Expression.Label(target, defaultValue));
        }

        public static void Goto(this ICollection<Expression> expressions, LabelTarget endOfScope, Expression value, GotoExpressionKind exitKind)
        {
            expressions.Add(Expression.MakeGoto(exitKind, endOfScope, value, value.Type));
        }
    }
}
