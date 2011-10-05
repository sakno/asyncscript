using System;
using System.Collections.Generic;
using System.IO;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Resources = Properties.Resources;

    /// <summary>
    /// Represents parser of DynamicScript interpreter command line.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    public sealed class CommandLineParser
    {
        /// <summary>
        /// Provides command line parsing.
        /// </summary>
        /// <param name="args">Set of command line arguments.</param>
        /// <returns></returns>
        public static IEnumerable<ICommand> Parse(IEnumerable<string> args)
        {
            if(args == null)args = new string[0];
            using (var enumerator = args.GetEnumerator())
                return Parse(enumerator);
        }

        private static IEnumerable<ICommand> Parse(IEnumerator<string> args)
        {
            switch (args.MoveNext())
            {
                case true:
                    do
                        switch (args.Current)
                        {
                            //compilation command
                            case CompileToAssemblyCommand.CommandName:
                            case CompileToAssemblyCommand.CommandNameAlt:
                                yield return CompileToAssemblyCommand.Parse(args) ?? new InvalidCommand();
                                continue;
                            //execute script.
                            case ExecuteScriptCommand.CommandName:
                            case ExecuteScriptCommand.CommandNameAlt:
                                yield return ExecuteScriptCommand.Parse(args) ?? new InvalidCommand();
                                continue;
                            //browse script metadata
                            case BrowseScriptMetadataCommand.CommandName:
                            case BrowseScriptMetadataCommand.CommandNameAlt:
                                yield return BrowseScriptMetadataCommand.Parse(args) ?? new InvalidCommand();
                                continue;
                            //interactive interpreter
                            case InteractiveInterpreter.CommandName:
                            case InteractiveInterpreter.CommandNameAlt:
                                yield return new InteractiveInterpreter();
                                continue;
                            //Invalid arg
                            default: yield return new InvalidCommand(Resources.InvalidCommandLineArg, args.Current); continue;
                        } while (args.MoveNext());
                    break;
                default: yield return new ShowHelpCommand(); break;
            }
        }

        /// <summary>
        /// Represents input stream.
        /// </summary>
        public readonly TextReader In;

        /// <summary>
        /// Represents output stream.
        /// </summary>
        public readonly TextWriter Out;

        /// <summary>
        /// Initializes a new command line parser.
        /// </summary>
        /// <param name="output">The parser output. Cannot be <see langword="null"/>.</param>
        /// <param name="input">The parser input stream. Cannot be <see langword="null"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="output"/> or <paramref name="input"/> is <see langword="null"/>.</exception>
        public CommandLineParser(TextWriter output, TextReader input)
        {
            if (output == null) throw new ArgumentNullException("output");
            if (input == null) throw new ArgumentNullException("input");
            Out = output;
            In = input;
        }

        private static bool Success(int exitCode)
        {
            return exitCode == 0;
        }

        /// <summary>
        /// Executes DynamicScript interpreter commands.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Command line interpretation result.</returns>
        public int Run(params string[] args)
        {
            var exitCode = 0;
            foreach (var cmd in Parse(args))
                if (!Success(exitCode = cmd.Execute(Out, In))) break;
            return exitCode;
        }
    }
}
