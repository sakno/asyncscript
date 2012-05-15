using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeStatement = Compiler.Ast.ScriptCodeStatement;

    /// <summary>
    /// Represents factory of runtime statement.
    /// </summary>
    /// <typeparam name="TStatement">Type of the statement produced by this factory.</typeparam>
    [ComVisible(false)]
    public interface IScriptStatementContract<out TStatement> : IScriptCodeElementFactory<TStatement, IScriptStatement<TStatement>>
        where TStatement : ScriptCodeStatement
    {
    }
}
