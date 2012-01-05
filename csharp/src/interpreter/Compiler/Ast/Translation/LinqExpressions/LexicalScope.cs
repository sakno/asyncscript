using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Runtime;
    using Interlocked = System.Threading.Interlocked;
    using DynamicMetaObject = System.Dynamic.DynamicMetaObject;

    /// <summary>
    /// Represents a lexical scope used for analyzing code document only.
    /// </summary>
    [ComVisible(false)]
    public abstract class LexicalScope: ILexicalScope, IEnumerable<KeyValuePair<string, ParameterExpression>>
    {
        #region Nested Types

        /// <summary>
        /// Represents scope variable with semantic attributes.
        /// </summary>
        [ComVisible(false)]
        public sealed class ScopeVariable
        {
            /// <summary>
            /// Represents expresssion tree that is used to compile the variable.
            /// </summary>
            public readonly ParameterExpression Expression;

            /// <summary>
            /// Represents type of the variable.
            /// </summary>
            public readonly ScriptTypeCode TypeCode;

            /// <summary>
            /// Initializes a new local variable descriptor.
            /// </summary>
            /// <param name="expr"></param>
            /// <param name="typeCode"></param>
            public ScopeVariable(ParameterExpression expr, ScriptTypeCode typeCode)
                : this(expr, new object[] { typeCode })
            {
            }

            /// <summary>
            /// Initializes a new local variable descriptor.
            /// </summary>
            /// <param name="expr"></param>
            /// <param name="attributes"></param>
            public ScopeVariable(ParameterExpression expr, params object[] attributes)
            {
                Expression = expr;
                TypeCode = ScriptTypeCode.Unknown;
                foreach (var a in attributes)
                    if (a is ScriptTypeCode)
                        TypeCode = (ScriptTypeCode)a;
            }
        }

        /// <summary>
        /// Represents runtime variable table.
        /// </summary>
        [ComVisible(false)]
        public interface IScopeVariables : IDictionary<string, ScopeVariable>
        {
        }

        /// <summary>
        /// Represents implementation of the runtime variable table.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class ScopeVariables : Dictionary<string, ScopeVariable>, IScopeVariables
        {
            public ScopeVariables(int capacity)
                : base(capacity, new StringEqualityComparer())
            {
            }
        }

        /// <summary>
        /// Represents a read-only empty dictionary of scope variables.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        protected sealed class EmptyScopeVariables : IScopeVariables
        {
            private readonly ICollection<string> Keys;
            private readonly ICollection<ScopeVariable> Values;

            private EmptyScopeVariables()
            {
                Keys = new string[0];
                Values = new ScopeVariable[0];
            }

            /// <summary>
            /// Represents a singleton instance of the dictionary.
            /// </summary>
            public static readonly EmptyScopeVariables Instance = new EmptyScopeVariables();

            void IDictionary<string, ScopeVariable>.Add(string key, ScopeVariable value)
            {
            }

            bool IDictionary<string, ScopeVariable>.ContainsKey(string key)
            {
                return false;
            }

            ICollection<string> IDictionary<string, ScopeVariable>.Keys
            {
                get { return Keys; }
            }

            bool IDictionary<string, ScopeVariable>.Remove(string key)
            {
                return false;
            }

            bool IDictionary<string, ScopeVariable>.TryGetValue(string key, out ScopeVariable value)
            {
                value = null;
                return false;
            }

            ICollection<ScopeVariable> IDictionary<string, ScopeVariable>.Values
            {
                get { return Values; }
            }

            ScopeVariable IDictionary<string, ScopeVariable>.this[string key]
            {
                get { return null; }
                set { }
            }

            void ICollection<KeyValuePair<string, ScopeVariable>>.Add(KeyValuePair<string, ScopeVariable> item)
            {
            }

            void ICollection<KeyValuePair<string, ScopeVariable>>.Clear()
            {
            }

            bool ICollection<KeyValuePair<string, ScopeVariable>>.Contains(KeyValuePair<string, ScopeVariable> item)
            {
                return false;
            }

            void ICollection<KeyValuePair<string, ScopeVariable>>.CopyTo(KeyValuePair<string, ScopeVariable>[] array, int arrayIndex)
            {
            }

            int ICollection<KeyValuePair<string, ScopeVariable>>.Count
            {
                get { return 0; }
            }

            bool ICollection<KeyValuePair<string, ScopeVariable>>.IsReadOnly
            {
                get { return true; }
            }

            bool ICollection<KeyValuePair<string, ScopeVariable>>.Remove(KeyValuePair<string, ScopeVariable> item)
            {
                return false;
            }

            IEnumerator<KeyValuePair<string, ScopeVariable>> IEnumerable<KeyValuePair<string, ScopeVariable>>.GetEnumerator()
            {
                yield break;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                yield break;
            }
        }

        /// <summary>
        /// Represents lexical scope options.
        /// </summary>
        [Serializable]
        [ComVisible(false)]
        protected enum ScopeOptions : int
        {
            /// <summary>
            /// Represents standard lexical scope with closures, labels and state variables.
            /// </summary>
            None = 0,

            /// <summary>
            /// Represents that the lexical scope doesn't change interpreter state.
            /// </summary>
            InheritedState = 0x01,

            /// <summary>
            /// Represents that the lexical scope doesn't have BOS and EOS labels.
            /// </summary>
            InheritedLabels = 0x02,
        }
        #endregion
        private const string BeginOfScopeLabelName = "__BOS__";
        private const string EndOfScopeLabelName = "__EOS__";
        private const string StateHolderVariableName = "__STATE__";

        /// <summary>
        /// Represents outer lexical scope.
        /// </summary>
        public readonly LexicalScope Parent;
        private readonly LabelTarget m_eos;
        private readonly LabelTarget m_bos;
        private readonly ParameterExpression m_state;
        private long m_counter;

        /// <summary>
        /// Initializes a new dictionary that is used to store variables.
        /// </summary>
        /// <param name="capacity">The initial capacity of the table.</param>
        /// <returns>A new dictionary that is used to store variables.</returns>
        protected static IScopeVariables CreateVariableTable(int capacity = 10)
        {
            return new ScopeVariables(capacity);
        }

        private LexicalScope(LexicalScope parent)
        {
            m_counter = 0;
            Parent = parent;
        }

        private LexicalScope(LexicalScope parent, LabelTarget bos, LabelTarget eos, ParameterExpression stateVar)
            :this(parent)
        {
            m_bos = bos;
            m_eos = eos;
            m_state = stateVar;
        }

        /// <summary>
        /// Initializes a new lexical scope.
        /// </summary>
        /// <param name="parent">Parent lexical scope.</param>
        /// <param name="endOfScopeType">The type of the enclosed label.</param>
        /// <param name="options">Lexical scope options.</param>
        protected LexicalScope(LexicalScope parent, Type endOfScopeType, ScopeOptions options = ScopeOptions.None)
            : this(parent,
                (options & ScopeOptions.InheritedLabels) == 0 ? Expression.Label(typeof(void), BeginOfScopeLabelName) : null,
            (options & ScopeOptions.InheritedLabels) == 0 ? Expression.Label(endOfScopeType, EndOfScopeLabelName) : null,
            (options & ScopeOptions.InheritedState) == 0 ? Expression.Parameter(typeof(InterpreterState)) : null)
        {
        }

        /// <summary>
        /// Initializes a new lexical scope.
        /// </summary>
        /// <param name="parent">Parent lexical scope.</param>
        /// <param name="options">Lexical scope options.</param>
        protected LexicalScope(LexicalScope parent, ScopeOptions options = ScopeOptions.None)
            : this(parent, typeof(IScriptObject), options)
        {
        }

        /// <summary>
        /// Generates a temporary variable name.
        /// </summary>
        /// <returns>Tbe temporary variable name.</returns>
        public string GenerateVariableName()
        {
            const string TempVariablePrefix = "#DS_";
            var name = String.Concat(TempVariablePrefix, m_counter);
            Interlocked.Increment(ref m_counter);
            return name;
        }

        internal static IEnumerable<ParameterExpression> GetExpressions(IEnumerable<ScopeVariable> variables)
        {
            return variables.Select(l => l.Expression);
        }

        /// <summary>
        /// Gets a collection of declared variables.
        /// </summary>
        protected abstract IEnumerable<string> Variables
        {
            get;
        }

        IEnumerable<string> ILexicalScope.Variables
        {
            get { return Variables; }
        }

        /// <summary>
        /// Gets variable declaration by its name.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <returns>An expression that references the variable; or <see langword="null"/> if variable is not declared.</returns>
        public abstract ScopeVariable this[string variableName]
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        internal ParameterExpression GetVariableExpression(string variableName)
        {
            var v = this[variableName];
            return v != null ? v.Expression : null;
        }

        /// <summary>
        /// Declares a new variable in the current scope.
        /// </summary>
        /// <param name="variableName">The name of the variable to be declared.</param>
        /// <param name="declaration">The slot that is created by the scope and can be used in the LINQ expression trees.</param>
        /// <param name="attributes">An array of semantics attributes.</param>
        /// <returns></returns>
        public bool DeclareVariable(string variableName, out ParameterExpression declaration, params object[] attributes)
        {
            return DeclareVariable<IStaticRuntimeSlot>(variableName, out declaration, attributes);
        }

        /// <summary>
        /// Declares a new variable in the current scope.
        /// </summary>
        /// <param name="variableName">The name of the variable to be declared.</param>
        /// <param name="declaration">The slot that is created by the scope and can be used in the LINQ expression trees.</param>
        /// <param name="attributes">An array of semantic attributes.</param>
        /// <returns></returns>
        public bool DeclareVariable(string variableName, out Expression declaration, params object[] attributes)
        {
            var variableDef = default(ParameterExpression);
            switch (DeclareVariable(variableName, out declaration, attributes))
            {
                case true:
                    declaration = variableDef;
                    return true;
                default:
                    declaration = null;
                    return false;
            }
        }

        /// <summary>
        /// Declares a new variable in the current scope.
        /// </summary>
        /// <param name="variableName">The name of the variable to be declared.</param>
        /// <returns></returns>
        public bool DeclareVariable(string variableName)
        {
            var localVariable = default(ParameterExpression);
            return DeclareVariable(variableName, out localVariable);
        }

        /// <summary>
        /// Declares a new runtime variable with the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the runtime variable.</typeparam>
        /// <param name="variableName">The name of the variable to declare.</param>
        /// <param name="declaration">The slot that is created by the scope and can be used in the LINQ expression trees.</param>
        /// <param name="attributes">An array of semantics attributes.</param>
        /// <returns></returns>
        protected abstract bool DeclareVariable<T>(string variableName, out ParameterExpression declaration, params object[] attributes);

        bool ILexicalScope.DeclareVariable<T>(string variableName, object[] attributes)
        {
            var declaration = default(ParameterExpression);
            return DeclareVariable<T>(variableName, out declaration, attributes);
        }

        /// <summary>
        /// Gets scope variable, such as 'this'.
        /// </summary>
        public virtual Expression ScopeVar
        {
            get { return Parent != null ? Parent.ScopeVar : null; }
        }

        /// <summary>
        /// Gets variable that holds the state.
        /// </summary>
        public ParameterExpression StateHolder
        {
            get
            {
                switch (m_state != null)
                {
                    case true: return m_state;
                    default:
                        if (IsTopLevel) throw new NotSupportedException();
                        else return Parent.StateHolder;
                }
            }
        }

        /// <summary>
        /// Gets a label that points to the end of the lexical scope.
        /// </summary>
        public LabelTarget EndOfScope
        {
            get 
            {
                switch (m_eos != null)
                {
                    case true: return m_eos;
                    default:
                        if (IsTopLevel) throw new NotSupportedException();
                        else return Parent.EndOfScope;
                }
            }
        }

        /// <summary>
        /// Gets a lavel that points to the beginning of the lexical scope.
        /// </summary>
        public LabelTarget BeginOfScope
        {
            get 
            {
                switch (m_bos != null)
                {
                    case true: return m_bos;
                    default:
                        if (IsTopLevel) throw new NotSupportedException();
                        else return Parent.BeginOfScope;
                }
            }
        }

        bool ILexicalScope.Transparent
        {
            get { return m_state == null; }
        }

        /// <summary>
        /// Gets a value indicating that the current scope is top-level scope.
        /// </summary>
        public bool IsTopLevel
        {
            get { return Parent == null; }
        }

        ILexicalScope ILexicalScope.Parent
        {
            get { return Parent; }
        }

        /// <summary>
        /// Gets type of the variable.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public ScriptTypeCode GetType(string variableName)
        {
            return (ScriptTypeCode)GetAttribute(typeof(ScriptTypeCode), variableName);
        }

        private object GetAttribute(Type attributeType, string variableName)
        {
            var info = this[variableName];
            if (Equals(attributeType, typeof(ScriptTypeCode)))
                return info != null ? info.TypeCode : ScriptTypeCode.Unknown;
            else if (attributeType.IsValueType) return Activator.CreateInstance(attributeType);
            else return null;
        }

        T ILexicalScope.GetAttribute<T>(string variableName)
        {
            return (T)GetAttribute(typeof(T), variableName);
        }

        /// <summary>
        /// Returns an enumerator through local variables.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, ParameterExpression>> GetEnumerator()
        {
            foreach (var name in Variables)
                yield return new KeyValuePair<string, ParameterExpression>(name, this[name].Expression);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
