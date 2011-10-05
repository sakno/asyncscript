using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using QCodeBinaryOperatorType = Compiler.Ast.ScriptCodeBinaryOperatorType;
    using QCodeUnaryOperatorType = Compiler.Ast.ScriptCodeUnaryOperatorType;

    /// <summary>
    /// Represents an interface that describes runtime behaviour of DynamicScript object.
    /// </summary>
    [ComVisible(false)]
    
    public interface IScriptObject : IDynamicMetaObjectProvider
    {
        /// <summary>
        /// Performs binary operation.
        /// </summary>
        /// <param name="operator">THe binary operator.</param>
        /// <param name="right">The right operand of the operator.</param>
        /// <param name="state">The internal interpreter state.</param>
        /// <returns>The binary operation result.</returns>
        IScriptObject BinaryOperation(QCodeBinaryOperatorType @operator, IScriptObject right, InterpreterState state);

        /// <summary>
        /// Performs unary operation.
        /// </summary>
        /// <param name="operator">The unary operation.</param>
        /// <param name="state">The internal interpreter state.</param>
        /// <returns>The unary operation result.</returns>
        IScriptObject UnaryOperation(QCodeUnaryOperatorType @operator, InterpreterState state);

        /// <summary>
        /// Performs invocation of the object.
        /// </summary>
        /// <param name="args">The invocation arguments.</param>
        /// <param name="state">The internal state of the interpreter.</param>
        /// <returns>The invocation result.</returns>
        IScriptObject Invoke(IList<IScriptObject> args, InterpreterState state);

        /// <summary>
        /// Gets runtime slot by its name.
        /// </summary>
        /// <param name="slotName">The name of the slot to obtain.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The runtime slot; or <see langword="null"/> if it is not existed.</returns>
        /// <remarks>This indexer provides implementation of the MemberAccess operator.</remarks>
        IRuntimeSlot this[string slotName, InterpreterState state]
        {
            get;
        }

        /// <summary>
        /// Returns runtime descriptor of the specified slot.
        /// </summary>
        /// <param name="slotName">The name of the slot.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A script object that provides static description of the requested slot.</returns>
        /// <remarks>This method provides implementation of the MetadataDiscovery operator.</remarks>
        IScriptObject GetRuntimeDescriptor(string slotName, InterpreterState state);

        /// <summary>
        /// Gets or sets element to the object.
        /// </summary>
        /// <param name="args">The arguments of the array contract.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The element of the array contract.</returns>
        IRuntimeSlot this[IScriptObject[] args, InterpreterState state]
        {
            get;
        }

        /// <summary>
        /// Gets names of all runtime slots.
        /// </summary>
        ICollection<string> Slots { get; }

        /// <summary>
        /// Returns contract of the current object.
        /// </summary>
        /// <returns>The contract of the current object.</returns>
        IScriptContract GetContractBinding();
    }
}
