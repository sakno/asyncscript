using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IEnumerable = System.Collections.IEnumerable;
    using StringBuilder = System.Text.StringBuilder;
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    /// Represents complex object markup expression.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeObjectExpression: ScriptCodeExpression, IEnumerable, IEquatable<ScriptCodeObjectExpression>
    {
        #region Nested Types

        /// <summary>
        /// Represents object slot.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public struct Slot : ISlot, IEquatable<Slot>
        {
            /// <summary>
            /// Represents name of the object slot.
            /// </summary>
            public readonly string Name;
            private ScriptCodeExpression m_binding;
            private ScriptCodeExpression m_init;
            private int? m_hash;

            /// <summary>
            /// Initializes a new object slot.
            /// </summary>
            /// <param name="name">The name of the slot. Cannot be <see langword="null"/> or empty.</param>
            /// <param name="initExpr"></param>
            /// <param name="contractBinding"></param>
            /// <exception cref="System.ArgumentNullException"><paramref name="name"/> is <see langword="null"/> or empty.</exception>
            public Slot(string name, ScriptCodeExpression initExpr=null, ScriptCodeExpression contractBinding = null)
            {
                Name = name??string.Empty;
                m_binding = m_init = null;
                m_hash = null;
            }

            internal Slot(ISlot slotdef)
                : this(slotdef.Name, slotdef.InitExpression, slotdef.ContractBinding)
            {
            }

            string ISlot.Name
            {
                get { return Name; }
            }

            SlotDeclarationStyle ISlot.Style
            {
                get { return Style; }
            }

            internal SlotDeclarationStyle Style
            {
                get
                {
                    var result = (m_init != null ? SlotDeclarationStyle.ContractBindingOnly : SlotDeclarationStyle.Unknown) | (m_init != null ? SlotDeclarationStyle.InitExpressionOnly : SlotDeclarationStyle.Unknown);
                    return result == SlotDeclarationStyle.Unknown ? SlotDeclarationStyle.ContractBindingOnly : result;
                }
            }

            /// <summary>
            /// Gets or sets slot initialization expression.
            /// </summary>
            public ScriptCodeExpression InitExpression
            {
                get { return m_init; }
                set 
                { 
                    m_init = value;
                    m_hash = null;
                }
            }

            /// <summary>
            /// Gets or sets slot contract binding.
            /// </summary>
            public ScriptCodeExpression ContractBinding
            {
                get { return m_init == null && m_binding == null ? ScriptCodeVariableDeclaration.DefaultVariableType : m_binding; }
                set 
                {
                    m_binding = value;
                    m_hash = null;
                }
            }

            /// <summary>
            /// Returns a string representation of the slot.
            /// </summary>
            /// <returns>The string representation of the slot.</returns>
            public override string ToString()
            {
                var builder = new StringBuilder();
                builder.Append(string.Concat(Name, InitExpression != null ? String.Concat(Operator.Assignment, InitExpression) : String.Empty));
                builder.Append(ContractBinding != null ? String.Concat(Punctuation.Colon, ContractBinding) : String.Empty);
                return builder.ToString();
            }

            /// <summary>
            /// Determines whether this object represents the same slot definition as other.
            /// </summary>
            /// <param name="other">Other slot definition to compare.</param>
            /// <returns></returns>
            public bool Equals(Slot other)
            {
                return StringEqualityComparer.Equals(Name, other.Name) &&
                    Equals(ContractBinding, other.ContractBinding) &&
                    Equals(InitExpression, other.InitExpression);
            }

            bool IEquatable<ISlot>.Equals(ISlot other)
            {
                return other is Slot ? Equals((Slot)other) : false;
            }

            /// <summary>
            /// Determines whether this object represents the same slot definition as other.
            /// </summary>
            /// <param name="other">Other slot definition to compare.</param>
            /// <returns></returns>
            public override bool Equals(object other)
            {
                return other is Slot ? Equals((Slot)other) : false;
            }

            /// <summary>
            /// Computes a hash code for slot definition.
            /// </summary>
            /// <returns>A hash code of slot definition.</returns>
            public override int GetHashCode()
            {
                if (m_hash == null) m_hash = StringEqualityComparer.GetHashCode(ToString());
                return m_hash.Value;
            }

            Expression IRestorable.Restore()
            {
                var ctor = LinqHelpers.BodyOf<string, ScriptCodeExpression, ScriptCodeExpression, Slot, NewExpression>((name, init, bind) => new Slot(name, init, bind));
                return ctor.Update(new[] { LinqHelpers.Constant(Name), LinqHelpers.Restore(InitExpression), LinqHelpers.Restore(ContractBinding) });
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
                if (InitExpression != null) InitExpression = InitExpression.Visit(this, visitor) as ScriptCodeExpression ?? InitExpression;
                if (ContractBinding != null) ContractBinding = ContractBinding.Visit(this, visitor) as ScriptCodeExpression ?? ContractBinding;
                return visitor.Invoke(this) ?? this;
            }

            ISyntaxTreeNode ISyntaxTreeNode.Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
            {
                return Visit(parent, visitor);
            }

            object ICloneable.Clone()
            {
                return new Slot(Name, Extensions.Clone(InitExpression), Extensions.Clone(ContractBinding));
            }
        }
        #endregion

        private readonly IDictionary<string, Slot> m_slots;

        /// <summary>
        /// Initializes a new complex object expression.
        /// </summary>
        /// <param name="slots">Dictionary of the object slots.</param>
        public ScriptCodeObjectExpression(IDictionary<string, Slot> slots)
        {
            m_slots = slots != null ? new Dictionary<string, Slot>(slots, new StringEqualityComparer()) : new Dictionary<string, Slot>(new StringEqualityComparer());
        }

        private ScriptCodeObjectExpression(IEnumerable<KeyValuePair<string, Slot>> slots)
            :this(default(IDictionary<string, Slot>))
        {
            foreach (var s in slots ?? new KeyValuePair<string, Slot>[0])
                m_slots[s.Key] = s.Value;
        }

        /// <summary>
        /// Initializes a new complex object expression
        /// </summary>
        /// <param name="slots"></param>
        public ScriptCodeObjectExpression(params KeyValuePair<string, Slot>[] slots)
            : this((IEnumerable<KeyValuePair<string, Slot>>)slots)
        {
            
        }

        internal ScriptCodeObjectExpression(IEnumerable<ISlot> slots)
            : this(Enumerable.Select(slots, s => new KeyValuePair<string, Slot>(s.Name, new Slot(s))))
        {
        }

        /// <summary>
        /// Gets collection of slot names.
        /// </summary>
        public ICollection<string> Names
        {
            get { return m_slots.Keys; }
        }

        /// <summary>
        /// Gets slot by its name.
        /// </summary>
        /// <param name="name">The name of the slot.</param>
        /// <returns>The object's slot.</returns>
        public Slot this[string name]
        {
            get { return m_slots[name]; }
        }

        /// <summary>
        /// Resolves slot definition by its name.
        /// </summary>
        /// <param name="name">The name of the slot.</param>
        /// <param name="definition">A slot definition.</param>
        /// <returns></returns>
        public bool TryGetSlot(string name, out Slot definition)
        {
            return m_slots.TryGetValue(name, out definition);
        }

        /// <summary>
        /// Gets count of slots
        /// </summary>
        public int Count
        {
            get { return m_slots.Count; }
        }

        /// <summary>
        /// Adds a new slot to the expression.
        /// </summary>
        /// <param name="slotName">The name of the slot to be added.</param>
        /// <param name="initExpression">Slot initialization expression.</param>
        /// <param name="contractBinding">Slot contract binding.</param>
        public void Add(string slotName, ScriptCodeExpression initExpression = null, ScriptCodeExpression contractBinding = null)
        {
            Add(new Slot(slotName) { InitExpression = initExpression, ContractBinding = contractBinding });
        }

        /// <summary>
        /// Adds a new slot to the expression.
        /// </summary>
        /// <param name="s">The slot to be added.</param>
        public void Add(Slot s)
        {
            m_slots.Add(s.Name, s);
        }

        /// <summary>
        /// Removes slot from the expression.
        /// </summary>
        /// <param name="slotName">The name of the slot to be removed.</param>
        /// <returns><see langword="true"/> if slot is removed successfully; otherwise, <see langword="false"/>.</returns>
        public bool Remove(string slotName)
        {
            return m_slots.Remove(slotName);
        }

        /// <summary>
        /// Determines whether the slot with the specified name is existed.
        /// </summary>
        /// <param name="slotName">The name of the slot.</param>
        /// <returns><see langword="true"/> if slot with the specified name is already existed; otherwise, <see langword="false"/>.</returns>
        public bool Contains(string slotName)
        {
            return m_slots.ContainsKey(slotName);
        }

        System.Collections.IEnumerator IEnumerable.GetEnumerator()
        {
            return m_slots.Values.GetEnumerator();
        }

        internal static ScriptCodeObjectExpression Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            var expr = new ScriptCodeObjectExpression();
            do
            {
                var slotName = default(string);
                var initExpr = default(ScriptCodeExpression);
                var contractBinding = default(ScriptCodeExpression);
                if (!Parser.ParseSlot(lexer, out slotName, out initExpr, out contractBinding, Punctuation.DoubleRightBrace, Punctuation.Comma))
                    return expr;
                if (expr.Contains(slotName))
                    throw CodeAnalysisException.DuplicateIdentifier(slotName, lexer.Current.Key);
                else expr.Add(slotName, initExpr, contractBinding);
            } while (lexer.Current.Value == Punctuation.Comma);
            return expr;
        }

        internal override bool Completed
        {
            get { return true; }
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeObjectExpression other)
        {
            switch (Count == other.Count)
            {
                case true:
                    var slot1 = default(Slot);
                    var slot2 = default(Slot);
                    foreach (var slotName in Names)
                        if (TryGetSlot(slotName, out slot1) && other.TryGetSlot(slotName, out slot2) && slot1.Equals(slot2))
                            continue;
                        else return false;
                    return true;
                default: return false;
            }
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeObjectExpression);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<KeyValuePair<string, Slot>[], ScriptCodeObjectExpression, NewExpression>(slots => new ScriptCodeObjectExpression(slots));
            return ctor.Update(new[] { LinqHelpers.NewArray<string, Slot>(m_slots) });
        }

        internal T[] ToArray<T>(Converter<Slot, T> converter)
        {
            var slots = new Slot[m_slots.Count];
            m_slots.Values.CopyTo(slots, 0);
            return Array.ConvertAll(slots, converter);
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            foreach (var k in m_slots.Keys)
                m_slots[k] = ScriptCodeStatement.Visit(this, m_slots[k], visitor, p => new Slot(p));
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            var result = new ScriptCodeObjectExpression(default(IDictionary<string, Slot>));
            foreach (var p in m_slots.Values)
                result.Add(Extensions.Clone(p));
            return result;
        }

        /// <summary>
        /// Returns a string representation of this expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat(Punctuation.DoubleLeftBrace, string.Join(Punctuation.Comma, m_slots.Values), Punctuation.DoubleRightBrace);
        }
    }
}
