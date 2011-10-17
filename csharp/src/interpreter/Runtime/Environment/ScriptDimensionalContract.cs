using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Keyword = Compiler.Keyword;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using LinqExpression = System.Linq.Expressions.Expression;
    using MemberExpression = System.Linq.Expressions.MemberExpression;
    using SystemConverter = System.Convert;

    /// <summary>
    /// Represents DIMENSIONAL contract.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptDimensionalContract: ScriptBuiltinContract, IDimensionalContractSlots
    {
        #region Nested Types

        [ComVisible(false)]
        private sealed class ToSingleAction : ScriptFunc<IScriptArray>
        {
            private const string FirstParamName = "a";

            public ToSingleAction()
                : base(FirstParamName, Instance, Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptArray arg0)
            {
                return arg0.ToSingleDimensional();
            }
        }
        
        #endregion

        private IRuntimeSlot m_single;

        private ScriptDimensionalContract(SerializationInfo info, StreamingContext context)
        {
        }

        private ScriptDimensionalContract()
        {
        }

        /// <summary>
        /// Represents singleton instance of this contract.
        /// </summary>
        public static readonly ScriptDimensionalContract Instance = new ScriptDimensionalContract();

        /// <summary>
        /// Returns default value that satisfies to this contract.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        /// <exception cref="UnsupportedOperationException">This method is not supported.</exception>
        protected internal override ScriptObject FromVoid(InterpreterState state)
        {
            if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        internal override Keyword Token
        {
            get { return Keyword.Dimensional; }
        }

        /// <summary>
        /// Returns relationship with other contract.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is ScriptDimensionalContract)
                return ContractRelationshipType.TheSame;
            else if (contract is ScriptSuperContract)
                return ContractRelationshipType.Subset;
            else if (contract is ScriptArrayContract)
                return ContractRelationshipType.Superset;
            else return ContractRelationshipType.None;
        }

        internal static LinqExpression Expression
        {
            get { return LinqHelpers.BodyOf<Func<ScriptDimensionalContract>, MemberExpression>(() => Instance); }
        }

        /// <summary>
        /// Creates a new array contract.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override ScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 1: return new ScriptArray(args[0]);
                case 2:
                    //The first argument should be contract
                    var elementContract = args[0] as IScriptContract;
                    if (elementContract == null) throw new ContractBindingException(args[0], ScriptMetaContract.Instance, state);
                    //The second argument should contain number of dimensions
                    var dimensions = args[1];
                    if (!ScriptIntegerContract.Convert(ref dimensions)) throw new ContractBindingException(dimensions, ScriptIntegerContract.Instance, state);
                    return new ScriptArrayContract(elementContract, SystemConverter.ToInt32(dimensions));
                default: throw new ActionArgumentsMistmatchException(state);
            }
        }

        #region Runtime Slots

        IRuntimeSlot IDimensionalContractSlots.Single
        {
            get { return CacheConst<ToSingleAction>(ref m_single); }
        }

        #endregion
    }
}
