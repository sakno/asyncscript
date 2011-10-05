using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents script metadata extraction command.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class BrowseScriptMetadataCommand: ICommand
    {
        public const string CommandName = "-b";
        public const string CommandNameAlt = "/b";

        public int Execute(TextWriter output, TextReader input)
        {
            if (File.Exists(ScriptFile))
                try
                {
                    //Execute script program
                    IScriptObject result = DynamicScriptInterpreter.Run(ScriptFile, false, new string[0]);
                    var contract = result.GetContractBinding().ToString();
                    switch (string.IsNullOrEmpty(OutputFile))
                    {
                        case true:
                            output.Write(contract);
                            break;
                        default:
                            File.WriteAllText(OutputFile, contract, Encoding.Unicode);
                            break;
                    }
                    return InvalidCommand.Success;
                }
                catch (Exception e)
                {
                    output.Write(e.Message);
                    return InvalidCommand.InvalidFunction;
                }
            else return InvalidCommand.FileNotFound;
        }

        /// <summary>
        /// Sets script file to be explored.
        /// </summary>
        public string ScriptFile
        {
            private get;
            set;
        }

        public string OutputFile
        {
            set;
            private get;
        }

        public static ICommand Parse(IEnumerator<string> args)
        {
            switch (args.MoveNext())
            {
                case true:
                    var command = new BrowseScriptMetadataCommand { ScriptFile = args.Current };
                    if (args.MoveNext()) command.OutputFile = args.Current;
                    return command;
                default:
                    return InvalidCommand.ScriptFileNameExpected;
            }
        }
    }
}
