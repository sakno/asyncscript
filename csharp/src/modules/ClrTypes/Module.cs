using System;
using DynamicScript.Runtime;
using DynamicScript.Runtime.Environment;

namespace DynamicScript.Modules.ClrTypes
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Assembly = System.Reflection.Assembly;
    using FileNotFoundException = System.IO.FileNotFoundException;

    /// <summary>
    /// Represents implementation of ClrTypes module.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class Module: ScriptCompositeObject
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class FromGacAction : ScriptFunc<ScriptString>
        {
            public const string Name = "gac";
            private const string FirstParamName = "name";

            public FromGacAction()
                : base(new ScriptActionContract.Parameter(FirstParamName, ScriptStringContract.Instance), ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptString assemblyName, InterpreterState state)
            {
                if(string.IsNullOrWhiteSpace(assemblyName))return Void;
                try
                {
                    return new ScriptAssembly(Assembly.Load(assemblyName));
                }
                catch (FileNotFoundException)
                {
                    return Void;
                }
            }
        }

        [ComVisible(false)]
        private sealed class FromFileAction : ScriptFunc<ScriptString>
        {
            public const string Name = "file";
            private const string FirstParamName = "fileName";

            public FromFileAction()
                : base(new ScriptActionContract.Parameter(FirstParamName, ScriptStringContract.Instance), ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptString fileName, InterpreterState state)
            {
                try
                {
                    return new ScriptAssembly(Assembly.LoadFrom(fileName));
                }
                catch (FileNotFoundException)
                {
                    return Void;
                }
            }
        }

        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots()
            {
                AddConstant<FromGacAction>(FromGacAction.Name);
                AddConstant<FromFileAction>(FromFileAction.Name);
                AddConstant("mscorlib", ScriptAssembly.MsCorLib);
                AddConstant("system", ScriptAssembly.System);
                AddConstant("systemcore", ScriptAssembly.SystemCore);
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new module.
        /// </summary>
        public Module()
            : base(new Slots())
        {
        }

        /// <summary>
        /// Executes an entry point of the module.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IScriptObject Run(InterpreterState state)
        {
            return new Module();
        }
    }
}
