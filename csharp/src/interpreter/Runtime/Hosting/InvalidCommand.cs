using System;
using System.Collections.Generic;
using System.IO;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Resources = Properties.Resources;
    
    [ComVisible(false)]
    sealed class InvalidCommand: ICommand
    {
        public const int Success = 0;
        public const int InvalidFunction = 1;
        public const int FileNotFound = 2;
        private readonly string m_error;
        private readonly int m_exitCode;

        private InvalidCommand(int exitCode, string error, params object[] args)
        {
            m_exitCode = exitCode == Success ? InvalidFunction : exitCode;
            m_error = String.Format(error ?? String.Empty, args);
        }

        public InvalidCommand(string error, params object[] args)
            : this(InvalidFunction, error, args)
        {

        }

        public InvalidCommand()
            : this(Resources.InvalidCommandParameters)
        {
        }

        public static readonly InvalidCommand InvalidCommandParameters = new InvalidCommand();

        public static readonly InvalidCommand AssemblyNameExpected = new InvalidCommand(Resources.AssemblyNameExpected);

        public static readonly InvalidCommand AssemblyVersionExpected = new InvalidCommand(Resources.AssemblyVersionExpected);

        public static readonly InvalidCommand ScriptFileNameExpected = new InvalidCommand(Resources.ScriptFileNameExpected);

        public static readonly InvalidCommand AssemblyInfoExpected = new InvalidCommand(Resources.AssemblyInfoExpected);

        public int Execute(TextWriter output, TextReader input)
        {
            if (output == null) throw new ArgumentNullException("output");
            output.WriteLine(m_error);
            return m_exitCode;
        }
    }
}
