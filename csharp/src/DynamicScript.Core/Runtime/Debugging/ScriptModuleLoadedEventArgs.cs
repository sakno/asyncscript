using System;

namespace DynamicScript.Runtime.Debugging
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents data of the event occured when script module is loaded.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    public sealed class ScriptModuleLoadedEventArgs: EventArgs
    {
        private readonly Uri m_module;

        internal ScriptModuleLoadedEventArgs(Uri moduleLocation)
        {
            if (moduleLocation == null) throw new ArgumentNullException("moduleLocation");
            m_module = moduleLocation;
        }

        /// <summary>
        /// Gets location of the loaded module.
        /// </summary>
        public Uri ModuleLocation
        {
            get { return m_module; }
        }
    }
}
