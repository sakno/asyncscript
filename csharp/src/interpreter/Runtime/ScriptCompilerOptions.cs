using System;
using System.Collections.Generic;
using System.Reflection;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CompilerOptions = Microsoft.Scripting.CompilerOptions;
    using QScriptModule = Environment.ObjectModel.ScriptModule;


    /// <summary>
    /// Represents DynamicScript compiler options. This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class ScriptCompilerOptions: CompilerOptions
    {
        private const string DefaultAssemblyName = "qscript";
        private bool m_toAssembly;
        private IScriptObject m_global;
        private Version m_version;
        private string m_name;
        private bool m_executable;
        private PortableExecutableKinds m_kind;
        private ImageFileMachine m_im;

        /// <summary>
        /// Initializes a new instance of the DynamicScript compiler options.
        /// </summary>
        public ScriptCompilerOptions()
        {
            m_toAssembly = false;
            m_global = null;
            m_executable = true;
            typeof(DynamicScriptException).Assembly.ManifestModule.GetPEKind(out m_kind, out m_im);
        }

        /// <summary>
        /// Gets or sets generated image type.
        /// </summary>
        public ImageFileMachine ImageType
        {
            get { return m_im; }
            set { m_im = value; }
        }

        /// <summary>
        /// Gets or sets generated PE file kind.
        /// </summary>
        public PortableExecutableKinds PEKind
        {
            get { return m_kind; }
            set { m_kind = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating that the program shoudl be compiled to the assembly file.
        /// </summary>
        public bool CompileToAssembly
        {
            get { return m_toAssembly; }
            set { m_toAssembly = value; }
        }

        /// <summary>
        /// Gets or sets output assembly version.
        /// </summary>
        public Version AssemblyVersion
        {
            get { return m_version ?? new Version(1, 0, 0, 0); }
            set { m_version = value; }
        }

        /// <summary>
        /// Gets or sets output assembly name.
        /// </summary>
        public string AssemblyName
        {
            get { return string.IsNullOrEmpty(m_name) ? DefaultAssemblyName : m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// Gets or sets additional version information about assembly.
        /// </summary>
        public AssemblyVersionResource AssemblyInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating that the compiled assembly is an executable assembly.
        /// </summary>
        public bool Executable
        {
            get { return m_executable; }
            set { m_executable = value; }
        }

        /// <summary>
        /// Gets or sets global object.
        /// </summary>
        public IScriptObject Global
        {
            get { return m_global ?? new QScriptModule(); }
            set { m_global = value; }
        }

        /// <summary>
        /// Creates clone of the compiler options. 
        /// </summary>
        /// <returns>The clone of the compiler options.</returns>
        public override object Clone()
        {
            return new ScriptCompilerOptions
            {
                CompileToAssembly = this.CompileToAssembly,
                Global = this.Global
            };
        }

        /// <summary>
        /// Gets default compiler options.
        /// </summary>
        public static ScriptCompilerOptions Default
        {
            get { return new ScriptCompilerOptions(); }
        }
    }
}
