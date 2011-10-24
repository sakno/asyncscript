using System;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeExpression = System.CodeDom.CodeExpression;

    /// <summary>
    /// Represents contract binding that is known at compile-time.
    /// </summary>
    /// <typeparam name="TContract">Contract of the literal value.</typeparam>
    [ComVisible(false)]
    public interface IStaticContractBinding<out TContract>
        where TContract: ScriptCodeExpression
    {
        /// <summary>
        /// Gets contract of the literal value.
        /// </summary>
        TContract Contract { get; }
    }
}
