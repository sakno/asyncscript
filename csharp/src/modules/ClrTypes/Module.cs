using System;
using DynamicScript.Runtime;
using DynamicScript.Runtime.Environment;
using System.Collections.Generic;
using System.Linq;

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
                : base(new ScriptFunctionContract.Parameter(FirstParamName, ScriptStringContract.Instance), ScriptSuperContract.Instance)
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
                : base(new ScriptFunctionContract.Parameter(FirstParamName, ScriptStringContract.Instance), ScriptSuperContract.Instance)
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
        private sealed class IsGenericDefinitionAction : ScriptFunc<IScriptClass>
        {
            public const string Name = "is_generic_def";
            private const string FirstParamName = "t";

            public IsGenericDefinitionAction()
                : base(new ScriptFunctionContract.Parameter(FirstParamName, ScriptMetaContract.Instance), ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptClass @class, InterpreterState state)
            {
                return @class != null ? (ScriptBoolean)@class.NativeType.IsGenericTypeDefinition : ScriptBoolean.False;
            }
        }

        [ComVisible(false)]
        private sealed class MakeGenericAction : ScriptFunc<IScriptContract, IScriptArray, ScriptBoolean>
        {
            public const string Name = "generic";
            private const string FirstParamName = "baseType";
            private const string SecondParamName = "interfaces";
            private const string ThirdParamName = "defctor";

            public MakeGenericAction()
                : base(new ScriptFunctionContract.Parameter(FirstParamName, ScriptMetaContract.Instance),
                new ScriptFunctionContract.Parameter(SecondParamName, new ScriptArrayContract(ScriptMetaContract.Instance)),
                new ScriptFunctionContract.Parameter(ThirdParamName, ScriptBooleanContract.Instance),
                ScriptMetaContract.Instance)
            {
            }

            private static IScriptGeneric MakeGeneric(IScriptGeneric baseGeneric, IEnumerable<IScriptClass> interfaces, bool defctor)
            {
                return new ScriptGeneric(baseGeneric, interfaces, defctor);
            }

            private static IScriptGeneric MakeGeneric(Type baseType, IEnumerable<IScriptClass> interfaces, bool defctor)
            {
                return new ScriptGeneric((ScriptClass)baseType, interfaces, defctor);
            }

            public override IScriptObject Invoke(IScriptContract baseType, IScriptArray interfaces, ScriptBoolean defctor, InterpreterState state)
            {
                if (interfaces == null) interfaces = ScriptArray.Empty(ScriptSuperContract.Instance);
                return baseType is IScriptGeneric ?
                    MakeGeneric((IScriptGeneric)baseType, from iface in interfaces
                                                          let typedef = (ScriptClass)ScriptClass.GetType(iface as IScriptContract)
                                                          where typedef != null
                                                          select typedef, defctor) :
                                                          MakeGeneric(ScriptClass.GetType(baseType), from iface in interfaces
                                                                                let typedef = (ScriptClass)ScriptClass.GetType(iface as IScriptContract)
                                                                                where typedef != null
                                                                                select typedef, defctor);
            }
        }

        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots()
            {
                AddConstant<FromGacAction>(FromGacAction.Name);
                AddConstant<FromFileAction>(FromFileAction.Name);
                AddConstant<IsGenericDefinitionAction>(IsGenericDefinitionAction.Name);
                AddConstant<MakeGenericAction>(MakeGenericAction.Name);
                AddConstant("mscorlib", ScriptAssembly.MsCorLib);
                AddConstant("system", ScriptAssembly.System);
                AddConstant("systemcore", ScriptAssembly.SystemCore);
            }
        }
        #endregion

        static Module()
        {
            ScriptObject.RegisterConverter<ScriptClass.TypeConverter>();
            ScriptObject.RegisterConverter<ScriptGeneric.GenericConverter>();
            ScriptObject.RegisterConverter<ScriptMethod.DelegateConverter>();
        }

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
