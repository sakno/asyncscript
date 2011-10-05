using System;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Expression = System.Linq.Expressions.Expression;

    /// <summary>
    /// Represents an interface for object
    /// that supports restoration.
    /// </summary>
    [ComVisible(false)]
    
    public interface IRestorable
    {
        /// <summary>
        /// Restores the current state of
        /// the object as an expression.
        /// </summary>
        /// <returns>An expression that procudes
        /// the current object.</returns>
        Expression Restore();
    }
}
