using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;

    /// <summary>
    /// Represents DynamicScript literal object.
    /// </summary>
    /// <typeparam name="TContract">Type of the contract of the primitive object.</typeparam>
    /// <typeparam name="TValue">Type of the underlying value.</typeparam>
    [ComVisible(false)]
    [Serializable]
    [ScriptPrimitiveObjectConverter]
    public abstract class ScriptPrimitiveObject<TContract, TValue> : ScriptObjectWithStaticBinding<TContract>, IEquatable<TValue>, ISerializable
        where TContract : ScriptBuiltinContract
    {
        private const string UnderlyingValueSerializationSlot = "Value";
        private readonly IEqualityComparer<TValue> m_comparer;

        /// <summary>
        /// Represents underlying value of the primitive object.
        /// </summary>
        public readonly TValue Value;

        internal ScriptPrimitiveObject(TContract contractBinding, TValue value, IEqualityComparer<TValue> comparer = null)
            : base(contractBinding)
        {
            m_comparer = comparer ?? EqualityComparer<TValue>.Default;
            Value = value;
        }

        private IEqualityComparer<TValue> Comparer
        {
            get { return m_comparer; }
        }

        /// <summary>
        /// Gets underlying .NET type.
        /// </summary>
        public static Type UnderlyingType
        {
            get { return typeof(TValue); }
        }

        /// <summary>
        /// Returns an expression that represents access to the underlying .NET value.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static Expression UnderlyingValue(Expression obj)
        {
            var field = LinqHelpers.BodyOf<ScriptPrimitiveObject<TContract, TValue>, TValue, MemberExpression>(v => v.Value);
            return field.Update(obj);
        }

        #region IEquatable<TValue> Members

        /// <summary>
        /// Determines whether the specified object is the same as the stored underlying value.
        /// </summary>
        /// <param name="other">The object to compare.</param>
        /// <returns><see langword="true"/> if the specified object is the same as the stored underlying value; otherwise, <see langword="false"/>.</returns>
        public bool Equals(TValue other)
        {
            return Comparer.Equals(Value, other);
        }

        #endregion

        /// <summary>
        /// Returns a hash code for the underlying value.
        /// </summary>
        /// <returns>The hash code for the underlying value.</returns>
        public sealed override int GetHashCode()
        {
            return Comparer != null ? Comparer.GetHashCode(Value) : GetHashCode(Value);
        }

        private static int GetHashCode(TValue value)
        {
            return value != null ? value.GetHashCode() : 0;
        }

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(UnderlyingValueSerializationSlot, Value, typeof(TValue));
        }

        #endregion

        /// <summary>
        /// Returns the current object.
        /// </summary>
        /// <param name="args">An array of invocation arguments. Must be an empty.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The current object without changes.</returns>
        public sealed override IScriptObject Invoke(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return this;
                default: if (state.Context == InterpretationContext.Unchecked) return null;
                    else throw new FunctionArgumentsMistmatchException(state);
            }
        }

        /// <summary>
        /// Creates clone of the current primitive immutable object.
        /// </summary>
        /// <returns>The clone of the current primitive immutable object.</returns>
        protected sealed override ScriptObject Clone()
        {
            return this;
        }

        /// <summary>
        /// Restores underlying value from the serialization object graph.
        /// </summary>
        /// <param name="info">Serialization object graph.</param>
        /// <returns>Restored underlying value.</returns>
        protected static TValue Deserialize(SerializationInfo info)
        {
            return (TValue)info.GetValue(UnderlyingValueSerializationSlot, typeof(TValue));
        }

        /// <summary>
        /// Provides implicit conversion fro DynamicScript-specific form to the underlying system value.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static implicit operator TValue(ScriptPrimitiveObject<TContract, TValue> obj)
        {
            return obj != null ? obj.Value : default(TValue);
        }
    }
}
