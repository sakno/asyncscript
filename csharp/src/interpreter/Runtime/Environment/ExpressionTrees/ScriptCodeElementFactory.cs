using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeObject = System.CodeDom.CodeObject;
    using MethodInfo = System.Reflection.MethodInfo;
    using Enumerable = System.Linq.Enumerable;
    using ISyntaxTreeNode = Compiler.Ast.ISyntaxTreeNode;

    /// <summary>
    /// Represents an abstract factory for code element runtime representation.
    /// </summary>
    /// <typeparam name="TCodeObject">Type of the code code element</typeparam>
    /// <typeparam name="TRuntimeElement">Type of the runtime representation of the code element.</typeparam>
    [ComVisible(false)]
    [Serializable]
    abstract class ScriptCodeElementFactory<TCodeObject, TRuntimeElement> : 
        ScriptContract, 
        ISystemCodeElementFactory<TCodeObject, TRuntimeElement>,
        ISerializable
        where TCodeObject : CodeObject, ISyntaxTreeNode
        where TRuntimeElement : ScriptObject, IScriptCodeElement<TCodeObject>
    {
        #region Nested Types
        [ComVisible(false)]
        protected abstract class CodeElementPartProvider<TOutput> : ScriptFunc<IScriptCodeElement<TCodeObject>>
            where TOutput: class, IScriptObject
        {
            private const string FirstParamName = "elem";

            protected CodeElementPartProvider(ScriptCodeElementFactory<TCodeObject, TRuntimeElement> inputContract, IScriptContract outputContract = null)
                : base(FirstParamName, inputContract, outputContract)
            {
            }

            protected abstract TOutput Invoke(TCodeObject element, InterpreterState state);

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptCodeElement<TCodeObject> arg0)
            {
                IScriptObject result = arg0 != null ? Invoke(arg0.CodeObject, ctx.RuntimeState) : null;
                return result ?? Void;
            }
        }

        [ComVisible(false)]
        protected class ModifyActionBase : ScriptRuntimeAction
        {
            private const string FirstParamName = "element";


            public ModifyActionBase(ScriptCodeElementFactory<TCodeObject, TRuntimeElement> param0, ScriptActionContract.Parameter param1)
                : base(Modify, new ScriptActionContract.Parameter(FirstParamName, param0), param1)
            {
            }

            public ModifyActionBase(ScriptCodeElementFactory<TCodeObject, TRuntimeElement> param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2)
                : base(Modify, new ScriptActionContract.Parameter(FirstParamName, param0), param1, param2)
            {
            }

            public ModifyActionBase(ScriptCodeElementFactory<TCodeObject, TRuntimeElement> param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, ScriptActionContract.Parameter param3)
                : base(Modify, new ScriptActionContract.Parameter(FirstParamName, param0), param1, param2, param3)
            {
            }

            public ModifyActionBase(ScriptCodeElementFactory<TCodeObject, TRuntimeElement> param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, ScriptActionContract.Parameter param3, ScriptActionContract.Parameter param4)
                : base(Modify, new ScriptActionContract.Parameter(FirstParamName, param0), param1, param2, param3, param4)
            {
            }

            public ModifyActionBase(ScriptCodeElementFactory<TCodeObject, TRuntimeElement> param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, ScriptActionContract.Parameter param3, ScriptActionContract.Parameter param4, ScriptActionContract.Parameter param5)
                : base(Modify, new ScriptActionContract.Parameter(FirstParamName, param0), param1, param2, param3, param4, param5)
            {
            }

            private static void Modify(InvocationContext ctx, IRuntimeSlot param0, IRuntimeSlot param1, IRuntimeSlot param2, IRuntimeSlot param3, IRuntimeSlot param4, IRuntimeSlot param5)
            {
                var element = param0.GetValue(ctx.RuntimeState) as TRuntimeElement;
                element.Modify(new[] { param1.GetValue(ctx.RuntimeState), param2.GetValue(ctx.RuntimeState), param3.GetValue(ctx.RuntimeState), param4.GetValue(ctx.RuntimeState), param5.GetValue(ctx.RuntimeState) }, ctx.RuntimeState);
            }

            private static void Modify(InvocationContext ctx, IRuntimeSlot param0, IRuntimeSlot param1, IRuntimeSlot param2, IRuntimeSlot param3, IRuntimeSlot param4)
            {
                var element = param0.GetValue(ctx.RuntimeState) as TRuntimeElement;
                element.Modify(new[] { param1.GetValue(ctx.RuntimeState), param2.GetValue(ctx.RuntimeState), param3.GetValue(ctx.RuntimeState), param4.GetValue(ctx.RuntimeState) }, ctx.RuntimeState);
            }

            private static void Modify(InvocationContext ctx, IRuntimeSlot param0, IRuntimeSlot param1, IRuntimeSlot param2, IRuntimeSlot param3)
            {
                var element = param0.GetValue(ctx.RuntimeState) as TRuntimeElement;
                element.Modify(new[] { param1.GetValue(ctx.RuntimeState), param2.GetValue(ctx.RuntimeState), param3.GetValue(ctx.RuntimeState) }, ctx.RuntimeState);
            }

            private static void Modify(InvocationContext ctx, IRuntimeSlot param0, IRuntimeSlot param1, IRuntimeSlot param2)
            {
                var element = param0.GetValue(ctx.RuntimeState) as TRuntimeElement;
                element.Modify(new[] { param1.GetValue(ctx.RuntimeState), param2.GetValue(ctx.RuntimeState) }, ctx.RuntimeState);
            } 

            private static void Modify(InvocationContext ctx, IRuntimeSlot param0, IRuntimeSlot param1)
            {
                var element = param0.GetValue(ctx.RuntimeState) as TRuntimeElement;
                element.Modify(new[] { param1.GetValue(ctx.RuntimeState) }, ctx.RuntimeState);
            }
        }

        #endregion

        private const string NameHolder = "Name";
        private readonly string m_name;

        protected ScriptCodeElementFactory(SerializationInfo info, StreamingContext context)
            : this(info.GetString(NameHolder))
        {
        }

        protected ScriptCodeElementFactory(string contractName)
        {
            if (string.IsNullOrEmpty(contractName)) throw new ArgumentNullException("contractName");
            m_name = contractName;
        }

        /// <summary>
        /// Gets name of this contract.
        /// </summary>
        public string Name
        {
            get { return m_name; }
        }

        /// <summary>
        /// Creates a new runtime representation of the code element.
        /// </summary>
        /// <param name="args">Code element creation arguments.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime code element.</returns>
        public abstract TRuntimeElement CreateCodeElement(IList<IScriptObject> args, InterpreterState state);

        protected static TRuntimeElement CreateCodeElement(Func<TRuntimeElement> factory, IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 0 ? factory.Invoke() : null;
        }

        protected static TRuntimeElement CreateCodeElement<T>(Func<T, TRuntimeElement> factory, IList<IScriptObject> args, InterpreterState state)
            where T: class, IScriptObject
        {
            return args.Count == 1 ? factory.Invoke(args[0] as T) : null;
        }

        protected static TRuntimeElement CreateCodeElement<T1, T2>(Func<T1, T2, TRuntimeElement> factory, IList<IScriptObject> args, InterpreterState state)
            where T1 : class, IScriptObject
            where T2 : class, IScriptObject
        {
            return args.Count == 2 ? factory.Invoke(args[0] as T1, args[1] as T2) : null;
        }

        /// <summary>
        /// Creates a new runtime representation of the code element.
        /// </summary>
        /// <param name="args">Code element creation arguments.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new runtime code element.</returns>
        public sealed override ScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
        {
            ScriptObject result = CreateCodeElement(args, state);
            return result ?? Void;
        }

        /// <summary>
        /// Releases a memory associated with cached runtime slots.
        /// </summary>
        public virtual void Clear()
        {
        }

        /// <summary>
        /// Computes a hash code for this contract.
        /// </summary>
        /// <returns>A hash code of this contract.</returns>
        public sealed override int GetHashCode()
        {
            return typeof(TRuntimeElement).MetadataToken;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(NameHolder, m_name);
        }

        #region Runtime Slots

        /// <summary>
        /// Gets runtime slot that holds implementation of Modify action.
        /// </summary>
        protected abstract IRuntimeSlot Modify
        {
            get;
        }

        IRuntimeSlot ICodeElementFactorySlots.Modify
        {
            get { return Modify; }
        }

        #endregion

    }
}
