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
    using InliningSourceAttribute = Compiler.Ast.Translation.LinqExpressions.InliningSourceAttribute;

    /// <summary>
    /// Represents DIMENSIONAL contract.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptDimensionalContract: ScriptBuiltinContract
    {
        #region Nested Types

        [ComVisible(false)]
        private sealed class ToSingleFunction : ScriptFunc<IScriptArray>
        {
            public const string Name = "toSingle";
            private const string FirstParamName = "a";

            public ToSingleFunction()
                : base(FirstParamName, Instance, Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptArray array, InterpreterState state)
            {
                return ToSingle(array, state);
            }
        }
        
        #endregion

        private static AggregatedSlotCollection<ScriptDimensionalContract> StaticSlots = new AggregatedSlotCollection<ScriptDimensionalContract>
        {
            {ToSingleFunction.Name, (owner, state) => LazyField<ToSingleFunction, IScriptFunction>(ref owner.m_single)}
        };

        private IScriptFunction m_single;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject ToSingle(IScriptArray array, InterpreterState state)
        {
            return Extensions.IfThenElse<IScriptObject>(array != null, array.ToSingleDimensional(), Void);
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
        public override IScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
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
                    if (!ScriptIntegerContract.TryConvert(ref dimensions)) throw new ContractBindingException(dimensions, ScriptIntegerContract.Instance, state);
                    return new ScriptArrayContract(elementContract, SystemConverter.ToInt32(dimensions));
                default: throw new FunctionArgumentsMistmatchException(state);
            }
        }

        /// <summary>
        /// Clears all cached internal fields.
        /// </summary>
        public override void Clear()
        {
            m_single = null;
        }

        /// <summary>
        /// Gets collection of aggregated slots.
        /// </summary>
        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }

        /// <summary>
        /// Gets or sets value of the aggregated object.
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IScriptObject this[string slotName, InterpreterState state]
        {
            get { return StaticSlots.GetValue(this, slotName, state); }
            set { StaticSlots.SetValue(this, slotName, value, state); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return StaticSlots.GetSlotMetadata(this, slotName, state);
        }
    }
}
