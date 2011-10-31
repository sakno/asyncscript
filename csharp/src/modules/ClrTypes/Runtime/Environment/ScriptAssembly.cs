using System;
using DynamicScript.Runtime.Environment;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Assembly = System.Reflection.Assembly;

    /// <summary>
    /// Represents script-compliant representation of the .NET assembly.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptAssembly: ScriptCompositeObject
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class GetClassAction : ScriptFunc<ScriptString>
        {
            public const string Name = "class";
            private const string FirstParamName = "name";

            private readonly Assembly m_source;

            public GetClassAction(Assembly source)
                : base(new ScriptActionContract.Parameter(FirstParamName, ScriptStringContract.Instance), ScriptMetaContract.Instance)
            {
                m_source = source;
            }

            protected override IScriptObject Invoke(ScriptString typeName, InterpreterState state)
            {
                var resolvedType = m_source.GetType(typeName, false);
                if (resolvedType == null) return Void;
                else return (ScriptClass)resolvedType;
            }
        }

        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots(Assembly source)
            {
                AddConstant(GetClassAction.Name, new GetClassAction(source));
                AddConstant("name", new ScriptString(source.FullName));
                AddConstant("location", new ScriptString(source.Location));
            }
        }
        #endregion

        public static readonly ScriptAssembly MsCorLib = new ScriptAssembly(typeof(string));
        public static readonly ScriptAssembly System = new ScriptAssembly(typeof(Uri));
        public static readonly ScriptAssembly SystemCore = new ScriptAssembly(typeof(System.Dynamic.CallInfo));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asm"></param>
        public ScriptAssembly(Assembly asm)
            : base(new Slots(asm))
        {
        }

        public ScriptAssembly(Type t)
            : this(t.Assembly)
        {
        }
    }
}
