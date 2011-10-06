using System;
using System.Collections.Generic;
using System.Dynamic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using BindingRestrictions = System.Dynamic.BindingRestrictions;
    using SystemConverter = System.Convert;

    /// <summary>
    /// Represents DynamicScript literal object that supports conversion to .NET standard types.
    /// </summary>
    /// <typeparam name="TContract">Type of the contract of the primitive object.</typeparam>
    /// <typeparam name="TValue">Type of the underlying value.</typeparam>
    [ComVisible(false)]
    [CLSCompliant(false)]
    [Serializable]
    public abstract class ScriptConvertibleObject<TContract, TValue>: ScriptPrimitiveObject<TContract, TValue>, IConvertible
        where TContract: ScriptBuiltinContract
        where TValue: IConvertible, IEquatable<TValue>, IComparable<TValue>
    {
        internal ScriptConvertibleObject(TContract contractBinding, TValue value, IEqualityComparer<TValue> comparer = null)
            : base(contractBinding, value, comparer)
        {

        }

        #region IConvertible Members

        TypeCode IConvertible.GetTypeCode()
        {
            return Type.GetTypeCode(UnderlyingType);
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return SystemConverter.ToBoolean(Value, provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return SystemConverter.ToByte(Value, provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return SystemConverter.ToChar(Value, provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return SystemConverter.ToDateTime(Value, provider);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return SystemConverter.ToDecimal(Value, provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return SystemConverter.ToDouble(Value, provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return SystemConverter.ToInt16(Value, provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return SystemConverter.ToInt32(Value, provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return SystemConverter.ToInt64(Value, provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return SystemConverter.ToSByte(Value, provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return SystemConverter.ToSingle(Value, provider);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return SystemConverter.ToString(Value, provider);
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return SystemConverter.ChangeType(Value, conversionType, provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return SystemConverter.ToUInt16(Value, provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return SystemConverter.ToUInt32(Value, provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return SystemConverter.ToUInt64(Value, provider);
        }

        #endregion

        /// <summary>
        /// Provides implementation for type conversion operations.
        /// </summary>
        /// <param name="binder">Provides information about the conversion operation.</param>
        /// <param name="result">The result of the type conversion operation.</param>
        /// <returns><see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.</returns>
        public sealed override bool TryConvert(ConvertBinder binder, out object result)
        {
            var tc = default(TypeCode);
            switch (tc = Type.GetTypeCode(binder.ReturnType))
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return base.TryConvert(binder, out result);
                default:
                    result = SystemConverter.ChangeType(Value, tc);
                    return true;
            }
        }

        /// <summary>
        /// Converts the specified object to the wrapped .NET convertible object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected static TValue Convert(IScriptObject obj)
        {
            return (TValue)SystemConverter.ChangeType(obj, typeof(TValue));
        }

        /// <summary>
        /// Returns string representation of the object.
        /// </summary>
        /// <returns></returns>
        public sealed override string ToString()
        {
            return SystemConverter.ToString(Value);
        }

        internal override ScriptObject Intern(InterpreterState state)
        {
            state.Intern(this);
            return this;
        }
    }
}
