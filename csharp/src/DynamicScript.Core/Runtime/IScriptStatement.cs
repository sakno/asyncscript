using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeStatement = Compiler.Ast.ScriptCodeStatement;

    /// <summary>
    /// Represents runtime representation of the script statement.
    /// </summary>
    /// <typeparam name="TStatement">Type of the statement.</typeparam>
    [ComVisible(false)]
    public interface IScriptStatement<out TStatement> : IScriptCodeElement<TStatement>
        where TStatement : ScriptCodeStatement
    {
        /// <summary>
        /// Executes statement.
        /// </summary>
        /// <param name="args">Execution parameters.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/> if the current statement is executable; otherwise, <see langword="false"/>.</returns>
        bool Execute(IList<IScriptObject> args, InterpreterState state);
    }
}
