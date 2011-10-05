using System;
using System.IO;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;
    using Lexeme = Compiler.Lexeme;
    using Stopwatch = System.Diagnostics.Stopwatch;

    [ComVisible(false)]
    sealed class InteractiveInterpreter: ICommand
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class InteractiveSession
        {
            private const string ExecuteCommand = "#exec";
            private const string ExitCommand = "#exit";
            private const string ClearCommand = "#clear";
            private const string SaveCommand = "#save";
            private const string SetArgumentsCommand = "#args";
            private const string InsertCommand = "#insert";

            public readonly TextReader In;
            public readonly TextWriter Out;
            private readonly StringBuilder SourceCode;
            private bool ContinueSession;
            private string[] Arguments;

            public InteractiveSession(TextWriter output, TextReader input)
            {
                SourceCode = new StringBuilder();
                In = input;
                Out = output;
                ContinueSession = true;
                Arguments = new string[0];
            }

            private void ExecuteScript()
            {
                var watcher = Stopwatch.StartNew();
                var result = DynamicScriptInterpreter.Run(SourceCode.ToString(), Arguments);
                watcher.Stop();
                Out.WriteLine(InteractiveModeStrings.SExecutionTime, watcher.Elapsed);
                Out.WriteLine(InteractiveModeStrings.SExecutionResult, result);
            }

            private void Clear()
            {
                Console.Clear();
                Out.WriteLine(InteractiveModeStrings.SInteractivePrompt);
                SourceCode.Clear();
            }

            private void SaveSourceCode(string outputFile)
            {
                File.WriteAllText(outputFile, SourceCode.ToString(), CharEnumerator.DefaultEncoding);
            }

            private void InsertSourceCode(string inputFile)
            {
                var sourceCode = File.ReadAllText(inputFile, CharEnumerator.DefaultEncoding);
                SourceCode.AppendLine(sourceCode);
                Out.WriteLine(sourceCode);
            }

            private void SetArguments(string args)
            {
                Arguments = args.Split(new[] { Lexeme.Comma }, StringSplitOptions.RemoveEmptyEntries);
            }

            private void ExecuteMacro(string command)
            {
                switch (command)
                {
                    case ExecuteCommand:
                        ExecuteScript(); return;
                    case ExitCommand:
                        ContinueSession = false; return;
                    case ClearCommand:
                        Clear(); return;
                    default:
                        const StringComparison Comparison = StringComparison.OrdinalIgnoreCase;
                        if (command.StartsWith(SaveCommand, Comparison))
                            SaveSourceCode(command.Replace(SaveCommand, string.Empty).TrimStart());
                        else if (command.StartsWith(InsertCommand, Comparison))
                            InsertSourceCode(command.Replace(InsertCommand, string.Empty).TrimStart());
                        else if (command.StartsWith(SetArgumentsCommand, Comparison))
                            SetArguments(command.Replace(SetArgumentsCommand, string.Empty).TrimStart());
                        return;
                }
            }

            public void Run()
            {
                while (ContinueSession)
                {
                    var command = In.ReadLine();
                    if (command.Length > 0 && command[0] == Lexeme.Diez)    //handle interpreter command
                        ExecuteMacro(command);
                    else
                        SourceCode.AppendLine(command);    //append script code
                }
            }
        }
        #endregion

        public const string CommandName = "/i";
        public const string CommandNameAlt = "-i";

        private static void RunInteractive(TextWriter output, TextReader input)
        {
            output.WriteLine(InteractiveModeStrings.SInteractivePrompt);
            var session = new InteractiveSession(output, input);
            session.Run();
        }

        int ICommand.Execute(TextWriter output, TextReader input)
        {
            RunInteractive(output, input);
            return 0;
        }
    }
}
