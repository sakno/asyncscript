using System;
using System.Reflection.Emit;
using System.Reflection;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Resources = Properties.Resources;
    using SystemEnvironment = System.Environment;

    /// <summary>
    /// Defines native implementation of the DynamicScript program inside of the assembly.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class CompiledScriptAttribute : Attribute
    {
        private const string InvokeMethod = "Invoke";
        /// <summary>
        /// Represents default name of the method that implements DynamicScript program.
        /// </summary>
        public const string DefaultScriptMethod = "Run";

        private readonly Type m_scriptType;
        private string m_scriptMethod;

        /// <summary>
        /// Collection of file extensions that represents compiled scripts.
        /// </summary>
        public static readonly ReadOnlyCollection<string> CompiledScriptExtensions = new ReadOnlyCollection<string>(new[] { ".exe", ".dll" });

        /// <summary>
        /// Initializes a new instance of the attribute.
        /// </summary>
        /// <param name="scriptType">The type that contains script implementation. Cannot be <see langword="null"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="scriptType"/> is <see langword="null"/>.</exception>
        public CompiledScriptAttribute(Type scriptType)
        {
            if (scriptType == null) throw new ArgumentNullException("scriptType");
            m_scriptType = scriptType;
        }

        private static IEqualityComparer<string> ExtensionComparer
        {
            get
            {
                switch (SystemEnvironment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                    case PlatformID.Unix:
                        return StringComparer.Ordinal;
                    default:
                        return StringComparer.OrdinalIgnoreCase;
                }
            }
        }

        /// <summary>
        /// Gets type that contains script implementation.
        /// </summary>
        public Type ScriptType
        {
            get { return m_scriptType; }
        }

        /// <summary>
        /// Gets or sets name of the method that implements script logic.
        /// </summary>
        public string ScriptMethod
        {
            get { return string.IsNullOrEmpty(m_scriptMethod) ? DefaultScriptMethod : m_scriptMethod; }
            set { m_scriptMethod = value; }
        }

        private static ConstructorInfo Constructor
        {
            get { return typeof(CompiledScriptAttribute).GetConstructors()[0]; }
        }

        private static PropertyInfo ScriptMethodProperty
        {
            get { return typeof(CompiledScriptAttribute).GetProperty("ScriptMethod"); }
        }

        internal static void Emit(AssemblyBuilder assembly, Type scripType, string scriptMethod)
        {
            var builder = new CustomAttributeBuilder(Constructor, new[] { scripType }, new[] { ScriptMethodProperty }, new[] { scriptMethod });
            assembly.SetCustomAttribute(builder);
        }

        internal static void Emit(AssemblyBuilder assembly, Type scriptType, MethodInfo scriptMethod)
        {
            Emit(assembly, scriptType, scriptMethod.Name);
        }

        /// <summary>
        /// Gets return type of the script method.
        /// </summary>
        public static Type ScriptMethodReturnType
        {
            get
            {
                return typeof(ScriptInvoker).GetMethod(InvokeMethod).ReturnType;
            }
        }

        /// <summary>
        /// Gets signature of the script method.
        /// </summary>
        public static Type[] ScriptMethodParameters
        {
            get { return Array.ConvertAll<ParameterInfo, Type>(typeof(ScriptInvoker).GetMethod(InvokeMethod).GetParameters(), p => p.ParameterType); }
        }

        /// <summary>
        /// Loads script implementation using properties of the attribute.
        /// </summary>
        /// <returns>The script implementation using properties of the attribute.</returns>
        public ScriptInvoker Load()
        {
            var method = ScriptType.GetMethod(ScriptMethod, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, ScriptMethodParameters, null);
            return method != null ? Delegate.CreateDelegate(typeof(ScriptInvoker), method, false) as ScriptInvoker : null;
        }

        private static ScriptInvoker Load(Assembly asm)
        {
            var attr = Attribute.GetCustomAttribute(asm, typeof(CompiledScriptAttribute), false) as CompiledScriptAttribute;
            return attr != null ? attr.Load() : null;
        }

        /// <summary>
        /// Loads an assembly that contains compiled script.
        /// </summary>
        /// <param name="name">The name of the assembly. Cannot be <see langword="null"/>.</param>
        /// <returns>The delegate that encapsulates script logic; or <see langword="null"/> if assembly doesn't contain compiled script.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        public static ScriptInvoker Load(AssemblyName name)
        {
            if (name == null) throw new ArgumentNullException("name");
            //Loads an assembly that contains compiled script.
            return Load(Assembly.Load(name));
        }

        private static ScriptInvoker Load(string compiledScriptFile)
        {
            var extension = Path.HasExtension(compiledScriptFile) ? Path.GetExtension(compiledScriptFile) : null;
            switch (CompiledScriptExtensions.Contains(extension, ExtensionComparer))
            {
                case true:
                    try
                    {
                        return Load(AssemblyName.GetAssemblyName(compiledScriptFile));
                    }
                    catch (BadImageFormatException)
                    {
                        return null;
                    }
                    catch (FileLoadException)
                    {
                        return null;
                    }
                default: return null;
            }
        }

        internal static ScriptInvoker Load(Uri scriptLocation)
        {
            if (scriptLocation == null) throw new ArgumentNullException("scriptLocation");
            return scriptLocation.IsFile ? Load(scriptLocation.LocalPath) : null;
        }
    }
}
