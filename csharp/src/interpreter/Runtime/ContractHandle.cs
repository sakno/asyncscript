﻿using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

#if USE_REL_MATRIX
    /// <summary>
    /// Represents a handle that uniquely identifies the script contract at runtime.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public struct ContractHandle: IEquatable<ContractHandle>
    {
        private readonly long Value;

        private ContractHandle(Type contractType, int hashCode)
        {
            Value = contractType.MetadataToken << 32 | hashCode;
        }

        /// <summary>
        /// Determines whether this contract handle is equal to the specified handle.
        /// </summary>
        /// <param name="other">Other handle to compare.</param>
        /// <returns></returns>
        public bool Equals(ContractHandle other)
        {
            return Value == other.Value;
        }

        /// <summary>
        /// Determines whether this contract handle is equal to the specified handle.
        /// </summary>
        /// <param name="other">Other handle to compare.</param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            return other is ContractHandle && Equals((ContractHandle)other);
        }

        /// <summary>
        /// Returns a string representation of this handle.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// Computes a hash code for this handle.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Generates a new contract handle.
        /// </summary>
        /// <returns>A newly generated handle.</returns>
        public static ContractHandle New<TContract>(TContract c)
            where TContract : class, IScriptObject
        {
            return new ContractHandle(c.GetType(), c.GetHashCode());
        }
    }
#endif
}