using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;
    using CodeExpressionCollection = System.CodeDom.CodeExpressionCollection;

    /// <summary>
    /// Represents runtime representation of 
    /// </summary>
    /// <typeparam name="TExpression">Type of the expression tree.</typeparam>
    /// <typeparam name="TOutput">Type of the compiled expression.</typeparam>
    [ComVisible(false)]
    [Serializable]
    abstract class ScriptExpression<TExpression, TOutput> : ScriptObject, IScriptExpression<TExpression>, ISerializable
        where TExpression : ScriptCodeExpression
        where TOutput : class, IScriptObject
    {
        #region Nested Types
        /// <summary>
        /// Represents an abstract expression converter.
        /// </summary>
        [ComVisible(false)]
        public abstract class ExpressionConverter : RuntimeConverter<TExpression>
        {
        }
        #endregion
        private const string ExpressionHolder = "Expression";
        private const string ContractHolder = "ContractBinding";

        /// <summary>
        /// Represents expression tree.
        /// </summary>
        private TExpression m_expression;
        public readonly IScriptExpressionContract<TExpression> ContractBinding;

        /// <summary>
        /// Deserializes a runtime representation of expression.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ScriptExpression(SerializationInfo info, StreamingContext context)
            : this(info.GetValue<TExpression>(ExpressionHolder), info.GetValue<IScriptExpressionContract<TExpression>>(ContractHolder))
        {
        }

        /// <summary>
        /// Initializes a new runtime representation of the expression.
        /// </summary>
        /// <param name="expression">An expression tree. Cannot be <see langword="null"/>.</param>
        /// <param name="contractBinding">An underlying contract binding for this object.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="expression"/> or <paramref name="contractBinding"/> is <see langword="null"/>.</exception>
        protected ScriptExpression(TExpression expression, IScriptExpressionContract<TExpression> contractBinding)
            : base(RuntimeBehavior.UnwrapSlotValue)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            if (contractBinding == null) throw new ArgumentNullException("contractBinding");
            m_expression = expression;
            ContractBinding = contractBinding;
        }

        /// <summary>
        /// Returns an underlying contract of this object.
        /// </summary>
        /// <returns></returns>
        public sealed override IScriptContract GetContractBinding()
        {
            return ContractBinding;
        }

        TExpression IScriptCodeElement<TExpression>.CodeObject
        {
            get { return Expression; }
        }

        /// <summary>
        /// Gets or sets expression tree.
        /// </summary>
        public TExpression Expression
        {
            get { return m_expression; }
            set 
            {
                if (value == null) throw new ArgumentNullException("value");
                m_expression = value;
            }
        }

        /// <summary>
        /// Compiles an expression tree.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>Compiled expression tree.</returns>
        public abstract TOutput Compile(InterpreterState state);

        IScriptObject IScriptExpression<TExpression>.Compile(InterpreterState state)
        {
            return Compile(state);
        }

        /// <summary>
        /// Constructs a new runtime representation of the unary expression.
        /// </summary>
        /// <param name="operator">Unary operator type.</param>
        /// <returns>A new runtime representation of the unary expression.</returns>
        protected IScriptObject UnaryOperation(ScriptCodeUnaryOperatorType @operator)
        {
            return Convert(new ScriptCodeUnaryOperatorExpression(@operator, m_expression));
        }

        /// <summary>
        /// Constructs a new runtime representation of the unary minus.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the unary minus.</returns>
        protected sealed override IScriptObject UnaryMinus(InterpreterState state)
        {
            return UnaryOperation(ScriptCodeUnaryOperatorType.Minus);
        }

        /// <summary>
        /// Constructs a new runtime representation of the unary plus.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the unary plus.</returns>
        protected sealed override IScriptObject UnaryPlus(InterpreterState state)
        {
            return UnaryOperation(ScriptCodeUnaryOperatorType.Plus);
        }

        /// <summary>
        /// Constructs a new runtime representation of the intern expression.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the intern expression.</returns>
        internal sealed override ScriptObject Intern(InterpreterState state)
        {
            return UnaryOperation(ScriptCodeUnaryOperatorType.Intern) as ScriptObject ?? Void;
        }

        /// <summary>
        /// Constructs a new runtime representation of the negation expression.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the negation expression.</returns>
        protected sealed override IScriptObject Not(InterpreterState state)
        {
            return UnaryOperation(ScriptCodeUnaryOperatorType.Negate);
        }

        /// <summary>
        /// Constructs a new runtime representation of the prefixed decrement expression.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the prefixed decrement expression.</returns>
        protected sealed override IScriptObject PreDecrementAssign(InterpreterState state)
        {
            return UnaryOperation(ScriptCodeUnaryOperatorType.DecrementPrefix);
        }

        /// <summary>
        /// Constructs a new runtime representation of the postfixed decrement expression.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the decrement expression.</returns>
        protected sealed override IScriptObject PostDecrementAssign(InterpreterState state)
        {
            return UnaryOperation(ScriptCodeUnaryOperatorType.DecrementPostfix);
        }

        /// <summary>
        /// Constructs a new runtime representation of the prefixed increment expression.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the prefixed increment expression.</returns>
        protected sealed override IScriptObject PreIncrementAssign(InterpreterState state)
        {
            return UnaryOperation(ScriptCodeUnaryOperatorType.IncrementPrefix);
        }

        /// <summary>
        /// Constructs a new runtime representation of the postfixed increment expression.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the postfixed increment expression.</returns>
        protected sealed override IScriptObject PostIncrementAssign(InterpreterState state)
        {
            return UnaryOperation(ScriptCodeUnaryOperatorType.IncrementPostfix);
        }

        /// <summary>
        /// Constructs a new runtime representation of the void check expression.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the void check  expression.</returns>
        protected sealed override IScriptObject IsVoid(InterpreterState state)
        {
            return UnaryOperation(ScriptCodeUnaryOperatorType.VoidCheck);
        }

        /// <summary>
        /// Constructs a new runtime representation of the postfixed square expression.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the postfixed square expression.</returns>
        protected sealed override IScriptObject PostSquareAssign(InterpreterState state)
        {
            return UnaryOperation(ScriptCodeUnaryOperatorType.SquarePostfix);
        }

        /// <summary>
        /// Constructs a new runtime representation of the prefixed square expression.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the prefixed square expression.</returns>
        protected sealed override IScriptObject PreSquareAssign(InterpreterState state)
        {
            return UnaryOperation(ScriptCodeUnaryOperatorType.SquarePrefix);
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary expression.
        /// </summary>
        /// <param name="operator">Binary operator type.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>A new runtime representation of the binary expression.</returns>
        protected IScriptObject BinaryOperation(ScriptCodeBinaryOperatorType @operator, IScriptExpression<ScriptCodeExpression> right)
        {
            return Convert(new ScriptCodeBinaryOperatorExpression(m_expression, @operator, right.CodeObject));
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary expression.
        /// </summary>
        /// <param name="operator">Binary operator type.</param>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the binary expression.</returns>
        protected new IScriptObject BinaryOperation(ScriptCodeBinaryOperatorType @operator, IScriptObject right, InterpreterState state)
        {
            switch (right is IScriptExpression<ScriptCodeExpression>)
            {
                case true: return BinaryOperation(@operator, (IScriptExpression<ScriptCodeExpression>)right);
                default:
                    var primitive = CreatePrimitiveExpression(right);
                    if (primitive != null)
                        return BinaryOperation(@operator, Convert(primitive), state);
                    else if (state.Context == InterpretationContext.Unchecked)
                        return Void;
                    else throw new UnsupportedOperationException(state);
            }
        }

        protected static ScriptCodePrimitiveExpression CreatePrimitiveExpression(IScriptObject value)
        {
            if (value is ScriptStringContract)
                return ScriptCodeStringContractExpression.Instance;
            else if (value is ScriptString)
                return new ScriptCodeStringExpression((ScriptString)value);
            else if (value is ScriptIntegerContract)
                return ScriptCodeIntegerContractExpression.Instance;
            else if (value is ScriptInteger)
                return new ScriptCodeIntegerExpression((ScriptInteger)value);
            else if (value is ScriptVoid)
                return ScriptCodeVoidExpression.Instance;
            else if (value is ScriptBooleanContract)
                return ScriptCodeBooleanContractExpression.Instance;
            else if (value is ScriptBoolean)
                return new ScriptCodeBooleanExpression((ScriptBoolean)value);
            else if (value is ScriptReal)
                return new ScriptCodeRealExpression((ScriptReal)value);
            else if (value is ScriptRealContract)
                return ScriptCodeRealContractExpression.Instance;
            else if (value is ScriptCallableContract)
                return ScriptCodeCallableContractExpression.Instance;
            else if (value is ScriptDimensionalContract)
                return ScriptCodeDimensionalContractExpression.Instance;
            else if (value is ScriptSuperContract)
                return ScriptCodeSuperContractExpression.Instance;
            else if (value is ScriptMetaContract)
                return ScriptCodeMetaContractExpression.Instance;
            else if (value is ScriptFinSetContract)
                return ScriptCodeFinSetContractExpression.Instance;
            else if (value is ScriptExpressionFactory)
                return ScriptCodeExpressionContractExpression.Instance;
            else if (value is ScriptStatementFactory)
                return ScriptCodeStatementContractExpression.Instance;
            else if (value is IScriptExpression<ScriptCodePrimitiveExpression>)
                return ((IScriptExpression<ScriptCodePrimitiveExpression>)value).CodeObject;
            else return null;
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary addition operator.
        /// </summary>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the binary addition operator.</returns>
        protected sealed override IScriptObject Add(IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(ScriptCodeBinaryOperatorType.Add, right, state);
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary intersection operator.
        /// </summary>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the binary intersection operator.</returns>
        protected sealed override IScriptObject And(IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(ScriptCodeBinaryOperatorType.Intersection, right, state);
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary coalescing operator.
        /// </summary>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the binary coalescing operator.</returns>
        protected sealed override IScriptObject Coalesce(IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(ScriptCodeBinaryOperatorType.Coalesce, right, state);
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary division operator.
        /// </summary>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the binary division operator.</returns>
        protected sealed override IScriptObject Divide(IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(ScriptCodeBinaryOperatorType.Divide, right, state);
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary exlusion operator.
        /// </summary>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the binary exlusion operator.</returns>
        protected sealed override IScriptObject ExclusiveOr(IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(ScriptCodeBinaryOperatorType.Exclusion, right, state);
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary subtraction operator.
        /// </summary>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the binary subtraction operator.</returns>
        protected sealed override IScriptObject Subtract(IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(ScriptCodeBinaryOperatorType.Subtract, right, state);
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary multiplication operator.
        /// </summary>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the binary multiplication operator.</returns>
        protected sealed override IScriptObject Multiply(IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(ScriptCodeBinaryOperatorType.Multiply, right, state);
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary expansion operator.
        /// </summary>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the binary expansion operator.</returns>
        protected sealed override IScriptObject Or(IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(ScriptCodeBinaryOperatorType.Union, right, state);
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary equality operator.
        /// </summary>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the binary equality operator.</returns>
        protected sealed override IScriptObject Equals(IScriptObject right, InterpreterState state)
        {
            return right is IScriptExpression<ScriptCodeExpression> ?
                BinaryOperation(ScriptCodeBinaryOperatorType.ValueEquality, (IScriptExpression<ScriptCodeExpression>)right) :
                ScriptBoolean.False;
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary inequality operator.
        /// </summary>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the binary inequality operator.</returns>
        protected sealed override IScriptObject NotEquals(IScriptObject right, InterpreterState state)
        {
            return right is IScriptExpression<ScriptCodeExpression> ?
                BinaryOperation(ScriptCodeBinaryOperatorType.ValueInequality, (IScriptExpression<ScriptCodeExpression>)right) :
                ScriptBoolean.True;
        }

        /// <summary>
        /// Constructs a new runtime represnetation of the slot discovery operation.
        /// </summary>
        /// <param name="slotName">The name of the slot.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime represnetation of the slot discovery operation.</returns>
        protected sealed override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return BinaryOperation(ScriptCodeBinaryOperatorType.MetadataDiscovery, Convert(new ScriptCodeVariableReference { VariableName = slotName }) as IScriptExpression<ScriptCodeVariableReference>);
        }

        /// <summary>
        /// Constructs a new runtime representation of the access operation.
        /// </summary>
        /// <param name="slotName">The name of the slot.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        /// <remarks>Result of this index will be unwrapped automatically because <see cref="ScriptObject.Behavior"/> is overridden in this class.</remarks>
        public sealed override IRuntimeSlot this[string slotName, InterpreterState state]
        {
            get
            {
                return new ScriptConstant(BinaryOperation(ScriptCodeBinaryOperatorType.MemberAccess, Convert(new ScriptCodeVariableReference { VariableName = slotName }) as IScriptExpression<ScriptCodeVariableReference>));
            }
        }

        /// <summary>
        /// Constructs a new runtime representation of the instance checking operation.
        /// </summary>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the instance checking operation.</returns>
        internal sealed override IScriptObject InstanceOf(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptContract)
                return InstanceOf((IScriptContract)right, state);
            else if (right is IScriptExpression<ScriptCodeExpression>)
                return BinaryOperation(ScriptCodeBinaryOperatorType.InstanceOf, (IScriptExpression<ScriptCodeExpression>)right);
            else return ScriptBoolean.False;
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary reference equality operator.
        /// </summary>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the binary reference equality operator.</returns>
        protected sealed override IScriptObject ReferenceEquals(IScriptObject right, InterpreterState state)
        {
            return right is IScriptExpression<ScriptCodeExpression> ?
                BinaryOperation(ScriptCodeBinaryOperatorType.ReferenceEquality, (IScriptExpression<ScriptCodeExpression>)right) :
                ScriptBoolean.False;
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary reference equality operator.
        /// </summary>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the binary reference equality operator.</returns>
        protected sealed override IScriptObject ReferenceNotEquals(IScriptObject right, InterpreterState state)
        {
            return right is IScriptExpression<ScriptCodeExpression> ?
                BinaryOperation(ScriptCodeBinaryOperatorType.ReferenceInequality, (IScriptExpression<ScriptCodeExpression>)right) :
                ScriptBoolean.True;
        }

        /// <summary>
        /// Gets empty collection of runtime slots.
        /// </summary>
        public sealed override ICollection<string> Slots
        {
            get
            {
                return new string[0];
            }
        }

        /// <summary>
        /// Constructs a new runtime representation of the binary modulo operator.
        /// </summary>
        /// <param name="right">Right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the binary modulo operator.</returns>
        protected sealed override IScriptObject Modulo(IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(ScriptCodeBinaryOperatorType.Modulo, right, state);
        }

        protected sealed override IScriptObject LessThanOrEqual(IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(ScriptCodeBinaryOperatorType.LessThanOrEqual, right, state);
        }

        protected sealed override IScriptObject LessThan(IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(ScriptCodeBinaryOperatorType.LessThan, right, state);
        }

        protected sealed override IScriptObject GreaterThan(IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(ScriptCodeBinaryOperatorType.GreaterThan, right, state);
        }

        protected sealed override IScriptObject GreaterThanOrEqual(IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(ScriptCodeBinaryOperatorType.GreaterThanOrEqual, right, state);
        }

        private IScriptObject Application(ScriptCodeExpressionCollection args)
        {
            var invocation = new ScriptCodeInvocationExpression();
            invocation.Target = Expression;
            invocation.ArgList.AddRange(args);
            return Convert(invocation);
        }

        private IScriptObject Indexing(ScriptCodeExpression[] args)
        {
            var indexing = new ScriptCodeIndexerExpression();
            indexing.Target = Expression;
            indexing.ArgList.AddRange(args);
            return Convert(indexing);
        }

        /// <summary>
        /// Constructs a new runtime representation of the invocation expression.
        /// </summary>
        /// <param name="args">Invocation arguments.</param>
        /// <param name="state">Internal intepreter state.</param>
        /// <returns></returns>
        public sealed override IScriptObject Invoke(IList<IScriptObject> args, InterpreterState state)
        {
            var expressions = new ScriptCodeExpressionCollection();
            foreach (var a in args)
                switch (a is IScriptExpression<ScriptCodeExpression>)
                {
                    case true: expressions.Add(((IScriptExpression<ScriptCodeExpression>)a).CodeObject); continue;
                    default:
                        var primitive = CreatePrimitiveExpression(a);
                        if (primitive != null)
                            expressions.Add(primitive);
                        else throw new UnsupportedOperationException(state);
                        continue;
                }
            return Application(expressions);
        }

        /// <summary>
        /// Constructs a new runtime representation of the indexer expression.
        /// </summary>
        /// <param name="args">Indexer arguments.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime representation of the indexer expression.</returns>
        public sealed override RuntimeSlotBase this[IScriptObject[] args, InterpreterState state]
        {
            get
            {
                return new ScriptConstant(Indexing(Array.ConvertAll<IScriptObject, ScriptCodeExpression>(args, delegate(IScriptObject a)
                {
                    switch (a is IScriptExpression<ScriptCodeExpression>)
                    {
                        case true: return ((IScriptExpression<ScriptCodeExpression>)a).CodeObject;
                        default: throw new UnsupportedOperationException(state);
                    }
                })));
            }
        }

        /// <summary>
        /// Determines whether this object contains in the specified collection or constructs a new expression.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject PartOf(IScriptObject collection, InterpreterState state)
        {
            return collection is IScriptExpression<ScriptCodeExpression> ?
                BinaryOperation(ScriptCodeBinaryOperatorType.PartOf, (IScriptExpression<ScriptCodeExpression>)collection) :
                base.PartOf(collection, state);
        }

        /// <summary>
        /// Returns a string representation of this script object.
        /// </summary>
        /// <returns>A string representation of this script object.</returns>
        public sealed override string ToString()
        {
            return Expression.ToString();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue<TExpression>(ExpressionHolder, m_expression);
            info.AddValue<IScriptExpressionContract<TExpression>>(ContractHolder, ContractBinding);
        }

        /// <summary>
        /// Creates a new expression tree.
        /// </summary>
        /// <param name="args">Expression tree creation arguments.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new instance of expression tree; or <see langword="null"/> if tree cannot be created
        /// using the specified arguments.</returns>
        protected abstract TExpression CreateExpression(IList<IScriptObject> args, InterpreterState state);

        bool IScriptCodeElement<TExpression>.Modify(IList<IScriptObject> args, InterpreterState state)
        {
            var expr = CreateExpression(args, state);
            switch (expr != null && expr.Completed)
            {
                case true:
                    Expression = expr;
                    return true;
                default: return false;
            }
        }
    }
}
