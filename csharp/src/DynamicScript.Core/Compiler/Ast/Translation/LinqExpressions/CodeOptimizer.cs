using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using Enumerable = System.Linq.Enumerable;

    sealed class CodeOptimizer: ICodeOptimizer
    {
        public readonly Type Lookup;

        public CodeOptimizer(Type lookupType)
        {
            if (lookupType == null) throw new ArgumentNullException("lookupType");
            Lookup = lookupType;
        }

        private static Expression Convert(Expression value, Type destinationType, ParameterExpression state)
        {
            if (Equals(destinationType, typeof(Runtime.IScriptObject)))
                return value;
            else if (Equals(destinationType, typeof(Runtime.Environment.ScriptInteger)))
                return Runtime.Environment.ScriptIntegerContract.TryConvert(value, state);
            else if (Equals(destinationType, typeof(Runtime.Environment.ScriptBoolean)))
                return Runtime.Environment.ScriptBooleanContract.TryConvert(value, state);
            else if (Equals(destinationType, typeof(Runtime.Environment.ScriptString)))
                return Runtime.Environment.ScriptStringContract.TryConvert(value, state);
            else return Expression.TypeAs(value, destinationType);
        }

        private static Expression InlineFunctionCall(MethodInfo source, IList<Expression> arguments, ParameterExpression state)
        {
            var parameters = source.GetParameters();
            if (parameters.LongLength - 1 == arguments.Count)   //-1 means that the static function includes interpreter state as the last argument
            {
                var invocationArguments = new Expression[parameters.LongLength];
                for (var i = 0; i < arguments.Count; i++)
                    invocationArguments[i] = Convert(arguments[i], parameters[i].ParameterType, state);
                invocationArguments[parameters.LongLength - 1] = state;  //interpreter state
                return Expression.Call(null, source, invocationArguments);
            }
            else return Runtime.Environment.FunctionArgumentsMistmatchException.Throw(state);
        }

        public Expression InlineFunctionCall(string functionName, IList<Expression> arguments, ParameterExpression state)
        {
            const BindingFlags MethodFlags = BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.IgnoreCase;
            var method = Lookup.GetMethod(functionName, MethodFlags);
            return method != null && InliningSourceAttribute.IsDefined(method) ? InlineFunctionCall(method, arguments, state) : null;
        }

        internal static void Add<TSource>(IDictionary<string, ICodeOptimizer> optimizers, string objectPath)
        {
            optimizers.Add(objectPath, new CodeOptimizer(typeof(TSource)));
        }
    }
}
