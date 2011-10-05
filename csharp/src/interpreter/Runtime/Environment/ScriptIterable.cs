using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    /// Represents iterable object.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    sealed class ScriptIterable: ScriptCompositeObject, IIterableSlots
    {
        #region Nested Types
        /// <summary>
        /// Represents iterable contract converter.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        internal sealed class ScriptIterableContractDefConverter : RuntimeConverter<ScriptContract.ScriptIterableContractDef>
        {
            public override bool Convert(ScriptContract.ScriptIterableContractDef input, out IScriptObject result)
            {
                result = input.MakeIterable(GetContractBinding);
                return true;
            }
        }

        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots(IEnumerable<IScriptObject> collection, IScriptContract elementContract)
            {
                AddConstant(IteratorAction, new ScriptIteratorAction(collection, elementContract));
            }
        }
        #endregion

        private ScriptIterable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptIterable(IEnumerable<IScriptObject> enumerable, IScriptContract elementContract = null)
            : base(new Slots(enumerable, elementContract))
        {
        }

        public static ScriptCompositeContract GetContractBinding(IScriptContract elementContract = null)
        {
            return new ScriptCompositeContract(new[] { new KeyValuePair<string, ScriptCompositeContract.SlotMeta>(IteratorAction, new ScriptCompositeContract.SlotMeta(ScriptIteratorAction.GetContractBinding(elementContract))) });
        }

        IRuntimeSlot IIterableSlots.Iterator
        {
            get { return base[IteratorAction]; }
        }

        /// <summary>
        /// Returns an empty script collection.
        /// </summary>
        /// <param name="elementContract"></param>
        /// <returns></returns>
        public static ScriptIterable Empty(IScriptContract elementContract = null)
        {
            return new ScriptIterable(Enumerable.Empty<IScriptObject>(), elementContract);
        }
    }
}
