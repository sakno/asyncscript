﻿using System;
using System.Collections.Generic;

namespace DynamicScript.Compiler.Ast.Translation
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents lexical scope.
    /// </summary>
    [ComVisible(false)]
    public interface ILexicalScope
    {
        /// <summary>
        /// Gets variables declared in the current scope.
        /// </summary>
        IEnumerable<string> Variables { get; }

        /// <summary>
        /// Declares a new variable with the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the variable.</typeparam>
        /// <param name="variableName">The name of the variable.</param>
        /// <returns><see langword="true"/> if variable is registered in the scope; <see langword="false"/>
        /// if variable with the specified name is already declared in the scope.</returns>
        bool DeclareVariable<T>(string variableName);

        /// <summary>
        /// Generates temporary variable name.
        /// </summary>
        /// <returns>The temporary variable name.</returns>
        string GenerateVariableName();

        /// <summary>
        /// Gets parent lexical scope.
        /// </summary>
        ILexicalScope Parent { get; }

        /// <summary>
        /// Gets a value indicating whether this scope represents a code block inside
        /// of the same stack frame.
        /// </summary>
        bool Transparent { get; }
    }
}
