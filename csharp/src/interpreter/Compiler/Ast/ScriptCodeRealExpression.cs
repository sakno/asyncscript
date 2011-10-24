using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents floating-point number expression.
    /// This class cannot be inherited.
    /// </summary>
    
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeRealExpression: ScriptCodePrimitiveExpression, 
        IStaticContractBinding<ScriptCodeRealContractExpression>,
        ILiteralExpression
    {
        internal ScriptCodeRealExpression(RealLiteral token)
        {
            Value = token;
        }

        /// <summary>
        /// Initializes a new floating-point number expression.
        /// </summary>
        /// <param name="value">The floating-point number expression.</param>
        public ScriptCodeRealExpression(double value)
            : this(new RealLiteral(value))
        {
        }

        internal ScriptCodeRealExpression Minus()
        {
            return new ScriptCodeRealExpression(-Value);
        }

        internal ScriptCodeRealExpression Intern()
        {
            return new ScriptCodeRealExpression(Value) { IsInterned = true };
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
        /// Gets or sets value of the expression.
        /// </summary>
        public new double Value
        {
            get { return (RealLiteral)base.Value; }
            set { base.Value = new RealLiteral(value); }
        }

        /// <summary>
        /// Gets static contract binding.
        /// </summary>
        public ScriptCodeRealContractExpression Contract
        {
            get { return ScriptCodeRealContractExpression.Instance; }
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
            var ctor = LinqHelpers.BodyOf<double, ScriptCodeRealExpression, NewExpression>(value => new ScriptCodeRealExpression(value));
            return ctor.Update(new[] { LinqHelpers.Constant(Value) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeRealExpression(Value);
        }

        internal ScriptCodeRealExpression Square()
        {
            return new ScriptCodeRealExpression(Value * Value);
        }

        internal ScriptCodeRealExpression Decrement()
        {
            return new ScriptCodeRealExpression(Value - 1);
        }

        internal ScriptCodeRealExpression Increment()
        {
            return new ScriptCodeRealExpression(Value + 1);
        }
    }
}
