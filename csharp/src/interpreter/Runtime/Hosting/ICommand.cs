using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.IO;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents DynamicScript interpeter command.
    /// </summary>
    [ComVisible(false)]
    
    public interface ICommand
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="output">The output stream that is used to print execution results. Cannot be <see langword="null"/>.</param>
        /// <param name="input">The input stream that is used to read user data. Cannot be <see langword="null"/>.</param>
        /// <returns>The exit code returned from the command.</returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        int Execute(TextWriter output, TextReader input);
    }
}
