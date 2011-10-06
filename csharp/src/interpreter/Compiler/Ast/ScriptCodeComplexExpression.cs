using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeComplexExpression: ScriptCodeExpression
    {
        public readonly ScriptCodeStatementCollection Body;

        private ScriptCodeComplexExpression(ScriptCodeStatementCollection body)
        {
            Body = body ?? new ScriptCodeStatementCollection();
        }

        public ScriptCodeComplexExpression(params ScriptCodeStatement[] statements)
            : this(new ScriptCodeStatementCollection(statements))
        {
        }

        internal override bool Completed
        {
            get { return true; }
        }

        public override bool Equals(ScriptCodeExpression other)
        {
            throw new NotImplementedException();
        }

        protected override Expression Restore()
        {
            throw new NotImplementedException();
        }

        internal override void Verify()
        {
            throw new NotImplementedException();
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            throw new NotImplementedException();
        }

        protected override ScriptCodeExpression Clone()
        {
            throw new NotImplementedException();
        }
    }
}
