using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents string literal expression.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeStringExpression : ScriptCodePrimitiveExpression, 
        IStaticContractBinding<ScriptCodeStringContractExpression>, 
        ILiteralExpression<ScriptCodeStringContractExpression>
    {
        internal ScriptCodeStringExpression(StringLiteral token)
        {
            Value = token;
        }

        /// <summary>
        /// Initializes a new string literal expression.
        /// </summary>
        /// <param name="value">The value of the literal.</param>
        public ScriptCodeStringExpression(string value)
            : this(new StringLiteral(value))
        {
        }

        internal ScriptCodeStringExpression Intern()
        {
            return new ScriptCodeStringExpression(Value) { IsInterned = true };
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
        /// Gets or sets string value.
        /// </summary>
        public new string Value
        {
            get { return (StringLiteral)base.Value; }
            set { base.Value = new StringLiteral(value); }
        }

        /// <summary>
        /// Gets static contract binding.
        /// </summary>
        public ScriptCodeStringContractExpression Contract
        {
            get { return ScriptCodeStringContractExpression.Instance; }
        }

        IConvertible ILiteralExpression<ScriptCodeStringContractExpression>.Value
        {
            get { return Value; }
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<string, ScriptCodeStringExpression, NewExpression>(value => new ScriptCodeStringExpression(value));
            return ctor.Update(new[] { LinqHelpers.Constant(Value) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeStringExpression(Value);
        }
    }
}
