using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents variable declaration statement.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeVariableDeclaration: ScriptCodeStatement, ISlot, IEquatable<ScriptCodeVariableDeclaration>
    {
        /// <summary>
        /// Represents default variable type that is assigned to the variable that doesn't explicit defines
        /// init expression and type.
        /// </summary>
        public static readonly ScriptCodeExpression DefaultVariableType = ScriptCodeSuperContractExpression.Instance;

        /// <summary>
        /// Represents default variable value.
        /// </summary>
        public static readonly CodeExpression DefaultVariableValue = ScriptCodeVoidExpression.Instance;

        private ScriptCodeExpression m_binding;
        private ScriptCodeExpression m_init;
        private string m_name;
        private bool m_const;

        /// <summary>
        /// Initializes a new variable declaration
        /// </summary>
        public ScriptCodeVariableDeclaration()
        {
            m_const = false;
        }

        internal ScriptCodeVariableDeclaration(ISlot slotdef)
            : this()
        {
            m_name = slotdef.Name;
            m_binding = slotdef.ContractBinding;
            m_init = slotdef.InitExpression;
        }

        /// <summary>
        /// Initializes a new variable declaration.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isConst"></param>
        /// <param name="initExpr"></param>
        /// <param name="contract"></param>
        public ScriptCodeVariableDeclaration(string name, bool isConst = false, ScriptCodeExpression initExpr = null, ScriptCodeExpression contract = null)
            : this()
        {
            m_name = name;
            m_const = isConst;
            m_binding = contract;
            m_init = initExpr;
        }

        /// <summary>
        /// Gets or sets a value indicating that the value of the declared variable cannot be changed.
        /// </summary>
        public bool IsConst
        {
            get { return m_const; }
            set { m_const = value; }
        }

        /// <summary>
        /// Gets or sets variable name.
        /// </summary>
        public string Name
        {
            get { return m_name ?? string.Empty; }
            set {  m_name = value; }
        }

        /// <summary>
        /// Gets or sets initialization expression.
        /// </summary>
        public ScriptCodeExpression InitExpression
        {
            get { return m_init; }
            set { m_init = value; }
        }

        SlotDeclarationStyle ISlot.Style
        {
            get { return Style; }
        }

        internal SlotDeclarationStyle Style
        {
            get
            {
                return (ContractBinding != null ? SlotDeclarationStyle.ContractBindingOnly : SlotDeclarationStyle.Unknown) | (InitExpression != null ? SlotDeclarationStyle.InitExpressionOnly : SlotDeclarationStyle.Unknown);
            }
        }

        /// <summary>
        /// Gets or sets variable type expression.
        /// </summary>
        public ScriptCodeExpression ContractBinding
        {
            get 
            {
                if (IsConst && m_binding == null && m_init is IStaticContractBinding<ScriptCodeExpression>)
                    return ((IStaticContractBinding<ScriptCodeExpression>)m_init).Contract;
                else if (m_binding == null && m_init == null)
                    return DefaultVariableType;
                else return m_binding; 
            }
            set { m_binding = value; }
        }

        /// <summary>
        /// Returns type code of this variable.
        /// </summary>
        /// <returns></returns>
        public ScriptTypeCode GetTypeCode()
        {
            return ContractBinding is IWellKnownContractInfo ? ((IWellKnownContractInfo)ContractBinding).GetTypeCode() : ScriptTypeCode.Unknown;
        }

        internal static ScriptCodeVariableDeclaration Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Punctuation[] terminator)
        {
            if (terminator == null) terminator = new Punctuation[0];
            if (terminator.Length == 0) terminator = new[] { Punctuation.Semicolon };
            var initialPosition = lexer.Current.Key;
            var declaration = new ScriptCodeVariableDeclaration { IsConst = lexer.Current.Value == Keyword.HashCodes.lxmConst };
            var variableName = String.Empty;
            var initExpression = default(ScriptCodeExpression);
            var typeExpression = default(ScriptCodeExpression);
            //Parse variable name, initialization expression and contract binding.
            if (!Parser.ParseSlot(lexer, out variableName, out initExpression, out typeExpression, terminator))
                throw CodeAnalysisException.IdentifierExpected(lexer.Current.Key);
            declaration.Name = variableName;
            declaration.InitExpression = initExpression;
            declaration.ContractBinding = typeExpression;
            switch (declaration.IsConst && declaration.InitExpression == null)
            {
                case true:
                    throw CodeAnalysisException.UnitializedConstant(initialPosition);
                default:
                    return declaration;
            }
        }

        /// <summary>
        /// Returns the source code that represents variable declaration.
        /// </summary>
        /// <returns>The source code that represents variable declaration.</returns>
        public override string ToString()
        {
            return ToString(false);
        }

        internal override bool Completed
        {
            get { return IsConst ? InitExpression != null : InitExpression != null || ContractBinding != null; }
        }

        bool IEquatable<ISlot>.Equals(ISlot other)
        {
            return Equals(other as ScriptCodeVariableDeclaration);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeVariableDeclaration other)
        {
            return other != null &&
                IsConst == other.IsConst &&
                Equals(InitExpression, other.InitExpression) &&
                Equals(ContractBinding, other.ContractBinding);
        }

        internal string ToString(bool omitSemicolon)
        {
            var result = String.Concat(IsConst ? Keyword.Const : Keyword.Var, ' ', Name,
                InitExpression != null ? String.Concat(Operator.Assignment, InitExpression) : String.Empty,
                ContractBinding != null ? String.Concat(Punctuation.Colon, ContractBinding) : String.Empty);
            return omitSemicolon ? result : string.Concat(result, Punctuation.Semicolon);
        }

        internal override ScriptCodeStatement Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            if (InitExpression != null) InitExpression = InitExpression.Visit(this, visitor) as ScriptCodeExpression;
            if (ContractBinding != null) ContractBinding = ContractBinding.Visit(this, visitor) as ScriptCodeExpression;
            return visitor.Invoke(this) as ScriptCodeStatement;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeStatement Clone()
        {
            return new ScriptCodeVariableDeclaration
            {
                Name = this.Name,
                ContractBinding = Extensions.Clone(ContractBinding),
                InitExpression = Extensions.Clone(InitExpression),
                IsConst = this.IsConst,
                LinePragma = this.LinePragma
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(ScriptCodeStatement other)
        {
            return Equals(other as ScriptCodeVariableDeclaration);
        }

        /// <summary>
        /// Restores an expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<string, bool, ScriptCodeExpression, ScriptCodeExpression, ScriptCodeVariableDeclaration, NewExpression>((name, isconst, init, contract) => new ScriptCodeVariableDeclaration(name, isconst, init, contract));
            return ctor.Update(new[] { LinqHelpers.Constant(Name), LinqHelpers.Constant(IsConst), LinqHelpers.Restore(InitExpression), LinqHelpers.Restore(ContractBinding) });
        }
    }
}
