using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeStatement = Compiler.Ast.ScriptCodeStatement;

    /// <summary>
    /// Represents runtime representation of the statement.
    /// </summary>
    /// <typeparam name="TStatement">Type of the statement.</typeparam>
    [ComVisible(false)]
    abstract class ScriptStatement<TStatement> : ScriptObject, IScriptStatement<TStatement>, ISerializable
        where TStatement : ScriptCodeStatement
    {
        private const string StatementHolder = "Statement";
        private const string ContractHolder = "ContractBinding";
        private TStatement m_statement;

        /// <summary>
        /// Represents contract binding.
        /// </summary>
        public readonly IScriptStatementContract<TStatement> ContractBinding;

        /// <summary>
        /// Deserializes runtime representation of the statement.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ScriptStatement(SerializationInfo info, StreamingContext context)
            : this(info.GetValue<TStatement>(StatementHolder), info.GetValue < IScriptStatementContract<TStatement>>(ContractHolder))
        {
        }

        /// <summary>
        /// Initializes a new runtime representation of the code statement.
        /// </summary>
        /// <param name="statement">A code statement definition. Cannot be <see langword="null"/>.</param>
        /// <param name="contractBinding">An underlying contract binding of the creating object. Cannot be <see langword="null"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="statement"/> or <paramref name="contractBinding"/> is <see langword="null"/>.</exception>
        protected ScriptStatement(TStatement statement, IScriptStatementContract<TStatement> contractBinding)
        {
            if (statement == null) throw new ArgumentNullException("statement");
            m_statement = statement;
            ContractBinding = contractBinding;
        }

        /// <summary>
        /// Executes statement associated with this object.
        /// </summary>
        /// <param name="args">Execution parameters.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/> if this statement supports execution; otherwise, <see langword="false"/>.</returns>
        public virtual bool Execute(IList<IScriptObject> args, InterpreterState state)
        {
            return false;
        }

        TStatement IScriptCodeElement<TStatement>.CodeObject
        {
            get { return Statement; }
        }

        /// <summary>
        /// Gets or sets statement associated with this object.
        /// </summary>
        public TStatement Statement
        {
            get { return m_statement; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                m_statement = value;
            }
        }

        /// <summary>
        /// Creates a new statement definition.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected abstract TStatement CreateStatement(IList<IScriptObject> args, InterpreterState state);

        bool IScriptCodeElement<TStatement>.Modify(IList<IScriptObject> args, InterpreterState state)
        {
            var statement = CreateStatement(args, state);
            switch (statement != null)
            {
                case true:
                    Statement = statement;
                    return true;
                default:
                    return false;
            }
        }


        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue<TStatement>(StatementHolder, m_statement);
            info.AddValue<IScriptStatementContract<TStatement>>(ContractHolder, ContractBinding);
        }

        /// <summary>
        /// Returns an underlying contract binding of this object.
        /// </summary>
        /// <returns></returns>
        public sealed override IScriptContract GetContractBinding()
        {
            return ContractBinding;
        }

        /// <summary>
        /// Returns a string representation of this statement.
        /// </summary>
        /// <returns></returns>
        public sealed override string ToString()
        {
            return Statement.ToString();
        }

        private ScriptBoolean Equals(IScriptStatement<TStatement> right)
        {
            return Equals(Statement, right.CodeObject);
        }

        protected sealed override IScriptObject Equals(IScriptObject right, InterpreterState state)
        {
            return right is IScriptStatement<TStatement> ?
                Equals((IScriptStatement<TStatement>)right) :
                ScriptBoolean.False;
        }

        protected sealed override IScriptObject NotEquals(IScriptObject right, InterpreterState state)
        {
            return right is IScriptStatement<TStatement> ?
                !Equals((IScriptStatement<TStatement>)right) :
                ScriptBoolean.True;
        }
    }
}
