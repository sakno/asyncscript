using System;

namespace DynamicScript.Runtime.Environment.ObjectModel
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptCompositeObjectMetadata: ScriptCompositeObject
    {
        private const string NamesSlot = "names";

        #region Nested Types

        [ComVisible(false)]
        private sealed class GetSlotValueFunction : ScriptGetItemFunction<IScriptCompositeObject>
        {
            public GetSlotValueFunction(IScriptCompositeObject obj)
                : base(ScriptSuperContract.Instance, new[] { ScriptStringContract.Instance }, obj)
            {
            }

            private static IScriptObject GetItem(IScriptCompositeObject obj, ScriptString name, InterpreterState state)
            {
                return name != null ? obj[name, state] : Void;
            }

            protected override IScriptObject GetItem(IScriptObject[] indicies, InterpreterState state)
            {
                return GetItem(This, indicies[0] as ScriptString, state); 
            }
        }

        private sealed class SetSlotValueFunction : ScriptSetItemAction<IScriptCompositeObject>
        {
            public SetSlotValueFunction(IScriptCompositeObject obj)
                : base(ScriptSuperContract.Instance, new IScriptContract[] { ScriptStringContract.Instance, ScriptSuperContract.Instance }, obj)
            {
            }

            private static void SetItem(IScriptCompositeObject obj, ScriptString name, IScriptObject value, InterpreterState state)
            {
                obj[name, state] = value;
            }

            protected override void SetItem(IScriptObject value, IScriptObject[] indicies, InterpreterState state)
            {
                SetItem(This, value as ScriptString, indicies[0], state);
            }
        }

        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots(IScriptCompositeObject obj)
            {
                if (obj == null) throw new ArgumentNullException("obj");
                //Explore names
                var names = new string[obj.Slots.Count];
                obj.Slots.CopyTo(names, 0);
                AddConstant(NamesSlot, ScriptArray.Create(names));
                AddConstant(GetItemAction, new GetSlotValueFunction(obj));
                AddConstant(SetItemAction, new SetSlotValueFunction(obj));
            }
        }
        #endregion

        public ScriptCompositeObjectMetadata(IScriptCompositeObject obj)
            : base(new Slots(obj))
        {
        }
    }
}
