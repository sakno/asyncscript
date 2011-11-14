using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents diagnostics functionality of running scripts.
    /// </summary>
    [ComVisible(false)]
    public static class Monitoring
    {
        private static bool m_enabled = false;

        /// <summary>
        /// Enables monitoring.
        /// </summary>
        public static void Enable()
        {
            switch (MonoRuntime.Available)
            {
                case true: m_enabled = true; return;
                default: AppDomain.MonitoringIsEnabled = m_enabled = true; return;
            }
        }

        /// <summary>
        /// Gets a value indicating that the monitoring is enabled.
        /// </summary>
        public static bool IsEnabled
        {
            get { return MonoRuntime.Available ? m_enabled : AppDomain.MonitoringIsEnabled; }
        }

        /// <summary>
        /// Gets the total size, in bytes, of all memory allocations that have been made by the application
        /// domain in which script is executed.
        /// </summary>
        public static long AllocatedMemory
        {
            get { return IsEnabled ? AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize : -1; }
        }

        /// <summary>
        /// Gets the number of bytes that survived the last full, blocking collection and that are known to be
        /// referenced by executed script.
        /// </summary>
        public static long SurvivedMemory
        {
            get { return IsEnabled ? AppDomain.CurrentDomain.MonitoringSurvivedMemorySize : -1; }
        }

        /// <summary>
        /// Gets total processor time that has been used by all threads while executing the script.
        /// </summary>
        public static long ProcessorTime
        {
            get { return IsEnabled ? (long)AppDomain.CurrentDomain.MonitoringTotalProcessorTime.TotalMilliseconds : -1; }
        }
    }
}
