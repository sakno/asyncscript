using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeExpressionCollection = System.CodeDom.CodeExpressionCollection;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptArrayExpressionFactory: ScriptExpressionFactory<ScriptCodeArrayExpression, ScriptArrayExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string FirstParamName = "elems";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(FirstParamName, Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetElementFunction : ScriptFunc<IScriptExpression<ScriptCodeArrayExpression>, ScriptInteger>
        {
            public const string Name = "elementAt";
            private const string FirstParamName = "array";
            private const string SecondParamName = "index";

            public GetElementFunction()
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, ScriptExpressionFactory.Instance)
            {
            }

            private static IScriptObject Invoke(CodeExpressionCollection elements, long index)
            {
                return index.Between(0, elements.Count - 1) ?
                    Convert(elements[(int)index]) :
                    Void;
            }

            protected override IScriptObject Invoke(IScriptExpression<ScriptCodeArrayExpression> array, ScriptInteger index, InterpreterState state)
            {
                return index.IsInt32 ? Invoke(array.CodeObject.Elements, index) : Void;
            }
        }

        [ComVisible(false)]
        private sealed class GetLengthFunction : CodeElementPartProvider<ScriptInteger>
        {
            public const string Name = "length";

            public GetLengthFunction()
                : base(Instance, ScriptIntegerContract.Instance)
            {
            }

            protected override ScriptInteger Invoke(ScriptCodeArrayExpression element, InterpreterState state)
            {
                return element.Elements.Count;
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptArrayExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptArrayExpressionFactory>
        {
            {ModifyFunction.Name, (owner, state) =>LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
            {GetElementFunction.Name, (owner, state) => LazyField<GetElementFunction, IScriptFunction>(ref owner.m_element)},
            {GetLengthFunction.Name, (owner, state) => LazyField<GetLengthFunction, IScriptFunction>(ref owner.m_length)}
        };

        public new const string Name = "array";

        private IScriptFunction m_modify;
        private IScriptFunction m_element;
        private IScriptFunction m_length;

        private ScriptArrayExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptArrayExpressionFactory()
            :base(Name)
        {
        }

        public static readonly ScriptArrayExpressionFactory Instance = new ScriptArrayExpressionFactory();

        public static ScriptArrayExpression CreateExpression(IEnumerable<IScriptObject> elements = null)
        {
            var expr = ScriptArrayExpression.CreateExpression(elements);
            return expr != null ? new ScriptArrayExpression(expr) : null;
        }

        public override ScriptArrayExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return CreateExpression();
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject>);
                default: return null;
            }
        }

        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }

        public override IScriptObject this[string slotName, InterpreterState state]
        {
            get { return StaticSlots.GetValue(this, slotName, state); }
            set { StaticSlots.SetValue(this, slotName, value, state); }
        }

        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return StaticSlots.GetSlotMetadata(this, slotName, state);
        }
    }
}
