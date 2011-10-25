using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;
    using IEnumerable = System.Collections.IEnumerable;   

    /// <summary>
    /// Represents action contract expression.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeActionContractExpression: ScriptCodeExpression, IStaticContractBinding<ScriptCodeCallableContractExpression>, IEquatable<ScriptCodeActionContractExpression>
    {
        #region Nested Types

        /// <summary>
        /// Represents action parameter.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public struct Parameter : ISlot, IEquatable<Parameter>, IRestorable
        {
            /// <summary>
            /// Represents name of the parameter.
            /// </summary>
            public readonly string Name;
            private ScriptCodeExpression m_defval;
            private ScriptCodeExpression m_binding;
            private int? m_hash;

            /// <summary>
            /// Initializes a new parameter.
            /// </summary>
            /// <param name="name">The name of the parameter. Cannot be <see langword="null"/> or empty.</param>
            /// <param name="defval"></param>
            /// <param name="contractBinding"></param>
            /// <exception cref="System.ArgumentNullException"><paramref name="name"/> is <see langword="null"/> or empty.</exception>
            public Parameter(string name, ScriptCodeExpression defval = null, ScriptCodeExpression contractBinding = null)
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
                Name = name;
                m_defval = m_binding = null;
                m_hash = null;
                m_binding = contractBinding;
                m_defval = defval;
            }

            internal Parameter(ISlot s)
                : this(s.Name, s.InitExpression, s.ContractBinding)
            {
            }

            string ISlot.Name
            {
                get { return Name; }
            }

            /// <summary>
            /// Gets or sets default value of the parameter.
            /// </summary>
            public ScriptCodeExpression DefaultValue
            {
                get { return m_defval; }
                set 
                { 
                    m_defval = value;
                    m_hash = null;
                }
            }

            /// <summary>
            /// Gets or sets parameter contract binding.
            /// </summary>
            public ScriptCodeExpression ContractBinding
            {
                get { return m_defval == null && m_binding == null ? ScriptCodeVariableDeclaration.DefaultVariableType : m_binding; }
                set 
                { 
                    m_binding = value;
                    m_hash = null;
                }
            }

            SlotDeclarationStyle ISlot.Style
            {
                get { return Style; }
            }

            internal SlotDeclarationStyle Style
            {
                get
                {
                    return (ContractBinding != null ? SlotDeclarationStyle.ContractBindingOnly : SlotDeclarationStyle.Unknown) | (DefaultValue != null ? SlotDeclarationStyle.InitExpressionOnly : SlotDeclarationStyle.Unknown);
                }
            }

            ScriptCodeExpression ISlot.InitExpression
            {
                get { return DefaultValue; }
            }

            /// <summary>
            /// Returns a string representation of the parameter.
            /// </summary>
            /// <returns>The string representation of the parameter.</returns>
            public override string ToString()
            {
                var builder = new StringBuilder();
                builder.Append(String.Concat(NameToken.Normalize(Name), DefaultValue != null ? String.Concat(Operator.Assignment, DefaultValue) : String.Empty));
                builder.Append(ContractBinding != null ? String.Concat(Punctuation.Colon, ContractBinding) : String.Empty);
                return builder.ToString();
            }

            /// <summary>
            /// Determines whether this parameter is equal to another.
            /// </summary>
            /// <param name="other">Other parameter to compare.</param>
            /// <returns><see langword="true"/> if this parameter is equal to another; otherwise, <see langword="false"/>.</returns>
            public bool Equals(Parameter other)
            {
                return StringEqualityComparer.Equals(Name, other.Name) &&
                    Equals(DefaultValue, other.DefaultValue) &&
                    Equals(ContractBinding, other.ContractBinding);
            }

            /// <summary>
            /// Determines whether this parameter is equal to another.
            /// </summary>
            /// <param name="other">Other parameter to compare.</param>
            /// <returns><see langword="true"/> if this parameter is equal to another; otherwise, <see langword="false"/>.</returns>
            bool IEquatable<ISlot>.Equals(ISlot other)
            {
                return other is Parameter ? Equals((Parameter)other) : false;
            }

            /// <summary>
            /// Determines whether this parameter is equal to another.
            /// </summary>
            /// <param name="other">Other parameter to compare.</param>
            /// <returns><see langword="true"/> if this parameter is equal to another; otherwise, <see langword="false"/>.</returns>
            public override bool Equals(object other)
            {
                return other is Parameter ? Equals((Parameter)other) : false;
            }

            /// <summary>
            /// Computes a hash code for this parameter.
            /// </summary>
            /// <returns>A hash code of this parameter.</returns>
            public override int GetHashCode()
            {
                if (m_hash == null) m_hash = StringEqualityComparer.GetHashCode(ToString());
                return m_hash.Value;
            }

            Expression IRestorable.Restore()
            {
                var ctor = LinqHelpers.BodyOf<string, ScriptCodeExpression, ScriptCodeExpression, Parameter, NewExpression>((name, def, type) => new Parameter(name, def, type));
                return ctor.Update(new[] { LinqHelpers.Constant(Name), LinqHelpers.Restore(DefaultValue), LinqHelpers.Restore(ContractBinding) });
            }

            bool ISyntaxTreeNode.Completed
            {
                get { return true; }
            }

            void ISyntaxTreeNode.Verify()
            {
            }

            internal ISyntaxTreeNode Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
            {
                if (ContractBinding != null) ContractBinding = ContractBinding.Visit(this, visitor);
                if (DefaultValue != null) DefaultValue = DefaultValue.Visit(this, visitor);
                return (Parameter)visitor.Invoke(this);
            }

            ISyntaxTreeNode ISyntaxTreeNode.Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
            {
                return Visit(parent, visitor);
            }

            object ICloneable.Clone()
            {
                return new Parameter(Name, Extensions.Clone(DefaultValue), Extensions.Clone(ContractBinding));
            }
        }

        /// <summary>
        /// Represents ordered list of parameters.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public sealed class ParameterList : Collection<Parameter>, ISyntaxTreeNode
        {
            /// <summary>
            /// Initializes a new list of parameters.
            /// </summary>
            /// <param name="parameters">An array of parameters.</param>
            public ParameterList(Parameter[] parameters)
                : this((IList<Parameter>)parameters)
            {
            }

            private ParameterList(IList<Parameter> parameters)
                : base(parameters.IsReadOnly ? new List<Parameter>(parameters) : parameters)
            {
            }

            /// <summary>
            /// Initializes a new list of parameters.
            /// </summary>
            public ParameterList()
            {
            }

            /// <summary>
            /// Adds a new parameter to the list.
            /// </summary>
            /// <param name="paramName">The name of the parameter.</param>
            /// <param name="defval">Default value of the parameter.</param>
            /// <param name="contractBinding">Parameter contract binding.</param>
            public void Add(string paramName, ScriptCodeExpression defval = null, ScriptCodeExpression contractBinding = null)
            {
                Add(new Parameter(paramName) { DefaultValue = defval, ContractBinding = contractBinding });
            }

            /// <summary>
            /// Determines whether the parameter with the specified name is already existed.
            /// </summary>
            /// <param name="parameterName">The name of the parameter.</param>
            /// <returns><see langword="true"/>if parameter with the specified name is already existed; otherwise, <see langword="false"/>.</returns>
            public bool Contains(string parameterName)
            {
                foreach (var p in Items)
                    if (StringEqualityComparer.Equals(p.Name, parameterName)) return true;
                return false;
            }

            internal static bool TheSame(ParameterList list1, ParameterList list2)
            {
                if (list1 == null) list1 = new ParameterList();
                if (list2 == null) list2 = new ParameterList();
                switch (list1.Count == list2.Count)
                {
                    case true:
                        for (var i = 0; i < list1.Count; i++)
                            if (list1[i].Equals(list2[i])) continue;
                            else return false;
                        return true;
                    default: return false;
                }
            }

            internal T[] ToArray<T>(Converter<Parameter, T> converter)
            {
                var result = new T[Count];
                for(var i=0; i<Count; i++)
                     result[i]= converter.Invoke(this[i]);
                return result;
            }

            bool ISyntaxTreeNode.Completed
            {
                get { return true; }
            }

            void ISyntaxTreeNode.Verify()
            {
            }

            internal ISyntaxTreeNode Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
            {
                for (var i = 0; i < Count; i++)
                    this[i] = ScriptCodeStatement.Visit(this, this[i], visitor, p => new Parameter(p));
                return visitor.Invoke(this);
            }

            ISyntaxTreeNode ISyntaxTreeNode.Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
            {
                return Visit(parent, visitor);
            }

            object ICloneable.Clone()
            {
                return new ParameterList(Extensions.CloneCollection(this));
            }

            Expression IRestorable.Restore()
            {
                var ctor = LinqHelpers.BodyOf<Parameter[], ParameterList, NewExpression>(p => new ParameterList(p));
                return ctor.Update(new[] { LinqHelpers.NewArray(this) });
            }
        }
        #endregion

        /// <summary>
        /// Represents a list of action parameters.
        /// </summary>
        public readonly ParameterList ParamList;
        private ScriptCodeExpression m_return;

        /// <summary>
        /// Initializes a new action signature descriptor.
        /// </summary>
        public ScriptCodeActionContractExpression()
        {
            ParamList = new ParameterList();
            m_return = null;
        }

        /// <summary>
        /// Initializes a new action signature descriptor.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="returnType"></param>
        public ScriptCodeActionContractExpression(Parameter[] parameters, ScriptCodeExpression returnType)
            :this(new ParameterList(parameters), returnType)
        {
        }

        private ScriptCodeActionContractExpression(ParameterList parameters, ScriptCodeExpression returnType)
        {
            ParamList = parameters ?? new ParameterList();
            m_return = returnType;
        }

        /// <summary>
        /// Gets a value indicating that the action should be executed asynchronously.
        /// </summary>
        public bool IsAsynchronous
        {
            get { return m_return is ScriptCodeAsyncExpression; }
        }

        /// <summary>
        /// Gets or sets return type.
        /// </summary>
        public ScriptCodeExpression ReturnType
        {
            get
            {
                switch(m_return is ScriptCodeAsyncExpression)
                {
                    case true:
                        var asyncexpr = (ScriptCodeAsyncExpression)m_return;
                        return asyncexpr.Contract == ScriptCodeVoidExpression.Instance || asyncexpr.Contract == null ? ScriptCodeSuperContractExpression.Instance : asyncexpr.Contract;
                    default: return m_return ?? ScriptCodeVariableDeclaration.DefaultVariableType;
                }
            }
            set { m_return = value; }
        }

        /// <summary>
        /// Gets a value indicatingn whether this signature doesn't describe a return value.
        /// </summary>
        public bool NoReturnValue
        {
            get { return m_return == null || m_return is ScriptCodeVoidExpression; }
        }

        /// <summary>
        /// Gets a value indicating whether this signature doesn't describe parameters and return value.
        /// </summary>
        public bool IsEmpty
        {
            get { return NoReturnValue && ParamList.Count == 0; }
        }

        /// <summary>
        /// Returns a string representation of the action signature.
        /// </summary>
        /// <returns>The string representation of the action signature.</returns>
        public override string ToString()
        {
            var signature = new string[ParamList.Count > 0 ? ParamList.Count : 1];
            switch (ParamList.Count)
            {
                case 0:
                    signature[0] = Convert.ToString(ScriptCodeVoidExpression.Instance); break;
                default:
                    for (var i = 0; i < ParamList.Count; i++)
                        signature[i] = String.Concat(ParamList[i]);
                    break;
            }
            return String.Concat(Punctuation.Dog, string.Join(Punctuation.Comma, signature), Punctuation.Arrow, ReturnType ?? ScriptCodeVariableDeclaration.DefaultVariableType);
        }

        /// <summary>
        /// Gets static contract binding.
        /// </summary>
        public ScriptCodeCallableContractExpression Contract
        {
            get { return ScriptCodeCallableContractExpression.Instance; }
        }

        internal override bool Completed
        {
            get { return ReturnType != null; }
        }

        /// <summary>
        /// Converts expression to the statement.
        /// </summary>
        /// <param name="expr">The expression to be converted.</param>
        /// <returns>The statement that encapsulates the expression.</returns>
        public static explicit operator ScriptCodeExpressionStatement(ScriptCodeActionContractExpression expr)
        {
            return new ScriptCodeExpressionStatement<ScriptCodeActionContractExpression>(expr);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeActionContractExpression);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeActionContractExpression other)
        {
            return other != null &&
                Completed &&
                other.Completed &&
                IsAsynchronous == other.IsAsynchronous &&
                ReturnType.Equals(other.ReturnType) &&
                ParameterList.TheSame(ParamList, other.ParamList);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<Parameter[], ScriptCodeExpression, ScriptCodeActionContractExpression, NewExpression>((pars, ret) => new ScriptCodeActionContractExpression(pars, ret));
            return ctor.Update(new[] { LinqHelpers.NewArray(ParamList), LinqHelpers.Restore(m_return) });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            if (ReturnType != null) ReturnType = ReturnType.Visit(this, visitor);
            ParamList.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// Creates a new deep copy of this expression.
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeActionContractExpression(Extensions.Clone(ParamList), Extensions.Clone(m_return));
        }
    }
}
