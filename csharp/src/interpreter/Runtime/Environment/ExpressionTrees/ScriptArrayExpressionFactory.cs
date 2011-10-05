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
    sealed class ScriptArrayExpressionFactory: ScriptExpressionFactory<ScriptCodeArrayExpression, ScriptArrayExpression>, IArrayExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string FirstParamName = "elems";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(FirstParamName, Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetElementAction : ScriptFunc<IScriptExpression<ScriptCodeArrayExpression>, ScriptInteger>
        {
            private const string FirstParamName = "array";
            private const string SecondParamName = "index";

            public GetElementAction()
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, ScriptExpressionFactory.Instance)
            {
            }

            private static IScriptObject Invoke(CodeExpressionCollection elements, long index)
            {
                return index.Between(0, elements.Count - 1) ?
                    Convert(elements[(int)index]) :
                    Void;
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptExpression<ScriptCodeArrayExpression> array, ScriptInteger index)
            {
                return index.IsInt32 ? Invoke(array.CodeObject.Elements, index) : Void;
            }
        }

        [ComVisible(false)]
        private sealed class GetLengthAction : CodeElementPartProvider<ScriptInteger>
        {
            public GetLengthAction()
                : base(Instance, ScriptIntegerContract.Instance)
            {
            }

            protected override ScriptInteger Invoke(ScriptCodeArrayExpression element, InterpreterState state)
            {
                return element.Elements.Count;
            }
        }
        #endregion

        public new const string Name = "array";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_element;
        private IRuntimeSlot m_length;

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

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        public override void Clear()
        {
            m_element = m_length = m_modify = null;
        }

        IRuntimeSlot IArrayExpressionFactorySlots.ElementAt
        {
            get { return CacheConst<GetElementAction>(ref m_element); }
        }

        IRuntimeSlot IArrayExpressionFactorySlots.Length
        {
            get { return CacheConst<GetLengthAction>(ref m_length); }
        }
    }
}
