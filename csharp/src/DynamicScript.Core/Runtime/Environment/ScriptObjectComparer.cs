using System;
using System.Linq.Expressions;
using System.ComponentModel;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using MethodInfo = System.Reflection.MethodInfo;
    using QCodeBinaryOperatorType = Compiler.Ast.ScriptCodeBinaryOperatorType;

    /// <summary>
    /// Represents an object that is used as selection source in 'caseof' expression.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ScriptObjectComparer: IEquatable<IScriptObject>
    {
        private readonly IScriptObject m_source;
        private readonly IScriptObject m_comparer;
        private readonly InterpreterState m_state;
        
        private ScriptObjectComparer(IScriptObject source, IScriptObject comparer, InterpreterState state)
        {
            m_source = source ?? ScriptObject.Void;
            m_comparer = comparer;
            m_state = state ?? InterpreterState.Current;
        }

        /// <summary>
        /// Creates a new comparable object.
        /// </summary>
        /// <param name="source">The object to be compared.</param>
        /// <param name="comparer">The object that implements comparison.</param>
        /// <param name="state">Internal interpreter state.</param>
        public static IEquatable<IScriptObject> Create(IScriptObject source, IScriptObject comparer, InterpreterState state)
        {
            return new ScriptObjectComparer(source, comparer, state);
        }

        /// <summary>
        /// Determines whether the first object is equal to another.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns><see langword="true"/> if the first object is equal to another; otherwise, <see langword="false"/>.</returns>
        public static bool Equals(IEquatable<IScriptObject> left, IScriptObject right)
        {
            if (left == null) throw new ArgumentNullException("left");
            return left.Equals(right);
        }

        internal static MethodCallExpression Bind(Expression source, Expression comparer, ParameterExpression stateVar)
        {
            var ctor = LinqHelpers.BodyOf<IScriptObject, IScriptObject, InterpreterState, IEquatable<IScriptObject>, MethodCallExpression>((src, cmp, s) => Create(src, cmp, s));
            return ctor.Update(null, new Expression[] { source, comparer ?? Expression.Default(typeof(IScriptObject)), stateVar });
        }

        internal static MethodInfo EqualsMethod
        {
            get { return LinqHelpers.BodyOf<IEquatable<IScriptObject>, IScriptObject, bool, MethodCallExpression>((left, right) => Equals(left, right)).Method; }
        }

        /// <summary>
        /// Determines whether the object stored in the current container is equal to another.
        /// </summary>
        /// <param name="other">The other object to compare.</param>
        /// <returns><see langword="true"/> if the stored object is equal to another; otherwise, <see langword="false"/>.</returns>
        public bool Equals(IScriptObject other)
        {
            if (other == null) other = ScriptObject.Void;
            return RuntimeHelpers.IsTrue(
                m_comparer != null ? m_comparer.Invoke(new[] { m_source, other }, m_state) : m_source.BinaryOperation(QCodeBinaryOperatorType.ValueEquality, other, m_state),
                m_state);
        }

        /// <summary>
        /// Determines whether the object stored in the current container is equal to another.
        /// </summary>
        /// <param name="other">The other object to compare.</param>
        /// <returns><see langword="true"/> if the stored object is equal to another; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object other)
        {
            return Equals(other as IScriptObject);
        }

        /// <summary>
        /// Computes the hash code of the stored object.
        /// </summary>
        /// <returns>The hash code of the stored object.</returns>
        public override int GetHashCode()
        {
            return m_source.GetHashCode();
        }

        /// <summary>
        /// Returns string representation of the stored object.
        /// </summary>
        /// <returns>The string representation of the stored object.</returns>
        public override string ToString()
        {
            return m_source.ToString();
        }
    }
}
