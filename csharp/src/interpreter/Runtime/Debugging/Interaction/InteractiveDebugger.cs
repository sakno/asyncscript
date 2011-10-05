using System;

namespace DynamicScript.Runtime.Debugging.Interaction
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using QScriptIO = Hosting.DynamicScriptIO;
    using Thread = System.Threading.Thread;
    using Compiler;

    /// <summary>
    /// Represents interactive script debugger based on the standard I/O.
    /// </summary>
    [ComVisible(false)]
    static class InteractiveDebugger
    {
        private static readonly string HelpCommandLiteral = new string(new[] { Lexeme.Question });
        private static readonly Token ExecCommandLiteral = new NameToken("exec");

        private static IDebuggerCommand Parse(string command)
        {
            var lexer = new LexemeAnalyzer(command);
            try
            {
                while (lexer.MoveNext())
                    if (lexer.Current.Value == Keyword.Continue)
                        return new DbgContinueCommand();
                    else if (lexer.Current.Value == Keyword.Leave)
                        return new DbgLeaveCommand();
                    else if (lexer.Current.Value == HelpCommandLiteral)
                        return new DbgHelpCommand();
                    else if (lexer.Current.Value == ExecCommandLiteral)
                        return DbgExecCommand.Parse(lexer);
            }
            catch (CodeAnalysisException e)
            {
                QScriptIO.Output.WriteLine(DebuggerStrings.DebuggerError, e.Message);
                return DbgCommandStub.Instance;
            }
            finally
            {
                lexer.Dispose();
            }
            return null;
        }

        private static bool RunDebuggerCommand(IScriptDebuggerSession session, BreakPointReachedEventArgs bp, IDebuggerCommand command)
        {
            switch (command != null)
            {
                case true:
                    return command.Execute(session, bp);
                default:
                    QScriptIO.Output.WriteLine(DebuggerStrings.BadCommand);
                    return true;
            }
        }

        /// <summary>
        /// Interprets debugger command.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="e"></param>
        /// <param name="command"></param>
        /// <returns><see langword="true"/> to continue interactive mode; otherwise, <see langword="false"/>.</returns>
        private static bool RunDebuggerCommand(IScriptDebuggerSession session, BreakPointReachedEventArgs e, string command)
        {
            return RunDebuggerCommand(session, e, Parse(command));
        }
 
        private static void Interaction(IScriptDebuggerSession session, BreakPointReachedEventArgs e)
        {
            do
            {
                //prints prompt
                QScriptIO.Output.Write(DebuggerStrings.Prompt);
            } while (RunDebuggerCommand(session, e, QScriptIO.Input.ReadLine()));
        }

        private static void BreakPointReached(object sender, BreakPointReachedEventArgs e)
        {
            var session = (IScriptDebuggerSession)sender;
            //prints notification
            QScriptIO.Output.WriteLine(DebuggerStrings.BreakPointReached, 
                CallStack.Current, 
                session.MainThread.ManagedThreadId==e.SourceThread.ManagedThreadId ? DebuggerStrings.MainThread : e.SourceThread.ManagedThreadId.ToString("X"), 
                e.Comment);
            Interaction(session, e);
        }


        private static void DebugStarted(object sender, DebuggingStartedEventArgs e)
        {
            e.DebuggerSession.BreakPointReached += BreakPointReached;
        }

        /// <summary>
        /// Gets debugger hook.
        /// </summary>
        public static EventHandler<DebuggingStartedEventArgs> Hook
        {
            get { return new EventHandler<DebuggingStartedEventArgs>(DebugStarted); }
        }
    }
}
