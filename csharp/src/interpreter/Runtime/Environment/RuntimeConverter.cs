using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an abstract class for DynamicScript runtime converters.
    /// </summary>
    /// <typeparam name="TInput">Type of the input value to be converted.</typeparam>
    [ComVisible(false)]
    public abstract class RuntimeConverter<TInput> : IRuntimeConverter
    {
        /// <summary>
        /// Initializes a new instance of the converter.
        /// </summary>
        protected RuntimeConverter()
        {
        }

        /// <summary>
        /// Converts the specified value to the DynamicScript-compliant representation.
        /// </summary>
        /// <param name="input">The object to be converted.</param>
        /// <param name="result">Conversion result.</param>
        /// <returns><see langword="true"/> if conversion is possible; otherwise, <see langword="false"/>.</returns>
        public abstract bool Convert(TInput input, out IScriptObject result);

        #region IRuntimeConverter Members

        bool IRuntimeConverter.Convert(object value, out IScriptObject result)
        {
            switch (value is TInput)
            {
                case true:
                    return Convert((TInput)value, out result);
                default:
                    result = null;
                    return false;
            }
        }

        #endregion

        #region IEquatable<IRuntimeConverter> Members

        /// <summary>
        /// Determines whether the specified converter is the same as current.
        /// </summary>
        /// <param name="other">Other converter to compare.</param>
        /// <returns><see langword="true"/> if the specified converter is the same as current; otherwise, <see langword="false"/>.</returns>
        public virtual bool Equals(IRuntimeConverter other)
        {
            return other is RuntimeConverter<TInput>;
        }

        #endregion

        /// <summary>
        /// Determines whether the specified converter is the same as current.
        /// </summary>
        /// <param name="other">Other converter to compare.</param>
        /// <returns><see langword="true"/> if the specified converter is the same as current; otherwise, <see langword="false"/>.</returns>
        public sealed override bool Equals(object other)
        {
            return Equals(other as IRuntimeConverter);
        }

        /// <summary>
        /// Computes a hash code for the current converter.
        /// </summary>
        /// <returns>The hash code for the current converter.</returns>
        public override int GetHashCode()
        {
            return typeof(TInput).MetadataToken;
        }
    }
}
