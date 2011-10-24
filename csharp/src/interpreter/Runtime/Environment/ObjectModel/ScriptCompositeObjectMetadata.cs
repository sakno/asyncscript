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
        private sealed class GetSlotValueAction : ScriptGetItemAction<IScriptCompositeObject>
        {
            public GetSlotValueAction(IScriptCompositeObject obj)
                : base(ScriptSuperContract.Instance, new[] { ScriptStringContract.Instance }, obj)
            {
            }

            private static IScriptObject GetItem(IScriptCompositeObject obj, ScriptString name, InterpreterState state)
            {
                return name != null ? obj[name, state].GetValue(state) : Void;
            }

            protected override IScriptObject GetItem(IScriptObject[] indicies, InterpreterState state)
            {
                return GetItem(This, indicies[0] as ScriptString, state); 
            }
        }

        private sealed class SetSlotValueAction : ScriptSetItemAction<IScriptCompositeObject>
        {
            public SetSlotValueAction(IScriptCompositeObject obj)
                : base(ScriptSuperContract.Instance, new IScriptContract[] { ScriptStringContract.Instance, ScriptSuperContract.Instance }, obj)
            {
            }

            private static void SetItem(IScriptCompositeObject obj, ScriptString name, IScriptObject value, InterpreterState state)
            {
                obj[name, state].SetValue(value, state);
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
                AddConstant(GetItemAction, new GetSlotValueAction(obj));
                AddConstant(SetItemAction, new SetSlotValueAction(obj));
            }
        }
        #endregion

        public ScriptCompositeObjectMetadata(IScriptCompositeObject obj)
            : base(new Slots(obj))
        {
        }
    }
}
