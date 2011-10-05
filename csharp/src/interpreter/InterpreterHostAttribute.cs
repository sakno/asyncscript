using System;

namespace DynamicScript
{
    using PEFileKinds = System.Reflection.Emit.PEFileKinds;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Assembly = System.Reflection.Assembly;

    /// <summary>
    /// Marks an assembly as DynamicScript interpreter host.
    /// This class cannot be inherited.
    /// </summary>
    
    [ComVisible(false)]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class InterpreterHostAttribute : Attribute
    {
        private const PEFileKinds DefaultHostType = PEFileKinds.Dll;
        private readonly PEFileKinds m_host;

        /// <summary>
        /// Initializes a new instance of the attribute.
        /// </summary>
        /// <param name="hostType">The type of the interpreter host.</param>
        public InterpreterHostAttribute(PEFileKinds hostType)
        {
            m_host = hostType;
        }

        /// <summary>
        /// Gets host type.
        /// </summary>
        public PEFileKinds HostType
        {
            get { return m_host; }
        }

        /// <summary>
        /// Returns host type of the entry assembly.
        /// </summary>
        /// <returns></returns>
        public static PEFileKinds GetHostType()
        {
            var attr = Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(InterpreterHostAttribute)) as InterpreterHostAttribute;
            return attr != null ? attr.HostType : DefaultHostType;
        }
    }
}
