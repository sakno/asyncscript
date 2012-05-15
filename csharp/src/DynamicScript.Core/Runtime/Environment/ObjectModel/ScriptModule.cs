using System;
using System.Dynamic;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Linq;

namespace DynamicScript.Runtime.Environment.ObjectModel
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using DScriptIO = Hosting.DynamicScriptIO;
    using SystemConverter = System.Convert;
    using ConstructorInfo = System.Reflection.ConstructorInfo;

    /// <summary>
    /// Represents an object that holds basic routines for DynamicScript programs.
    /// </summary>
    [ComVisible(false)]
    public class ScriptModule : ScriptCompositeObject
    {
        #region Nested Type

        [ComVisible(false)]
        private sealed class PutsFunction : ScriptAction<IScriptObject>
        {
            /// <summary>
            /// Represents name of the action.
            /// </summary>
            public const string Name = "puts";
            private static string FirstParamName = "obj";

            public PutsFunction()
                : base(FirstParamName, ScriptSuperContract.Instance)
            {
            }

            protected override void Invoke(IScriptObject obj, InterpreterState state)
            {
                Puts(obj);
            }
        }

        [ComVisible(false)]
        private sealed class GetsFunction : ScriptFunc
        {
            public const string Name = "gets";

            public GetsFunction()
                : base(ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InterpreterState state)
            {
                return Gets();
            }
        }

        /// <summary>
        /// Represents collection of the predefined slots.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        protected class ModuleSlots : ObjectSlotCollection
        {
            /// <summary>
            /// Initializes a new collection of the predefined slots.
            /// </summary>
            public ModuleSlots()
            {
                AddConstant<PutsFunction>(PutsFunction.Name);
                AddConstant<GetsFunction>(GetsFunction.Name);
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new module with the specified restrictions.
        /// </summary>
        /// <param name="slots">The collection of the modules slots.</param>
        protected ScriptModule(ModuleSlots slots)
            : base(slots ?? new ModuleSlots())
        {
        }

        /// <summary>
        /// Initializes a new module.
        /// </summary>
        public ScriptModule()
            : this(null)
        {
        }

        /// <summary>
        /// Writes the specified object to the output stream.
        /// </summary>
        /// <param name="obj">The object to be written to the output stream.</param>
        public static void Puts(IScriptObject obj)
        {
            DScriptIO.WriteLine(obj);
        }

        /// <summary>
        /// Reads object from the input stream.
        /// </summary>
        /// <returns>The object restored from the input stream.</returns>
        public static IScriptObject Gets()
        {
            return DScriptIO.ReadLine();
        }

        internal static ConstructorInfo DefaultConstructor
        {
            get { return LinqHelpers.BodyOf<Func<ScriptModule>, NewExpression>(() => new ScriptModule()).Constructor; }
        }
    }
}
