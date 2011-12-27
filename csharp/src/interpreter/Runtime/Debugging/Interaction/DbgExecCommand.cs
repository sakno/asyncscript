using System;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Debugging.Interaction
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using LexemeAnalyzer = Compiler.LexemeAnalyzer;
    using DScriptIO = Hosting.DynamicScriptIO;
    using LinqExpressionTranslator = Compiler.Ast.Translation.LinqExpressions.LinqExpressionTranslator;
    using ScriptCompositeObject = Environment.ScriptCompositeObject;

    [ComVisible(false)]
    sealed class DbgExecCommand: IDebuggerCommand
    {
        private readonly ScriptInvoker m_compiled;

        private DbgExecCommand(ScriptInvoker compiled)
        {
            m_compiled = compiled;
        }

        public static string Help
        {
            get { return DebuggerStrings.ExecCommand; }
        }


        public bool Execute(IScriptDebuggerSession session, BreakPointReachedEventArgs bp)
        {
            if(CallStack.Current!=null)
            try
            {
                var rtdbg = new KeyValuePair<string, IStaticRuntimeSlot>(RTDebuggerObject.Name, RTDebuggerObject.Storage);
                var result = m_compiled.Invoke(bp.State.Update(new ScriptCompositeObject(CallStack.Current.Select(slot => new KeyValuePair<string, IStaticRuntimeSlot>(slot.Key, slot.Value)).Concat(new[] { rtdbg }))));
                if (!ScriptCompositeObject.IsVoid(result))
                    DScriptIO.WriteLine(result);
            }
            catch (Exception e)
            {
#if DEBUG
                DScriptIO.Output.WriteLine(DebuggerStrings.DebuggerError, e);
#else
                DScriptIO.Output.WriteLine(DebuggerStrings.DebuggerError, e.Message);
#endif
            }
            return true;
        }

        public static IDebuggerCommand Parse(LexemeAnalyzer lexer)
        {
            try
            {
                var jittedCode = LinqExpressionTranslator.Inject(lexer);
                return new DbgExecCommand(jittedCode.Compile());
            }
            catch (Exception e)
            {
                DScriptIO.Output.WriteLine(DebuggerStrings.DebuggerError, e.Message);
                return DbgCommandStub.Instance;
            }
        }
    }
}
