using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    sealed class ScriptSyntaxTreeVisitor
    {
        private readonly IScriptAction m_visitor;
        private readonly InterpreterState m_state;

        public ScriptSyntaxTreeVisitor(IScriptAction visitor, InterpreterState state)
        {
            m_visitor = visitor;
            m_state = state;
        }

        private ISyntaxTreeNode Visit(ISyntaxTreeNode node)
        {
            var n = default(IScriptObject);
            switch (m_visitor != null && ScriptObject.TryConvert(node, out n))
            {
                case true:

                    var visited = m_visitor.Invoke(new[] { n }, m_state);
                    if (visited is IScriptCodeElement<ScriptCodeExpression>)
                        return ((IScriptCodeElement<ScriptCodeExpression>)visited).CodeObject;
                    else if (visited is IScriptCodeElement<ScriptCodeStatement>)
                        return ((IScriptCodeElement<ScriptCodeStatement>)visited).CodeObject;
                    else return node;
                default: return node;
            }
        }

        public static implicit operator Converter<ISyntaxTreeNode, ISyntaxTreeNode>(ScriptSyntaxTreeVisitor visitor)
        {
            return visitor != null ? new Converter<ISyntaxTreeNode, ISyntaxTreeNode>(visitor.Visit) : null;
        }
    }
}
