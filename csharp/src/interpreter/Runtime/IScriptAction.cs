using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CallInfo = System.Dynamic.CallInfo;

    /// <summary>
    /// Represents DynamicScript action.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptAction : IScriptObject
    {
        /// <summary>
        /// Gets action owner.
        /// </summary>
        IScriptObject This { get; }

        /// <summary>
        /// Gets contract of the return value.
        /// </summary>
        IScriptContract ReturnValueContract { get; }

        /// <summary>
        /// Gets internal implementation of the action.
        /// </summary>
        Delegate Implementation { get; }

        /// <summary>
        /// Determines whether the action can be invoked with the specified of arguments.
        /// </summary>
        /// <param name="args">An arguments of the action.</param>
        /// <returns><see langword="true"/> if the action can be invoked with the specified of arguments; otherwise, <see langword="false"/>.</returns>
        bool CanInvoke(IList<IScriptObject> args);

        /// <summary>
        /// Gets information about action parameters.
        /// </summary>
        CallInfo SignatureInfo { get; }

        /// <summary>
        /// Gets a value indicating that this action is produced from composition of two or more
        /// actions.
        /// </summary>
        bool IsComposition { get; }

        /// <summary>
        /// Divides this action on its components.
        /// </summary>
        /// <param name="left">The left part of composition.</param>
        /// <param name="right">The right part of composition.</param>
        /// <returns><see langword="true"/> if this action is composite action; otherwise, <see langword="false"/>.</returns>
        bool Decompose(out IScriptAction left, out IScriptAction right);

        /// <summary>
        /// Composes this action with the specified action.
        /// </summary>
        /// <param name="right">The action to be composed with this action.</param>
        /// <param name="result">The composite action.</param>
        /// <returns><see langword="true"/> if composition is applicable; otherwise, <see langword="false"/>.</returns>
        bool Compose(IScriptAction right, out IScriptAction result);

        /// <summary>
        /// Binds the current function to the specified object.
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        IScriptAction Bind(IScriptObject @this);
    }
}
