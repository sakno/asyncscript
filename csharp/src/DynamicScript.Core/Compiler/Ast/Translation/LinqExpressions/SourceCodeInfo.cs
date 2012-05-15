using System;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SymbolDocumentInfo = System.Linq.Expressions.SymbolDocumentInfo;

    /// <summary>
    /// Represents information about DynamicScript source code.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class SourceCodeInfo: SourceCodeInfo<SymbolDocumentInfo>
    {
        /// <summary>
        /// Initializes a new information about DynamicScript source code.
        /// </summary>
        /// <param name="fileName">The path to the source file. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="symbolDocument">The debugging information source.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/> or empty.</exception>
        public SourceCodeInfo(string fileName, SymbolDocumentInfo symbolDocument = null)
            : base(fileName, symbolDocument)
        {
        }
    }
}
