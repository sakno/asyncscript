using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using QCodeBinaryOperatorType = Compiler.Ast.ScriptCodeBinaryOperatorType;

    /// <summary>
    /// Represents action that is combined from another actions.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class ScriptCombinedAction : ScriptObject, IEnumerable<IScriptAction>
    {
        private readonly IEnumerable<IScriptAction> m_actions;
        private readonly IScriptContract m_contract;

        internal ScriptCombinedAction(IEnumerable<IScriptAction> actions, InterpreterState state)
        {
            m_contract = ScriptContract.Unite(actions.Select(a => a.GetContractBinding()), state);
            m_actions = actions;
            
        }

        /// <summary>
        /// Initializes a new combined action.
        /// </summary>
        /// <param name="action1">The first action to combine. Cannot be <see langword="null"/>.</param>
        /// <param name="action2">The second action to combine. Cannot be <see langword="null"/>.</param>
        public ScriptCombinedAction(ScriptRuntimeAction action1, ScriptRuntimeAction action2)
            : this(new[] { action1, action2 }, InterpreterState.Initial)
        {
        }

        /// <summary>
        /// Gets collection of combined actions.
        /// </summary>
        public IEnumerable<IScriptAction> Actions
        {
            get { return m_actions; }
        }

        /// <summary>
        /// Invokes combined action.
        /// </summary>
        /// <param name="args">The arguments for the action.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>Invocation result.</returns>
        /// <exception cref="ActionArgumentsMistmatchException">No one action in the collection can be invoked with specified arguments.</exception>
        public override IScriptObject Invoke(IList<IScriptObject> args, InterpreterState state)
        {
            foreach (var a in Actions)
                if (a.CanInvoke(args))
                    return a.Invoke(args, state);
            throw new ActionArgumentsMistmatchException(state);
        }

        internal ScriptCombinedAction Combine(IScriptAction action, InterpreterState state)
        {
            if (action == null) throw new ArgumentNullException("action");
            return new ScriptCombinedAction(Enumerable.Concat(Actions, new[] { action }), state);
        }

        /// <summary>
        /// Provides combination of the current collection with the specified action.
        /// </summary>
        /// <param name="right">The action to combine.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The collection of actions.</returns>
        protected override IScriptObject Add(IScriptObject right, InterpreterState state)
        {
            if (right is ScriptCombinedAction)
                return Unite(this, (ScriptCombinedAction)right, state);
            else if (right is IScriptAction)
                return Unite(this, new[] { right }, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Returns contract binding of the action list.
        /// </summary>
        /// <returns></returns>
        public override IScriptContract GetContractBinding()
        {
            return m_contract;
        }

        /// <summary>
        /// Returns an enumerator through combined actions.
        /// </summary>
        /// <returns>An enumerator through combined actions.</returns>
        public new IEnumerator<IScriptAction> GetEnumerator()
        {
            return Actions.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
