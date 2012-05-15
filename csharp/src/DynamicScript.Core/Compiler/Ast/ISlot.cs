using System;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeExpression = System.CodeDom.CodeExpression;

    /// <summary>
    /// Represents slot in the object/type expression or action signature.
    /// </summary>
    [ComVisible(false)]
    interface ISlot: IEquatable<ISlot>, ISyntaxTreeNode
    {
        /// <summary>
        /// Gets name of the slot.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets slot initialization expression.
        /// </summary>
        ScriptCodeExpression InitExpression { get; }

        /// <summary>
        /// Gets slot contract expression.
        /// </summary>
        ScriptCodeExpression ContractBinding { get; }

        /// <summary>
        /// Gets declaration style.
        /// </summary>
        SlotDeclarationStyle Style { get; }
    }
}
