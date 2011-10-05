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

            protected override IScriptObject GetItem(InvocationContext ctx, IScriptObject[] indicies)
            {
                return GetItem(This, indicies[0] as ScriptString, ctx.RuntimeState); 
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

            protected override void SetItem(InvocationContext ctx, IScriptObject value, IScriptObject[] indicies)
            {
                SetItem(This, value as ScriptString, indicies[0], ctx.RuntimeState);
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

            private static void SetValue(IScriptCompositeObject obj, ScriptString name, IScriptObject value, InterpreterState state)
            {
                obj[name].SetValue(value, state);
            }

            private static void SetValue(InvocationContext ctx, IScriptObject value, params IScriptObject[] indicies)
            {
                if (indicies == null) indicies = new IScriptObject[0];
                if (ctx.This is IScriptCompositeObject && indicies.LongLength == 1L && indicies[0] is ScriptString)
                    SetValue((IScriptCompositeObject)ctx.This, (ScriptString)indicies[0], value, ctx.RuntimeState);
            }

            private static IScriptObject GetValue(IScriptCompositeObject obj, ScriptString name, InterpreterState state)
            {
                return obj[name].GetValue(state);
            }

            private static IScriptObject GetValue(InvocationContext ctx, params IScriptObject[] indicies)
            {
                if (indicies == null) indicies = new IScriptObject[0];
                return indicies.LongLength == 1L && ctx.This is IScriptCompositeObject && indicies[0] is ScriptString ? GetValue((IScriptCompositeObject)ctx.This, (ScriptString)indicies[0], ctx.RuntimeState) : Void;
            }
        }
        #endregion

        public ScriptCompositeObjectMetadata(IScriptCompositeObject obj)
            : base(new Slots(obj))
        {
        }
    }
}
