using System;
using System.Collections.Generic;
using System.IO;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Comment = Compiler.Comment;

    [ComVisible(false)]
    sealed class ExecuteScriptCommand : ICommand
    {
        public const string CommandName = "/e";
        public const string CommandNameAlt = "-e";
        //debug command
        private const string DebugCommandName = "/debug";
        private const string DebugCommandNameAlt = "-debug";

        private readonly List<string> m_args;

        private ExecuteScriptCommand()
        {
            m_args = new List<string>(10);
        }

        public ICollection<string> Arguments
        {
            get { return m_args; }
        }

        public bool EmitDebugInfo
        {
            set;
            private get;
        }

        public string ScriptFile
        {
            set;
            private get;
        }

        public int Execute(TextWriter output, TextReader input)
        {
            if (File.Exists(ScriptFile))
#if DEBUG
            {
                //Execute script program
                var result = DynamicScriptInterpreter.Run(ScriptFile, EmitDebugInfo, m_args.ToArray());
                return InvalidCommand.Success;
            }
#else
            try
            {
                //Execute script program
                var result = DynamicScriptInterpreter.Run(ScriptFile, EmitDebugInfo, m_args.ToArray());
                return InvalidCommand.Success;
            }
            catch (Exception e)
            {
                output.WriteLine(e.Message);
                return InvalidCommand.InvalidFunction;
            }
#endif
            else return InvalidCommand.FileNotFound;
        }

        public static ICommand Parse(IEnumerator<string> args)
        {
            switch (args.MoveNext())
            {
                case true:
                    var command = new ExecuteScriptCommand { ScriptFile = args.Current };
                    while (args.MoveNext()) switch (args.Current)
                        {
                            case DebugCommandName:
                            case DebugCommandNameAlt:
                                command.EmitDebugInfo = true;
                                continue;
                            default: command.Arguments.Add(args.Current); 
                                continue;
                        }
                    return command;
                default:
                    return InvalidCommand.ScriptFileNameExpected;
            }
        }
    }
}
