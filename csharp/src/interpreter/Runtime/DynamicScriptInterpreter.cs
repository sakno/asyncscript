using System;
using System.Dynamic;
using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.IO;
using System.Reflection.Emit;
using System.Reflection;

namespace DynamicScript.Runtime
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using LinqExpressionTranslator = Compiler.Ast.Translation.LinqExpressions.LinqExpressionTranslator;
    using QSourceInfo = Compiler.Ast.Translation.LinqExpressions.SourceCodeInfo;
    using CodeAnalysisException = Compiler.CodeAnalysisException;
    using ErrorMode = Compiler.Ast.Translation.ErrorMode;
    using QScriptIO = Hosting.DynamicScriptIO;
    using Encoding = System.Text.Encoding;
    using CompiledScriptAttribute = Hosting.CompiledScriptAttribute;
    using ScriptDebugger = Debugging.ScriptDebugger;
    using InteractiveDebugger = Debugging.Interaction.InteractiveDebugger;
    using DScriptModule = Environment.ObjectModel.ScriptModule;
    using ScriptObject = Environment.ScriptObject;
    using CodeStatement = System.CodeDom.CodeStatement;

    /// <summary>
    /// Represents DynamicScript language context.
    /// This class cannot be inherited.
    /// </summary>
    /// <remarks>This is an entry point to work with DynamicScript programs from your application.</remarks>
    
    [ComVisible(false)]
    public sealed class DynamicScriptInterpreter : LanguageContext
    {
        #region Nested Types

        [ComVisible(false)]
        private sealed class PreparedScript : MarshalByRefObject
        {
            private readonly ScriptInvoker m_invoker;
            private readonly bool m_debug;
            private readonly int m_internPoolSize;

            public PreparedScript(ScriptInvoker invoker, bool debug, int internPoolSize = 200)
            {
                if (invoker == null) throw new ArgumentNullException("invoker");
                m_invoker = invoker;
                m_debug = debug;
                m_internPoolSize = internPoolSize;
            }

            public dynamic Invoke(IScriptObject global, string[] args)
            {
                return m_invoker.Invoke(new InterpreterState(global, args, m_internPoolSize, InterpretationContext.Default, m_debug));
            }

            public static implicit operator Func<IScriptObject, string[], dynamic>(PreparedScript prep)
            {
                return prep != null ? new Func<IScriptObject, string[], dynamic>(prep.Invoke) : null;
            }
        }

        /// <summary>
        /// Represents in-memory compiled representation of the DynamicScript program.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class DScriptCode : ScriptCode
        {
            private readonly PreparedScript m_implementation;
            private readonly IScriptObject m_global;

            public DScriptCode(SourceUnit unit, PreparedScript scriptImplementation, ScriptCompilerOptions options)
                : base(unit)
            {
                if (scriptImplementation == null) throw new ArgumentNullException("scriptImplementation");
                if (options == null) throw new ArgumentNullException("options");
                m_implementation = scriptImplementation;
                m_global = options.Global;
            }

            /// <summary>
            /// Executes script code.
            /// </summary>
            /// <param name="scope">The scope of the script execution.</param>
            /// <returns>Execution result.</returns>
            public override object Run(Scope scope)
            {
                return Run(scope.Storage);
            }

            /// <summary>
            /// Initializes a new instance of the script execution scope.
            /// </summary>
            /// <returns></returns>
            public override Scope CreateScope()
            {
                return new Scope(m_global);
            }

            private object Run(IScriptObject module)
            {
                return m_implementation.Invoke(module, new string[0]);
            }
        }

        [ComVisible(false)]
        private sealed class DScriptAssembly
        {
            private const string MainMethod = "ds_main";
            private const MethodAttributes MainMethodSemantic = MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig;
            
            private const string ScriptType = "DynamicScript";
            private const TypeAttributes ScriptTypeSemantic = TypeAttributes.Sealed | TypeAttributes.Abstract | TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.Class;
            private const string ScriptMethod = CompiledScriptAttribute.DefaultScriptMethod;
            private const MethodAttributes ScriptMethodSemantic = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;

            private readonly AssemblyBuilder m_assembly;
            private readonly ScriptCompilerOptions m_options;
            private readonly bool m_emitSymbolInfo;

            public DScriptAssembly(ScriptCompilerOptions options, bool emitSymbolInfo = false)
            {
                if (options == null) throw new ArgumentNullException("options");
                m_options = options;
                m_emitSymbolInfo = emitSymbolInfo;
                m_assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName { Name = options.AssemblyName, Version = options.AssemblyVersion }, AssemblyBuilderAccess.RunAndSave);
                //emits additional assembly information
                if (options.AssemblyInfo != null) options.AssemblyInfo.Emit(m_assembly);
                m_assembly.SetCustomAttribute(new CustomAttributeBuilder(CLSCompliantAttributeConstructor, new object[] { false }));    //DynamicScript assembly is not CLS-compliant
                //m_assembly.SetCustomAttribute(new CustomAttributeBuilder(AssemblyVersionAttributeConstructor, new[] { options.AssemblyVersion.ToString() }));   //emits assembly version
                m_assembly.SetCustomAttribute(new CustomAttributeBuilder(AssemblyDefaultAliasAttributeConstructor, new[] { options.AssemblyName }));            //emits alias name
            }

            private ScriptCompilerOptions Options
            {
                get { return m_options; }
            }

            private bool EmitSymbolInfo
            {
                get { return m_emitSymbolInfo; }
            }

            private static Type ScriptInvokerReturnType
            {
                get { return CompiledScriptAttribute.ScriptMethodReturnType; }
            }

            private static Type[] ScriptMethodParameters
            {
                get { return CompiledScriptAttribute.ScriptMethodParameters; }
            }

            private static ConstructorInfo CLSCompliantAttributeConstructor
            {
                get { return typeof(CLSCompliantAttribute).GetConstructor(new[] { typeof(bool) }); }
            }

            private static ConstructorInfo AssemblyVersionAttributeConstructor
            {
                get { return typeof(AssemblyVersionAttribute).GetConstructor(new[] { typeof(string) }); }
            }

            private static ConstructorInfo AssemblyDefaultAliasAttributeConstructor
            {
                get { return typeof(AssemblyDefaultAliasAttribute).GetConstructor(new[] { typeof(string) }); }
            }

            public ScriptInvoker Compile(Expression<ScriptInvoker> scriptImplementation, DebugInfoGenerator generator = null)
            {
                if (scriptImplementation == null) throw new ArgumentNullException("scriptImplementation");
                //creates dynamic module
                const string ExecutableFileExtension = ".exe";
                const string ClassLibraryFileExresion = ".dll";
                var fileName = Path.ChangeExtension(Options.AssemblyName, Options.Executable ? ExecutableFileExtension : ClassLibraryFileExresion);
                var module = m_assembly.DefineDynamicModule(Options.AssemblyName, fileName, EmitSymbolInfo);
                //creates class
                var st = module.DefineType(ScriptType, ScriptTypeSemantic);
                //declares method that implements script logic.
                var sm = st.DefineMethod(ScriptMethod, ScriptMethodSemantic, ScriptInvokerReturnType, ScriptMethodParameters);  //signature is satisfied to ScriptInvoker delegate 
                //Compiles script to the method.
                switch (generator != null)
                {
                    case true:
                        scriptImplementation.CompileToMethod(sm, generator);
                        break;
                    default:
                        scriptImplementation.CompileToMethod(sm);
                        break;
                }
                //Defines global attribute
                CompiledScriptAttribute.Emit(m_assembly, st, sm);
                if (Options.Executable)
                {
                    //Creates entry point
                    var ep = st.DefineMethod(MainMethod, MainMethodSemantic, typeof(void), new[] { typeof(string[]) });
                    //Emits entry point implementation
                    var entryPointImpl = ep.GetILGenerator();
                    entryPointImpl.Emit(OpCodes.Newobj, DScriptModule.DefaultConstructor);  //create a new instance of the module
                    entryPointImpl.Emit(OpCodes.Ldarg_0);                                   //loas command-line arguments
                    entryPointImpl.Emit(OpCodes.Ldc_I4, 10);                                //load default size of the intern pool
                    entryPointImpl.Emit(OpCodes.Ldc_I4_0);                                  //load default context
                    entryPointImpl.Emit(OpCodes.Ldc_I4_0);                                  //false for debug flag
                    entryPointImpl.Emit(OpCodes.Newobj, InterpreterState.Constructor);      //initializes a new interpreter state
                    entryPointImpl.EmitCall(OpCodes.Call, sm, null);             //call script implementation
                    entryPointImpl.Emit(OpCodes.Pop);                            //removes result from the stack
                    entryPointImpl.Emit(OpCodes.Ret);                                       //return from method
                    m_assembly.SetEntryPoint(ep, InterpreterHostAttribute.GetHostType());
                }
                //Completes type
                st.CreateType();
                //Saves module to the file.
                m_assembly.Save(fileName, Options.PEKind, Options.ImageType);
                return (ScriptInvoker)Delegate.CreateDelegate(typeof(ScriptInvoker), st.GetMethod(ScriptMethod), true);
            }

            public DScriptCode Compile(SourceUnit source, Expression<ScriptInvoker> scriptImplementation, DebugInfoGenerator generator = null)
            {
                if (source == null) throw new ArgumentNullException("source");
                return new DScriptCode(source, new PreparedScript(Compile(scriptImplementation, generator), generator != null), Options);
            }
        }
        #endregion

        /// <summary>
        /// Represents name of the language.
        /// </summary>
        public const string LanguageName = "QScript";

        /// <summary>
        /// Represents identifier of DynamicScript language.
        /// </summary>
        public static readonly Guid Language = new Guid("{AA452734-6095-4C10-9C0B-260B30ABB158}");

        /// <summary>
        /// Represents DynamicScript language vendor.
        /// </summary>
        public static readonly Guid Vendor = new Guid("{BA451735-7085-4A90-8B0B-160A20BBC128}");

        private static readonly Guid SymbolDocumentType = new Guid("{C7453745-A005-4B91-2A0B-161C30BB0137}");

        /// <summary>
        /// Represents DynamicScript language version.
        /// </summary>
        public static readonly Version Version = new Version(1, 0);

        private readonly LanguageOptions m_options;

        /// <summary>
        /// Initializes a new language context.
        /// </summary>
        /// <param name="domainManager">Script language manager. Cannot be <see langword="null"/>.</param>
        /// <param name="options">Interpreter options.</param>
        public DynamicScriptInterpreter(ScriptDomainManager domainManager, IDictionary<string, object> options)
            : base(domainManager)
        {
            m_options = new LanguageOptions(options);
            //Redirect DLR IO to DynamicScript IO
            domainManager.SharedIO.SetOutput(null, QScriptIO.Output);
            domainManager.SharedIO.SetInput(null, QScriptIO.Input, Encoding.Unicode);
            domainManager.SharedIO.SetErrorOutput(null, QScriptIO.Error);
        }

        /// <summary>
        /// Compiles DynamicScript program.
        /// </summary>
        /// <param name="sourceUnit">The source code to be compiled.</param>
        /// <param name="options">Compilation options.</param>
        /// <param name="errorSink">Error sink that is used to notify caller about compilation errors.</param>
        /// <returns>The script code that can be executed.</returns>
        public override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink)
        {
            return CompileSourceCode(sourceUnit, options is ScriptCompilerOptions ? (ScriptCompilerOptions)options : ScriptCompilerOptions.Default, errorSink);
        }

        /// <summary>
        /// Compiles DynamicScript program.
        /// </summary>
        /// <param name="sourceUnit">The source code to be compiled.</param>
        /// <param name="options">Compilation options.</param>
        /// <param name="errorSink">Error sink that is used to notify caller about compilation errors.</param>
        /// <returns>The script code that can be executed.</returns>
        public static ScriptCode CompileSourceCode(SourceUnit sourceUnit, ScriptCompilerOptions options, ErrorSink errorSink)
        {
            if (sourceUnit == null) throw new ArgumentNullException("sourceUnit");
            if (options == null) options = ScriptCompilerOptions.Default;
            var scriptImplementation = default(Expression<ScriptInvoker>);
            var internPoolSize = 0;
            try
            {
                scriptImplementation = sourceUnit.Kind == SourceCodeKind.File ?
                    CompileSourceCode(sourceUnit.Path, sourceUnit.EmitDebugSymbols ? sourceUnit.Document : null, options, out internPoolSize) :
                    CompileSourceCode(sourceUnit.GetCode(), options, out internPoolSize);
            }
            catch (CodeAnalysisException e)
            {
                var sourceLocation = new SourceLocation(0, e.Position.Line, e.Position.Column);
                errorSink.Add(sourceUnit, e.Message, new SourceSpan(sourceLocation, sourceLocation), (int)e.ErrorCode, Severity.Error);
                scriptImplementation = LinqExpressionTranslator.TranslateError(e);
            }
            switch (sourceUnit.EmitDebugSymbols)
            {
                case true:
                    var pdbgen = DebugInfoGenerator.CreatePdbGenerator();
                    return options.CompileToAssembly ? new DScriptAssembly(options, true).Compile(sourceUnit, scriptImplementation, pdbgen) : new DScriptCode(sourceUnit, new PreparedScript(scriptImplementation.Compile(pdbgen), true, internPoolSize), options);
                default:
                    return options.CompileToAssembly ? new DScriptAssembly(options, false).Compile(sourceUnit, scriptImplementation) : new DScriptCode(sourceUnit, new PreparedScript(scriptImplementation.Compile(), false, internPoolSize), options);
            }
        }

        private static Expression<ScriptInvoker> CompileSourceCode(IEnumerable<char> sourceCode, ScriptCompilerOptions options, out int capacity)
        {
            using (var enumerator = sourceCode.GetEnumerator())
                return CompileSourceCode(enumerator, null, out capacity);
        }

        private static Expression<ScriptInvoker> CompileSourceCode(string sourceFile, SymbolDocumentInfo symbolDocument, ScriptCompilerOptions options, out int capacity)
        {
            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.None))
            using (var enumerator = new CharEnumerator(sourceStream))
                return CompileSourceCode(enumerator, sourceFile, symbolDocument, out capacity);
        }

        private static Expression<ScriptInvoker> CompileSourceCode(IEnumerator<char> sourceCode, string sourceFile, SymbolDocumentInfo symbolDocument, out int capacity)
        {

            return CompileSourceCode(sourceCode, string.IsNullOrEmpty(sourceFile) ? null : new QSourceInfo(sourceFile, symbolDocument), out capacity);
        }

        private static Expression<ScriptInvoker> CompileSourceCode(IEnumerator<char> sourceCode, QSourceInfo sourceInfo, out int capacity)
        {
            return LinqExpressionTranslator.Translate(sourceCode, ErrorMode.Panic, sourceInfo, out capacity);
        }

        /// <summary>
        /// Returns a new instance of the DynamicScript compiler options.
        /// </summary>
        /// <returns>A new instance of the DynamicScript compiler options.</returns>
        public override CompilerOptions GetCompilerOptions()
        {
            return new ScriptCompilerOptions();
        }

        /// <summary>
        /// Gets DynamicScript language identifier.
        /// </summary>
        public override Guid LanguageGuid
        {
            get
            {
                return Language;
            }
        }

        /// <summary>
        /// Gets version of DynamicScript language.
        /// </summary>
        public override Version LanguageVersion
        {
            get
            {
                return Version;
            }
        }

        /// <summary>
        /// Gets language options.
        /// </summary>
        public override LanguageOptions Options
        {
            get
            {
                return m_options;
            }
        }

        /// <summary>
        /// Gets DynamicScript language vendor.
        /// </summary>
        public override Guid VendorGuid
        {
            get
            {
                return Vendor;
            }
        }

        private static dynamic Run(IScriptObject module, IEnumerator<char> sourceCode, string[] args)
        {
            if (module == null) throw new ArgumentNullException("module");
            if (sourceCode == null) sourceCode = String.Empty.GetEnumerator();
            var internPoolSize = 0;
#if DEBUG
            var compiledScript = CompileSourceCode(sourceCode, null, out internPoolSize);
            var preparedScript = new PreparedScript(compiledScript.Compile(), false, internPoolSize);
            return preparedScript.Invoke(module, args);
#else
            var preparedScript = new PreparedScript(CompileSourceCode(sourceCode, null, out internPoolSize).Compile(), false, internPoolSize);
            return preparedScript.Invoke(module, args);
#endif
        }

        /// <summary>
        /// Executes the specified script code.
        /// </summary>
        /// <param name="module">The script object that encapsulates global routines. Cannot be <see langword="null"/>.</param>
        /// <param name="sourceCode">The source code to be executed.</param>
        /// <param name="args">The arguments to be passed into the script.</param>
        /// <returns>The script execution result.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="module"/> is <see langword="null"/>.</exception>
        public static dynamic Run(ScriptObject module, IEnumerable<char> sourceCode, params string[] args)
        {
            if (sourceCode == null) sourceCode = String.Empty;
            using (var enumerator = sourceCode.GetEnumerator())
                return Run(module, enumerator, args);
        }

        /// <summary>
        /// Executes the specified script code.
        /// </summary>
        /// <typeparam name="TModule">Type of the global object to be passed to the script program.</typeparam>
        /// <param name="sourceCode">The source code to be executed.</param>
        /// <param name="args">The arguments to be passed into the script.</param>
        /// <returns>The script execution result.</returns>
        public static dynamic Run<TModule>(IEnumerable<char> sourceCode, params string[] args)
            where TModule : DScriptModule, new()
        {
            return Run(new TModule(), sourceCode, args);
        }

        /// <summary>
        /// Executes the specified script code.
        /// </summary>
        /// <param name="sourceCode">The source code to be executed.</param>
        /// <param name="args">The arguments to be passed into the script.</param>
        /// <returns>The script execution result.</returns>
        public static dynamic Run(IEnumerable<char> sourceCode, params string[] args)
        {
            return Run<DScriptModule>(sourceCode, args);
        }

        /// <summary>
        /// Executes the script from file.
        /// </summary>
        /// <param name="module">The global object to be passed into the script. Cannot be <see langword="null"/>.</param>
        /// <param name="sourceFile">The path to the source file to execute.</param>
        /// <param name="emitDebugInfo">Specifies that the debugging information should be emitted.</param>
        /// <param name="args">The arguments to be passed into the script.</param>
        /// <returns>Execution result.</returns>
        public static dynamic Run(ScriptObject module, string sourceFile, bool emitDebugInfo, params string[] args)
        {
            if (module == null) throw new ArgumentNullException("module");
            var internPoolSize = 0;
            var translatedResult = CompileSourceCode(sourceFile, emitDebugInfo ? CreateSymbolDocument(sourceFile) : null, null, out internPoolSize);
            try
            {
                //register debugger for the current app domain
                if (emitDebugInfo)
                {
                    //attach default interactive debugger if it is necessary
                    if (!ScriptDebugger.HookOverriden) ScriptDebugger.Debugging += InteractiveDebugger.Hook;
                    ScriptDebugger.Attach(new ScriptDebugger(), module);
                }
                //Execute script
                var preparedScript = new PreparedScript(translatedResult.Compile(), emitDebugInfo, internPoolSize);
                return preparedScript.Invoke(module, args);
            }
            finally
            {
                ScriptDebugger.Detach();
            }
        }

        /// <summary>
        /// Executes the script from file.
        /// </summary>
        /// <typeparam name="TModule">Type of the global object to be passed to the script program.</typeparam>
        /// <param name="sourceFile">The path to the source file to execute.</param>
        /// <param name="emitDebugInfo">Specifies that the debugging information should be emitted.</param>
        /// <param name="args">The arguments to be passed into the script.</param>
        /// <returns>Execution result.</returns>
        public static dynamic Run<TModule>(string sourceFile, bool emitDebugInfo = false, params string[] args)
            where TModule : DScriptModule, new()
        {
            return Run(new TModule(), sourceFile, emitDebugInfo, args);
        }

        /// <summary>
        /// Executes the script from file.
        /// </summary>
        /// <param name="sourceFile">The path to the source file to execute.</param>
        /// <param name="emitDebugInfo">Specifies that the debugging information should be emitted.</param>
        /// <param name="args">The arguments to be passed into the script.</param>
        /// <returns>Execution result.</returns>
        public static dynamic Run(string sourceFile, bool emitDebugInfo, string[] args)
        {
            return Run<DScriptModule>(sourceFile, emitDebugInfo, args);
        }

        /// <summary>
        /// Compiles the source code to the assembly.
        /// </summary>
        /// <param name="sourceCode">The source code to be compiled.</param>
        /// <param name="options">The compiler options.</param>
        /// <returns>The delegate that implements script logic.</returns>
        public static Func<IScriptObject, string[], dynamic> Compile(IEnumerable<char> sourceCode, ScriptCompilerOptions options)
        {
            var assembly = new DScriptAssembly(options);
            var internPoolSize = 0;
            var scriptImplementation = CompileSourceCode(sourceCode, options, out internPoolSize);
            return new PreparedScript(assembly.Compile(scriptImplementation), false);
        }

        private static SymbolDocumentInfo CreateSymbolDocument(string sourceFile)
        {
            const string PdbFileExtension = ".pdb";
            return Expression.SymbolDocument(Path.ChangeExtension(sourceFile, PdbFileExtension), Language, Vendor, SymbolDocumentType);
        }

        /// <summary>
        /// Compiles source file.
        /// </summary>
        /// <param name="sourceFile">The path to the script file.</param>
        /// <param name="options">Compiler options.</param>
        /// <param name="emitSymbolInfo">Specifies that the debug information should be emitted.</param>
        /// <returns>Compiled script code.</returns>
        public static Func<IScriptObject, string[], dynamic> Compile(string sourceFile, ScriptCompilerOptions options, bool emitSymbolInfo)
        {
            var assembly = new DScriptAssembly(options, emitSymbolInfo);
            var symbolDocument = emitSymbolInfo ? CreateSymbolDocument(sourceFile) : null;
            var internPoolSize = 0;
            var scriptImplementation = CompileSourceCode(sourceFile, symbolDocument, options, out internPoolSize);
            return new PreparedScript(assembly.Compile(scriptImplementation, emitSymbolInfo ? DebugInfoGenerator.CreatePdbGenerator() : null), emitSymbolInfo, internPoolSize);
        }

        private static ScriptInvoker PrepareScript(IEnumerator<char> scriptCode)
        {
            var internPoolSize = 0;
            return CompileSourceCode(scriptCode, null, out internPoolSize).Compile();
        }

        /// <summary>
        /// Compiles the specified script code for inline execution.
        /// </summary>
        /// <param name="scriptCode"></param>
        /// <returns></returns>
        public static ScriptInvoker PrepareScript(IEnumerable<char> scriptCode)
        {
            if (scriptCode == null) throw new ArgumentNullException("scriptCode");
            using (var enumerator = scriptCode.GetEnumerator())
                return PrepareScript(enumerator);
        }

        internal static Func<IEnumerator<char>, ScriptInvoker> OnFlyCompiler
        {
            get { return PrepareScript; }
        }

        internal static ScriptInvoker Compile(IEnumerable<CodeStatement> statements)
        {
            var invokerExpr = LinqExpressionTranslator.Inject(statements);
            return invokerExpr.Compile();
        }

        internal static IScriptObject Run(IEnumerable<CodeStatement> statements, InterpreterState state)
        {
            var invoker = LinqExpressionTranslator.Inject(statements).Compile();
            return invoker.Invoke(state);
        }

        internal static IScriptObject Run(CodeStatement stmt, InterpreterState state)
        {
            return Run(new[] { stmt }, state);
        }

        internal static IScriptObject Run(ScriptCodeExpression expr, InterpreterState state)
        {
            return Run(new ScriptCodeReturnStatement { Value = expr }, state);
        }
    }
}
