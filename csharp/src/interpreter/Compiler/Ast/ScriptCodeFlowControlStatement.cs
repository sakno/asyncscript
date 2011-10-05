using System;
using System.CodeDom;
using System.Collections.Generic;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using LinqExpression = System.Linq.Expressions.Expression;

    /// <summary>
    /// Represents an abstract class that represents flow control statement.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public abstract class ScriptCodeFlowControlStatement : ScriptCodeStatement, ICodeFlowControlInstruction, IEnumerable<ScriptCodeExpression>, IEquatable<ScriptCodeFlowControlStatement>
    {
        private ScriptCodeExpressionCollection m_argList;
        private readonly Keyword m_token;

        internal ScriptCodeFlowControlStatement(Keyword token, ScriptCodeExpressionCollection arguments)
        {
            if (token == null) throw new ArgumentNullException("token");
            m_token = token;
            m_argList = arguments;
        }

        internal Keyword Value
        {
            get { return m_token; }
        }

        internal void Add(ScriptCodeExpression expr)
        {
            ArgList.Add(expr);
        }

        /// <summary>
        /// Gets argument list of the flow control statement.
        /// </summary>
        public virtual ScriptCodeExpressionCollection ArgList
        {
            get 
            {
                if (m_argList == null) m_argList = new ScriptCodeExpressionCollection();
                return m_argList;
            }
        }

        /// <summary>
        /// Returns a string that represents flow control statement.
        /// </summary>
        /// <returns>The string that represents flow control statement.</returns>
        public sealed override string ToString()
        {
            return string.Concat(Value, Lexeme.WhiteSpace, ArgList, Punctuation.Semicolon);
        }

        /// <summary>
        /// Determines whether this statement are equal to other statement.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ScriptCodeFlowControlStatement other)
        {
            return other != null &&
                Equals(Value, other.Value) &&
                ScriptCodeExpressionCollection.TheSame(ArgList, other.ArgList);
        }

        /// <summary>
        /// Determines whether this statement are equal to other statement.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public sealed override bool Equals(ScriptCodeStatement other)
        {
            return Equals(other as ScriptCodeFlowControlStatement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ScriptCodeExpression> GetEnumerator()
        {
            return ArgList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
