using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using RuntimeHelpers = Runtime.Environment.RuntimeHelpers;

    /// <summary>
    /// Represents integer literal expression.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeIntegerExpression : ScriptCodePrimitiveExpression, 
        IStaticContractBinding<ScriptCodeIntegerContractExpression>, 
        ILiteralExpression
    {
        private ScriptCodeIntegerExpression(IntegerLiteral token)
        {
            Value = token;
        }

        /// <summary>
        /// Initializes a new integer literal expression.
        /// </summary>
        /// <param name="value">The literal value.</param>
        public ScriptCodeIntegerExpression(long value)
            : this(new IntegerLiteral(value))
        {
        }

        internal ScriptCodeIntegerExpression Negate()
        {
            return new ScriptCodeIntegerExpression(~Value);
        }

        internal ScriptCodeIntegerExpression Minus()
        {
            return new ScriptCodeIntegerExpression(-Value);
        }

        internal ScriptCodeIntegerExpression Intern()
        {
            return new ScriptCodeIntegerExpression(Value) { IsInterned = true };
        }

        /// <summary>
        /// Gets a value indicating that this integer literal should be interned.
        /// </summary>
        public override bool IsInterned
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets integer literal.
        /// </summary>
        public new long Value
        {
            get { return (IntegerLiteral)base.Value; }
            set { base.Value = new IntegerLiteral(value); }
        }

        /// <summary>
        /// Converts literal expression to .NET-compliant type.
        /// </summary>
        /// <param name="expression">The expression to be converted.</param>
        /// <returns>A <see cref="System.Int64"/> value that represents DynamicScript integer literal.</returns>
        public static implicit operator long(ScriptCodeIntegerExpression expression)
        {
            return expression != null ? expression.Value : 0L;
        }

        /// <summary>
        /// Gets static contract binding.
        /// </summary>
        public ScriptCodeIntegerContractExpression Contract
        {
            get { return ScriptCodeIntegerContractExpression.Instance; }
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
            var ctor = LinqHelpers.BodyOf<long, ScriptCodeIntegerExpression, NewExpression>(value => new ScriptCodeIntegerExpression(value));
            return ctor.Update(new[]{LinqHelpers.Constant(Value)});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeIntegerExpression(Value);
        }
    }
}
