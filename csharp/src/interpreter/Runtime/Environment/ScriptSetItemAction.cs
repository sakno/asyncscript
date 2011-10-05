using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents item setter.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public abstract class ScriptSetItemAction: ScriptActionBase, IIndexerAccessor
    {
        #region Nested Types
        /// <summary>
        /// Represents item setter contract.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        private sealed class ActionContract : ScriptIndexerActionContract
        {
            private const string ValueParameterName = "value";
            private const string IndiciesHolder = "Indicies";
            private const string ItemHolder = "Item";

            /// <summary>
            /// Represents name of the item setter action.
            /// </summary>
            public const string Name = ScriptObject.SetItemAction;
            private readonly Parameter[] m_indicies;
            private readonly Parameter m_item;

            private ActionContract(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                m_indicies = (Parameter[])info.GetValue(IndiciesHolder, typeof(Parameter[]));
                m_item = (Parameter)info.GetValue(ItemHolder, typeof(Parameter));
            }

            private ActionContract(IScriptContract valueContract, IScriptContract[] indicies, Parameter item, Parameter[] ics)
                : base(CreateSignature(valueContract, indicies, out item, out ics))
            {
                m_indicies = ics;
                m_item = item;
            }

            /// <summary>
            /// Initializes a new item setter contract.
            /// </summary>
            /// <param name="valueContract">The contract of the value to set.</param>
            /// <param name="indicies">The contracts of the indicies.</param>
            public ActionContract(IScriptContract valueContract, params IScriptContract[] indicies)
                : this(valueContract, indicies, null, null)
            {
            }

            private static IEnumerable<Parameter> CreateSignature(IScriptContract valueContract, IScriptContract[] indicies, out Parameter item, out Parameter[] ics)
            {
                if (valueContract == null) throw new ArgumentNullException("valueContract");
                if (indicies == null) indicies = new IScriptContract[0];
                item = new Parameter(ValueParameterName, valueContract);
                var signature = new List<Parameter>(indicies.Length + 1) { item };
                const string IndexParameterPrefix = "index{0}";
                ics = new Parameter[indicies.Length];
                for (var i = 0; i < indicies.Length; i++)
                    signature.Add(ics[i] = new Parameter(String.Format(IndexParameterPrefix, i), indicies[i]));
                return signature;
            }

            /// <summary>
            /// Gets an array of the parameters that describes indicies.
            /// </summary>
            public override Parameter[] Indicies
            {
                get
                {
                    return m_indicies;
                }
            }

            internal Parameter Item
            {
                get { return m_item; }
            }

            protected override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue(IndiciesHolder, Indicies, typeof(Parameter[]));
                info.AddValue(ItemHolder, Item, typeof(Parameter));
                base.GetObjectData(info, context);
            }
        }

        #endregion

        private ScriptSetItemAction(ActionContract contract, IScriptObject @this)
            : base(contract, @this)
        {
        }

        /// <summary>
        /// Initializes a new item setter.
        /// </summary>
        /// <param name="valueContract">The contract of the value to set.</param>
        /// <param name="indicies">The array of indicies.</param>
        /// <param name="this">The action owner.</param>
        public ScriptSetItemAction(IScriptContract valueContract, IScriptContract[] indicies, IScriptObject @this = null)
            : this(new ActionContract(valueContract, indicies), @this)
        {
        }

        /// <summary>
        /// Gets contract of the item setter.
        /// </summary>
        /// <returns>The item setter contract.</returns>
        private new ActionContract GetContractBinding()
        {
            return (ActionContract)base.GetContractBinding();
        }

        /// <summary>
        /// Returns the contract of the item setter action.
        /// </summary>
        /// <param name="valueContract">The contract of the value to set.</param>
        /// <param name="indicies">The indicies.</param>
        /// <returns></returns>
        public static ScriptIndexerActionContract GetContractBinding(IScriptContract valueContract, params IScriptContract[] indicies)
        {
            return new ActionContract(valueContract, indicies);
        }

        /// <summary>
        /// Gets contract of the item.
        /// </summary>
        public IScriptContract ItemContract
        {
            get { return GetContractBinding().Item.ContractBinding; }
        }

        /// <summary>
        /// Gets contracts of the item setter indicies.
        /// </summary>
        public IScriptContract[] Indicies
        {
            get { return Array.ConvertAll(GetContractBinding().Indicies, p => p.ContractBinding); }
        }

        /// <summary>
        /// Sets item value.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value">A value to set.</param>
        /// <param name="indicies"></param>
        protected abstract void SetItem(InvocationContext ctx, IScriptObject value, IScriptObject[] indicies);

        /// <summary>
        /// Sets item value.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        protected internal sealed override IScriptObject Invoke(InvocationContext ctx, params IRuntimeSlot[] arguments)
        {
            var indicies = new IScriptObject[arguments.LongLength - 1];
            for (var i = 1; i < arguments.LongLength; i++)
                indicies[i - 1] = arguments[i].GetValue(ctx.RuntimeState);
            SetItem(ctx, arguments[0].GetValue(ctx.RuntimeState), indicies);
            return Void;
        }
    }

    [ComVisible(false)]
    internal abstract class ScriptSetItemAction<TOwner> : ScriptSetItemAction
        where TOwner: class, IScriptObject
    {
        /// <summary>
        /// Initializes a new item setter.
        /// </summary>
        /// <param name="valueContract">The contract of the value to set.</param>
        /// <param name="indicies">The array of indicies.</param>
        /// <param name="this">The action owner.</param>
        public ScriptSetItemAction(IScriptContract valueContract, IScriptContract[] indicies, TOwner @this)
            : base(valueContract, indicies, @this)
        {
        }

        public new TOwner This
        {
            get { return (TOwner)base.This; }
        }
    }
}
