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
    public abstract class LexicalScope: ILexicalScope
    {
        #region Nested Types

        /// <summary>
        /// Represents runtime variable table.
        /// </summary>
        [ComVisible(false)]
        public interface IScopeVariables : IDictionary<string, ParameterExpression>
        {
        }

        /// <summary>
        /// Represents a read-only empty dictionary of scope variables.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        protected sealed class EmptyScopeVariables : IScopeVariables
        {
            private readonly ICollection<string> Keys;
            private readonly ICollection<ParameterExpression> Values;

            private EmptyScopeVariables()
            {
                Keys = new string[0];
                Values = new ParameterExpression[0];
            }

            /// <summary>
            /// Represents a singleton instance of the dictionary.
            /// </summary>
            public static readonly EmptyScopeVariables Instance = new EmptyScopeVariables();

            void IDictionary<string, ParameterExpression>.Add(string key, ParameterExpression value)
            {
            }

            bool IDictionary<string, ParameterExpression>.ContainsKey(string key)
            {
                return false;
            }

            ICollection<string> IDictionary<string, ParameterExpression>.Keys
            {
                get { return Keys; }
            }

            bool IDictionary<string, ParameterExpression>.Remove(string key)
            {
                return false;
            }

            bool IDictionary<string, ParameterExpression>.TryGetValue(string key, out ParameterExpression value)
            {
                value = null;
                return false;
            }

            ICollection<ParameterExpression> IDictionary<string, ParameterExpression>.Values
            {
                get { return Values; }
            }

            ParameterExpression IDictionary<string, ParameterExpression>.this[string key]
            {
                get { return null; }
                set { }
            }

            void ICollection<KeyValuePair<string, ParameterExpression>>.Add(KeyValuePair<string, ParameterExpression> item)
            {
            }

            void ICollection<KeyValuePair<string, ParameterExpression>>.Clear()
            {
            }

            bool ICollection<KeyValuePair<string, ParameterExpression>>.Contains(KeyValuePair<string, ParameterExpression> item)
            {
                return false;
            }

            void ICollection<KeyValuePair<string, ParameterExpression>>.CopyTo(KeyValuePair<string, ParameterExpression>[] array, int arrayIndex)
            {
            }

            int ICollection<KeyValuePair<string, ParameterExpression>>.Count
            {
                get { return 0; }
            }

            bool ICollection<KeyValuePair<string, ParameterExpression>>.IsReadOnly
            {
                get { return true; }
            }

            bool ICollection<KeyValuePair<string, ParameterExpression>>.Remove(KeyValuePair<string, ParameterExpression> item)
            {
                return false;
            }

            IEnumerator<KeyValuePair<string, ParameterExpression>> IEnumerable<KeyValuePair<string, ParameterExpression>>.GetEnumerator()
            {
                yield break;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                yield break;
            }
        }

        /// <summary>
        /// Represents implementation of the runtime variable table.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class ScopeVariables : Dictionary<string, ParameterExpression>, IScopeVariables
        {
            public ScopeVariables(int capacity)
                : base(capacity, new StringEqualityComparer())
            {
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
        public abstract ParameterExpression this[string variableName]
        {
            get;
        }

        /// <summary>
        /// Declares a new variable in the current scope.
        /// </summary>
        /// <param name="variableName">The name of the variable to be declared.</param>
        /// <param name="declaration">The slot that is created by the scope and can be used in the LINQ expression trees.</param>
        /// <returns></returns>
        public bool DeclareVariable(string variableName, out ParameterExpression declaration)
        {
            return DeclareVariable<IRuntimeSlot>(variableName, out declaration);
        }

        /// <summary>
        /// Declares a new variable in the current scope.
        /// </summary>
        /// <param name="variableName">The name of the variable to be declared.</param>
        /// <param name="declaration">The slot that is created by the scope and can be used in the LINQ expression trees.</param>
        /// <returns></returns>
        public bool DeclareVariable(string variableName, out Expression declaration)
        {
            var variableDef = default(ParameterExpression);
            switch (DeclareVariable(variableName, out declaration))
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
        /// <returns></returns>
        protected abstract bool DeclareVariable<T>(string variableName, out ParameterExpression declaration);

        bool ILexicalScope.DeclareVariable<T>(string variableName)
        {
            var declaration = default(ParameterExpression);
            return DeclareVariable<T>(variableName, out declaration);
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
    }
}
