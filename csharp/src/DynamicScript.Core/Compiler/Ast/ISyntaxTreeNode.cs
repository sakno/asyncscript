using System;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an interface that is implemented by all DynamicScript expressions or statements.
    /// </summary>
    [ComVisible(false)]
    public interface ISyntaxTreeNode: ICloneable, IRestorable
    {
        /// <summary>
        /// Gets a value indicating that the expression is completed.
        /// </summary>
        /// <remarks>If expression is not completed then translator should throws an exception.</remarks>
        bool Completed { get; }

        /// <summary>
        /// Validates statement.
        /// </summary>
        /// <exception cref="CodeAnalysisException">The statement has invalid content.</exception>
        void Verify();

        /// <summary>
        /// Visits this syntax tree node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="visitor"></param>
        /// <returns></returns>
        ISyntaxTreeNode Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor);
    }
}
