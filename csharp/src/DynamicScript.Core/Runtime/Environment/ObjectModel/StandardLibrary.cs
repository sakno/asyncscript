using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DynamicScript.Runtime.Environment.ObjectModel
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InliningSourceAttribute = Compiler.Ast.Translation.LinqExpressions.InliningSourceAttribute;
    using SystemEnvironment = System.Environment;
    using SystemConverter = System.Convert;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using File = System.IO.File;
    using ScriptDebugger = Debugging.ScriptDebugger;
    using BindingFlags = System.Reflection.BindingFlags;
    using CultureInfo = System.Globalization.CultureInfo;

    /// <summary>
    /// Encapsulates set of common functions.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class StandardLibrary : ScriptCompositeObject
    {
        #region Nested Types

        [ComVisible(false)]
        private sealed class ImportFunction : ScriptAction<IScriptObject, IScriptCompositeObject>
        {
            public const string Name = "import";
            private const string FirstParamName = "source";
            private const string SecondParamName = "destination";

            public ImportFunction()
                : base(FirstParamName, ScriptSuperContract.Instance, SecondParamName, ScriptCompositeContract.Empty)
            {
            }

            protected override void Invoke(IScriptObject source, IScriptCompositeObject destination, InterpreterState state)
            {
                Import(source, destination, state);
            }
        }

        [ComVisible(false)]
        private sealed class InvokeFunction : ScriptFunc<IScriptObject, IScriptArray>
        {
            public const string Name = "__invoke";
            private const string FirstParamName = "target";
            private const string SecondParamName = "args";

            public InvokeFunction()
                : base(FirstParamName, ScriptSuperContract.Instance, SecondParamName, new ScriptArrayContract(), ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptObject target, IScriptArray arguments, InterpreterState state)
            {
                return __Invoke(target, arguments, state);
            }
        }

        [ComVisible(false)]
        private sealed class WeakRefFunction : ScriptFunc<IScriptObject>
        {
            public const string Name = "weakref";
            private const string FirstParamName = "obj";

            public WeakRefFunction()
                : base(FirstParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptObject arg0, InterpreterState state)
            {
                return WeakRef(arg0, state);
            }
        }

        [ComVisible(false)]
        private sealed class IsOverloadedFunction : ScriptFunc<IScriptFunction>
        {
            public const string Name = "__overloaded";
            private const string FirstParamName = "func";

            public IsOverloadedFunction()
                : base(FirstParamName, ScriptSuperContract.Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptFunction func, InterpreterState state)
            {
                return __Overloaded(func, state);
            }
        }

        [ComVisible(false)]
        private sealed class BindFunction : ScriptFunc<IScriptFunction, IScriptObject>
        {
            public const string Name = "bind";
            private const string FirstParamName = "act";
            private const string SecondParamName = "obj";

            public BindFunction()
                : base(FirstParamName, ScriptSuperContract.Instance, SecondParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptFunction action, IScriptObject @this, InterpreterState state)
            {
                return StandardLibrary.Bind(action, @this, state);
            }
        }

        [ComVisible(false)]
        private sealed class GetDataFunction : ScriptFunc<ScriptString>
        {
            public const string Name = "getdata";
            private const string FirstParamName = "name";

            public GetDataFunction()
                : base(FirstParamName, ScriptStringContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptString name, InterpreterState state)
            {
                return GetData(name, state);
            }
        }

        [ComVisible(false)]
        private sealed class SetDataFunction : ScriptAction<ScriptString, IScriptObject>
        {
            public const string Name = "setdata";
            private const string FirstParamName = "name";
            private const string SecondParamName = "data";

            public SetDataFunction()
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptSuperContract.Instance)
            {
            }

            protected override void Invoke(ScriptString name, IScriptObject obj, InterpreterState state)
            {
                SetData(name, obj, state);
            }
        }

        [ComVisible(false)]
        private sealed class ReflectFunction : ScriptFunc<IScriptObject>
        {
            public const string Name = "reflect";
            private const string FirstParamName = "obj";

            public ReflectFunction()
                : base(FirstParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptObject obj, InterpreterState state)
            {
                return Reflect(obj, state);
            }
        }

        /// <summary>
        /// Represents action that is used to run DynamicScript programs.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class EvalFunction : ScriptFunc<ScriptString, IScriptObject>
        {
            public const string Name = "eval";
            private const string FirstParamName = "script";
            private const string SecondParamName = "scopeObj";

            /// <summary>
            /// Initializes a new instance of the action.
            /// </summary>
            public EvalFunction()
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptString scriptCode, IScriptObject global, InterpreterState state)
            {
                return Eval(scriptCode, global, state);
            }
        }

        [ComVisible(false)]
        private sealed class SplitFunction : ScriptFunc<ScriptCompositeObject>
        {
            public const string Name = "split";
            private const string FirstParamName = "obj";

            public SplitFunction()
                : base(FirstParamName, ScriptCompositeContract.Empty, ScriptIterable.GetContractBinding())
            {
            }

            protected override IScriptObject Invoke(ScriptCompositeObject obj, InterpreterState state)
            {
                return Split(obj, state);
            }
        }

        [ComVisible(false)]
        private sealed class ParseFunction : ScriptFunc<ScriptString, IScriptContract, ScriptString>
        {
            private const string FirstParamName = "value";
            private const string SecondParamName = "t";
            private const string ThirdParamName = "lang";
            public const string Name = "parse";

            public ParseFunction()
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptMetaContract.Instance, ThirdParamName, ScriptStringContract.Instance, ScriptSuperContract.Instance)
            {
            }

            public override IScriptObject Invoke(ScriptString value, IScriptContract type, ScriptString language, InterpreterState state)
            {
                return Parse(value, type, language, state);
            }
        }

        [ComVisible(false)]
        private sealed class EnumFunction : ScriptFunc<IScriptArray>
        {
            public const string Name = "enum";
            private const string FirstParamName = "elements";

            public EnumFunction()
                : base(FirstParamName, ScriptDimensionalContract.Instance, ScriptFinSetContract.Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptArray array, InterpreterState state)
            {
                return Enum(array, state);
            }
        }

        [ComVisible(false)]
        private sealed class NewObjFunction : ScriptFunc<ScriptString, IScriptContract>
        {
            public const string Name = "newobj";
            private const string FirstParamName = "name";
            private const string SecondParamName = "contract";

            public NewObjFunction()
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptMetaContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptString name, IScriptContract contract, InterpreterState state)
            {
                return NewObj(name, contract, state);
            }
        }

        [ComVisible(false)]
        private sealed class DebugSlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "debug";

            public override IScriptObject GetValue(InterpreterState state)
            {
                return (ScriptBoolean)state.DebugMode;
            }

            public IScriptContract ContractBinding
            {
                get { return ScriptBooleanContract.Instance; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
            }

            public override bool HasValue
            {
                get { return true; }
                protected set { }
            }
        }

        [ComVisible(false)]
        private sealed class CompiledSlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "compiled";

            public override IScriptObject GetValue(InterpreterState state)
            {
                return ScriptBoolean.True;
            }

            public IScriptContract ContractBinding
            {
                get { return ScriptBooleanContract.Instance; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
            }

            public override bool HasValue
            {
                get { return true; }
                protected set { }
            }
        }

        [ComVisible(false)]
        private sealed class ArgsSlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "args";
            private ScriptArray m_cached;
            public readonly ScriptArrayContract ContractBinding = new ScriptArrayContract(ScriptStringContract.Instance);

            public override IScriptObject GetValue(InterpreterState state)
            {
                if (m_cached == null || m_cached.GetLength(0) != state.Arguments.Count)
                {
                    m_cached = state.Arguments.Count == 0 ? ScriptArray.Empty(ScriptStringContract.Instance) :
                        ScriptArray.Create(state.Arguments.Select(a => new ScriptString(a)).ToArray());
                }
                return m_cached;
            }

            IScriptContract IStaticRuntimeSlot.ContractBinding
            {
                get { return ContractBinding; }
            }

            public override bool DeleteValue()
            {
                m_cached = null;
                return true;
            }

            public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
            }

            public override bool HasValue
            {
                get { return true; }
                protected set { }
            }
        }

        [ComVisible(false)]
        private sealed class LoadModuleFunction : ScriptFunc<ScriptString>
        {
            /// <summary>
            /// Represents name of the action.
            /// </summary>
            public const string Name = "use";
            private const string FirstParamName = "scriptFile";


            public LoadModuleFunction()
                : base(FirstParamName, ScriptStringContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptString scriptFile, InterpreterState state)
            {
                return Use(scriptFile, state);
            }
        }

        [ComVisible(false)]
        private sealed class PrepareModuleFunction : ScriptAction<ScriptString, ScriptBoolean>
        {
            public const string Name = "prepare";
            private const string FirstParamName = "scriptFile";
            private const string SecondParamName = "pc";

            public PrepareModuleFunction()
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptBooleanContract.Instance)
            {
            }

            protected override void Invoke(ScriptString scriptFile, ScriptBoolean cacheResult, InterpreterState state)
            {
                Prepare(scriptFile, cacheResult, state);
            }
        }

        [ComVisible(false)]
        private sealed class QuitFunction : ScriptAction<ScriptInteger>
        {
            public const string Name = "quit";
            private const string FirstParamName = "exitCode";

            public QuitFunction()
                : base(FirstParamName, ScriptIntegerContract.Instance)
            {
            }

            protected override void Invoke(ScriptInteger exitCode, InterpreterState state)
            {
                Quit(exitCode, state);
            }
        }

        [ComVisible(false)]
        private sealed class WorkingDirectorySlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "wdir";

            public override IScriptObject GetValue(InterpreterState state)
            {
                return new ScriptString(SystemEnvironment.CurrentDirectory);
            }

            public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
            {
                if (ScriptStringContract.TryConvert(ref value))
                {
                    SystemEnvironment.CurrentDirectory = SystemConverter.ToString(value);
                    return value;
                }
                else if (state.Context == InterpretationContext.Unchecked)
                    return value;
                else throw new ContractBindingException(value, ScriptStringContract.Instance, state);
            }

            public IScriptContract ContractBinding
            {
                get { return ScriptStringContract.Instance; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
            }

            public override bool HasValue
            {
                get { return true; }
                protected set { }
            }
        }

        [ComVisible(false)]
        private sealed class ReadOnlyFunction : ScriptFunc<IScriptCompositeObject>
        {
            public const string Name = "readonly";
            private const string FirstParamName = "obj";

            public ReadOnlyFunction()
                : base(FirstParamName, ScriptCompositeContract.Empty, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptCompositeObject obj, InterpreterState state)
            {
                return ReadOnly(obj, state);
            }
        }

        [ComVisible(false)]
        private sealed class RegexFunction : ScriptFunc
        {
            public const string Name = "regex";

            public RegexFunction()
                : base(ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InterpreterState state)
            {
                return Regex(state);
            }
        }

        [ComVisible(false)]
        private sealed class DeclarePropertyFunction : ScriptFunc<ScriptCompositeObject, ScriptString, IScriptContract, IScriptFunction, IScriptFunction>
        {
            public const string Name = "declareProperty";
            private const string FirstParamName = "obj";
            private const string SecondParamName = "propertyName";
            private const string ThirdParamName = "contract";
            private const string FourthParamName = "getter";
            private const string FifthParamName = "setter";

            public DeclarePropertyFunction()
                : base(FirstParamName, ScriptCompositeContract.Empty, SecondParamName, ScriptStringContract.Instance, ThirdParamName, ScriptMetaContract.Instance, FourthParamName, ScriptSuperContract.Instance, FifthParamName, ScriptSuperContract.Instance, ScriptBooleanContract.Instance)
            {
            }

            public override IScriptObject Invoke(ScriptCompositeObject obj, ScriptString propertyName, IScriptContract contract, IScriptFunction getter, IScriptFunction setter, InterpreterState state)
            {
                return DeclareProperty(obj, propertyName, contract, getter, setter, state);
            }
        }

        [ComVisible(false)]
        private sealed new class Slots : ObjectSlotCollection
        {
            public Slots()
            {
                Add<DebugSlot>(DebugSlot.Name);
                Add<CompiledSlot>(CompiledSlot.Name);
                AddConstant<NewObjFunction>(NewObjFunction.Name);
                AddConstant<LoadModuleFunction>(LoadModuleFunction.Name);
                AddConstant<PrepareModuleFunction>(PrepareModuleFunction.Name);
                Add<ArgsSlot>(ArgsSlot.Name);
                AddConstant<DebuggerModule>(DebuggerModule.Name);
                Add<WorkingDirectorySlot>(WorkingDirectorySlot.Name);
                AddConstant("ver", new ScriptInteger(DynamicScriptInterpreter.Version.Major));
                AddConstant<ReadOnlyFunction>(ReadOnlyFunction.Name);
                AddConstant<EnumFunction>(EnumFunction.Name);
                AddConstant<SplitFunction>(SplitFunction.Name);
                AddConstant<EvalFunction>(EvalFunction.Name);
                AddConstant<SetDataFunction>(SetDataFunction.Name);
                AddConstant<GetDataFunction>(GetDataFunction.Name);
                AddConstant<RegexFunction>(RegexFunction.Name);
                AddConstant<ReflectFunction>(ReflectFunction.Name);
                AddConstant<BindFunction>(BindFunction.Name);
                AddConstant<WeakRefFunction>(WeakRefFunction.Name);
                AddConstant<ParseFunction>(ParseFunction.Name);
                AddConstant<QuitFunction>(QuitFunction.Name);
                AddConstant<ImportFunction>(ImportFunction.Name);
                AddConstant<InvokeFunction>(InvokeFunction.Name);
                AddConstant<IsOverloadedFunction>(IsOverloadedFunction.Name);
                AddConstant<DeclarePropertyFunction>(DeclarePropertyFunction.Name);
            }
        }
        #endregion


        private StandardLibrary()
            : base(new Slots())
        {
        }

        private static ISet<string> m_entities;

        internal static bool IsStandardLibraryEntity(string name)
        {
            const BindingFlags NestedTypeFlags = BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            const BindingFlags NameFieldFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
            if (m_entities == null)
                m_entities = new HashSet<string>(from t in typeof(StandardLibrary).GetNestedTypes(NestedTypeFlags)
                                                 let nameField = t.GetField("Name", NameFieldFlags)
                                                 where nameField != null
                                                 select (string)nameField.GetValue(null), StringEqualityComparer.Instance);
            return m_entities.Contains(name);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly StandardLibrary Instance = new StandardLibrary();

        internal static MemberExpression InstanceAccess
        {
            get { return LinqHelpers.BodyOf<Func<StandardLibrary>, MemberExpression>(() => Instance); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exitCode"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject Quit(ScriptInteger exitCode, InterpreterState state)
        {
            SystemEnvironment.Exit(exitCode.IsInt32 ? (int)exitCode : 0);
            return Void;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject ReadOnly(IScriptCompositeObject obj, InterpreterState state)
        {
            return Extensions.IfThenElse<IScriptObject>(obj != null, obj.AsReadOnly(state), Void);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptCompositeObject Regex(InterpreterState state)
        {
            return new Regex();
        }

        private static bool Prepare(Uri scriptFile, bool cacheResult, InterpreterState state)
        {
            var cookie = state.Cache.Lookup(scriptFile, DynamicScriptInterpreter.OnFlyCompiler);
            switch (cacheResult && cookie != null)
            {
                case true:
                    cookie.ScriptResult = cookie.CompiledScript.Invoke(state);
                    return true;
                default: return false;
            }
        }

        private static bool Prepare(IEnumerable<Uri> scriptFiles, bool cacheResult, InterpreterState state)
        {
            foreach (var f in scriptFiles)
                if (Prepare(f, cacheResult, state)) return true;
            return false;
        }

        /// <summary>
        /// Compiles the specified script file.
        /// </summary>
        /// <param name="scriptFile"></param>
        /// <param name="cacheResult"></param>
        /// <param name="state"></param>
        [InliningSource]
        public static IScriptObject Prepare(ScriptString scriptFile, ScriptBoolean cacheResult, InterpreterState state)
        {
            if (scriptFile == null || cacheResult == null) throw new ArgumentException();
            Prepare(RuntimeHelpers.PrepareScriptFilePath(scriptFile), cacheResult, state);
            return Void;
        }

        private static IScriptObject Use(IEnumerable<Uri> scriptLocations, InterpreterState state)
        {
            IScriptObject module = Void;
            foreach (var location in scriptLocations)
                if (Use(location, out module, state)) break;
            return module;
        }

        private static bool Use(Uri scriptLocation, out IScriptObject module, InterpreterState state)
        {
            //if script file is not existed the return void.
            if (scriptLocation.IsFile && !File.Exists(scriptLocation.LocalPath))
            {
                module = Void;
                return false;
            }
            var compiledScript = state.Cache.Cached(scriptLocation) ?
                state.Cache.Lookup(scriptLocation, DynamicScriptInterpreter.OnFlyCompiler).CompiledScript :
                InterpreterState.Compile(scriptLocation, DynamicScriptInterpreter.OnFlyCompiler).CompiledScript;
            //trace module loading
            if (ScriptDebugger.CurrentDebugger != null) ScriptDebugger.CurrentDebugger.OnLoadModule(scriptLocation);
            module = compiledScript.Invoke(state);
            return true;
        }

        /// <summary>
        /// Executes a script stored in the specified file.
        /// </summary>
        /// <param name="scriptFile"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject Use(ScriptString scriptFile, InterpreterState state)
        {
            if (scriptFile == null) throw new ArgumentException();
            else if (scriptFile.IsEmptyOrWhitespace) return Void;
            else return Use(RuntimeHelpers.PrepareScriptFilePath(scriptFile), state);
        }

        /// <summary>
        /// Constructs a new object dynamically.
        /// </summary>
        /// <param name="name">The name of the slot.</param>
        /// <param name="contract">The contract binding for the slot. Cannot be <see cref="ScriptObject.Void"/>.</param>
        /// <param name="state"></param>
        /// <returns>The created object.</returns>
        [InliningSource]
        public static IScriptObject NewObj(ScriptString name, IScriptContract contract, InterpreterState state)
        {
            if (name == null || IsVoid(contract)) throw new ArgumentException();
            return new ScriptCompositeObject(new[] { Variable(name, contract) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject Enum(IScriptArray array, InterpreterState state)
        {
            if (array == null) throw new ContractBindingException(new ScriptArrayContract(), state);
            return array.GetContractBinding().Rank == 1 || array.GetLength(0) > 1L ? new ScriptSetContract(array) : null;
        }

        private static IScriptObject Parse(ScriptString value, IScriptContract type, CultureInfo formatProvider)
        {
            if (type is ScriptIntegerContract)
                return ScriptInteger.TryParse(value, formatProvider);
            else if (type is ScriptStringContract || type is ScriptSuperContract)
                return value;
            else if (type is ScriptRealContract)
                return ScriptRealContract.TryParse(value, formatProvider);
            else if (type is ScriptBooleanContract)
                return ScriptBooleanContract.TryParse(value);
            else if (type is ExpressionTrees.ScriptExpressionFactory)
                return ExpressionTrees.ScriptExpressionFactory.Parse(value);
            else if (type is ScriptVoid)
                return Void;
            else return Void;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="language"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject Parse(ScriptString value, IScriptContract type, ScriptString language, InterpreterState state)
        {
            if (value == null || language == null) throw new ContractBindingException(ScriptStringContract.Instance, state);
            if (type == null) throw new ContractBindingException(ScriptMetaContract.Instance, state);
            return Parse(value, type, language.IsEmptyOrWhitespace ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfoByIetfLanguageTag(language));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptCompositeObject Split(ScriptCompositeObject obj, InterpreterState state)
        {
            if (obj == null) throw new ContractBindingException(ScriptCompositeContract.Empty, state);
            return new ScriptIterable(obj != null ? obj.Split() : Enumerable.Empty<ScriptCompositeObject>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scriptCode"></param>
        /// <param name="global"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject Eval(ScriptString scriptCode, IScriptObject global, InterpreterState state)
        {
            if (scriptCode == null) throw new ContractBindingException(ScriptStringContract.Instance, state);
            if (IsVoid(global)) global = state.Global;
            if (scriptCode == null) scriptCode = ScriptString.Empty;
            using (var enumerator = scriptCode.Value.GetEnumerator())
            {
                var compiledScript = DynamicScriptInterpreter.OnFlyCompiler.Invoke(enumerator);
                return compiledScript.Invoke(state);
            }
        }

        /// <summary>
        /// Reflects the specified composite contract.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject Reflect(IScriptObject obj, InterpreterState state)
        {
            if (obj is ScriptCompositeContract)
                return new ScriptIterable(((ScriptCompositeContract)obj).Reflect());
            else if (obj is IScriptCompositeObject)
                return new ScriptCompositeObjectMetadata((IScriptCompositeObject)obj);
            else return ScriptCompositeContract.Empty.CreateCompositeObject(EmptyArray, state);
        }

        /// <summary>
        /// Stores object into the internal data store.
        /// </summary>
        /// <param name="name">The name of data slot.</param>
        /// <param name="obj">An object to store.</param>
        /// <param name="state">Internal interpreter state.</param>
        [InliningSource]
        public static IScriptObject SetData(ScriptString name, IScriptObject obj, InterpreterState state)
        {
            if (name == null) throw new ContractBindingException(ScriptStringContract.Instance, state);
            state[name] = IsVoid(obj) ? null : obj;
            return Void;
        }

        /// <summary>
        /// Restores object from the internal data store.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject GetData(ScriptString name, InterpreterState state)
        {
            if (name == null) throw new ContractBindingException(ScriptStringContract.Instance, state);
            return (state[name] as IScriptObject) ?? Void;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="this"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject Bind(IScriptFunction action, IScriptObject @this, InterpreterState state)
        {
            if (action == null) throw new ContractBindingException(ScriptCallableContract.Instance, state);
            return action.Bind(@this ?? Void);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptBoolean __Overloaded(IScriptFunction func, InterpreterState state)
        {
            return func is ScriptFunctionBase.ICombination;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject WeakRef(IScriptObject arg0, InterpreterState state)
        {
            return arg0 is ScriptWeakReference ? arg0 : new ScriptWeakReference(arg0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="arguments"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject __Invoke(IScriptObject target, IScriptArray arguments, InterpreterState state)
        {
            if (arguments == null) throw new ContractBindingException(new ScriptArrayContract(), state);
            return target.Invoke(arguments.ToArray(), state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject Import(IScriptObject source, IScriptCompositeObject destination, InterpreterState state)
        {
            if (destination == null) throw new ContractBindingException(ScriptCompositeContract.Empty, state);
            destination.Import(source, state);
            return Void;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="contract"></param>
        /// <param name="getter"></param>
        /// <param name="setter"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptBoolean DeclareProperty(ScriptCompositeObject obj, ScriptString propertyName, IScriptContract contract, IScriptFunction getter, IScriptFunction setter, InterpreterState state)
        {
            if (obj == null) throw new ContractBindingException(ScriptCompositeContract.Empty, state);
            if (getter == null && setter == null) throw new ScriptFault("Getter or setter should be specified", state);
            if (IsVoid(contract)) contract = ScriptSuperContract.Instance;
            return obj.AddSlot(propertyName, new ScriptProperty(contract, getter, setter));
        }
    }
}
