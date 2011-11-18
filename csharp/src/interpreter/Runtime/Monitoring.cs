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
        /// <summary>
        /// Enables monitoring.
        /// </summary>
        public static void Enable()
        {
            switch (MonoRuntime.Available)
            {
                case true: IsEnabled = true; return;
                default: AppDomain.MonitoringIsEnabled = IsEnabled = true; return;
            }
        }

        /// <summary>
        /// Gets a value indicating that the monitoring is enabled.
        /// </summary>
        public static bool IsEnabled { get; private set; }

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
