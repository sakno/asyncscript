using System;

namespace DynamicScript.Compiler
{
    using CodeLinePragma = System.CodeDom.CodeLinePragma;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents advanced information about statement location in the code.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    public sealed class ScriptDebugInfo : CodeLinePragma
    {
        /// <summary>
        /// Gets or sets line number of the statement beginning.
        /// </summary>
        public int StartLine
        {
            get { return LineNumber; }
            set { LineNumber = value; }
        }

        /// <summary>
        /// Gets or sets column number of the statement beginning.
        /// </summary>
        public int StartColumn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets line number of the statement ending.
        /// </summary>
        public int EndLine
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets column number of the statement ending.
        /// </summary>
        public int EndColumn
        {
            get;
            set;
        }

        internal Lexeme.Position Start
        {
            set
            {
                StartLine = value.Line;
                StartColumn = value.Column;
            }
            get { return new Lexeme.Position(StartLine, StartColumn); }
        }

        internal Lexeme.Position End
        {
            set
            {
                EndLine = value.Line;
                EndColumn = value.Column;
            }
            get { return new Lexeme.Position(EndLine, EndColumn); }
        }
    }
}
