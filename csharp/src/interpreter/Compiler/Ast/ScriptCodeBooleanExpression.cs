using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeExpression = System.CodeDom.CodeExpression;
    using RuntimeHelpers = Runtime.Environment.RuntimeHelpers;

    /// <summary>
    /// Represents boolean literal.
    /// This class cannot be inherited.
    /// </summary>
    
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeBooleanExpression : ScriptCodePrimitiveExpression, 
        IStaticContractBinding<ScriptCodeBooleanContractExpression>,
        ILiteralExpression
    {
        /// <summary>
        /// Initializes a new boolean literal.
        /// </summary>
        /// <param name="value">The value of the boolean expression.</param>
        public ScriptCodeBooleanExpression(bool value)
        {
            Value = value;
        }

        internal ScriptCodeBooleanExpression Negate()
        {
            return new ScriptCodeBooleanExpression(!Value);
        }

        /// <summary>
        /// Gets or sets a value of the boolean expression.
        /// </summary>
        public new bool Value
        {
            get { return bool.Parse(base.Value); }
            set { base.Value = value ? Keyword.True : Keyword.False; }
        }

        /// <summary>
        /// Gets static contract binding.
        /// </summary>
        public ScriptCodeBooleanContractExpression Contract
        {
            get { return ScriptCodeBooleanContractExpression.Instance; }
        }

        IConvertible ILiteralExpression.Value
        {
            get { return Value; }
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<bool, ScriptCodeBooleanExpression, NewExpression>(value => new ScriptCodeBooleanExpression(value));
            return ctor.Update(new[] { LinqHelpers.Constant(Value) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeBooleanExpression(Value);
        }
    }
}
