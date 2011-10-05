using System;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an operator. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    sealed class Operator: Token
    {
        private Operator(string @operator)
            : base(@operator)
        {
        }

        private Operator(params char[] @operator)
            : this(new string(@operator))
        {
        }

        /// <summary>
        /// Represents + operator.
        /// </summary>
        public static readonly new Operator Plus = new Operator(Lexeme.Plus);

        /// <summary>
        /// Represents ++ operator.
        /// </summary>
        public static readonly Operator DoublePlus = new Operator(Lexeme.Plus, Lexeme.Plus);

        /// <summary>
        /// Represents * operator.
        /// </summary>
        public static readonly new Operator Asterisk = new Operator(Lexeme.Asterisk);

        /// <summary>
        /// Represents ** operator.
        /// </summary>
        public static readonly Operator DoubleAsterisk = new Operator(Lexeme.Asterisk, Lexeme.Asterisk);

        /// <summary>
        /// Represents / operator.
        /// </summary>
        public static new readonly Operator Slash = new Operator(Lexeme.Slash);

        /// <summary>
        /// Represents = operator.
        /// </summary>
        public static new readonly Operator Assignment = new Operator(Lexeme.Assignment);

        /// <summary>
        /// Represents scope operator.
        /// </summary>
        public static readonly Operator MemberAccess = new Operator(Lexeme.Dot);

        /// <summary>
        /// Represents - operator.
        /// </summary>
        public static readonly new Operator Minus = new Operator(Lexeme.Minus);

        /// <summary>
        /// Represents -- operator.
        /// </summary>
        public static readonly Operator DoubleMinus = new Operator(Lexeme.Minus, Lexeme.Minus);

        /// <summary>
        /// Represents value equality operator(==).
        /// </summary>
        public static readonly Operator ValueEquality = new Operator(Lexeme.Assignment, Lexeme.Assignment);

        /// <summary>
        /// Represents reference equality operator(===).
        /// </summary>
        public static readonly Operator ReferenceEquality = new Operator(Lexeme.Assignment, Lexeme.Assignment, Lexeme.Assignment);

        /// <summary>
        /// Represents ! operator.
        /// </summary>
        public static readonly Operator Negotiation = new Operator(Lexeme.Exclamation);

        /// <summary>
        /// Represents &amp; operator.
        /// </summary>
        public static readonly Operator Intersection = new Operator(Lexeme.Ampersand);

        /// <summary>
        /// Represents | operator.
        /// </summary>
        public static readonly Operator Union = new Operator(Lexeme.Vertical);

        /// <summary>
        /// Represents != operator.
        /// </summary>
        public static readonly Operator ValueInequality = new Operator(Lexeme.Exclamation, Lexeme.Assignment);

        /// <summary>
        /// Representd !== operator.
        /// </summary>
        public static readonly Operator ReferenceInequality = new Operator(Lexeme.Exclamation, Lexeme.Assignment, Lexeme.Assignment);

        /// <summary>
        /// Represents &amp;= operator
        /// </summary>
        public static readonly Operator IntersectionAssignment = new Operator(Lexeme.Ampersand, Lexeme.Assignment);

        /// <summary>
        /// Represents |= operator.
        /// </summary>
        public static readonly Operator UnionAssignment = new Operator(Lexeme.Vertical, Lexeme.Assignment);

        /// <summary>
        /// Represents += operator.
        /// </summary>
        public static readonly Operator PlusAssignment = new Operator(Lexeme.Plus, Lexeme.Assignment);

        /// <summary>
        /// Represents -= operator.
        /// </summary>
        public static readonly Operator MinusAssignment = new Operator(Lexeme.Minus, Lexeme.Assignment);

        /// <summary>
        /// Represents *= operator.
        /// </summary>
        public static readonly Operator AsteriskAssignment = new Operator(Lexeme.Asterisk, Lexeme.Assignment);

        /// <summary>
        /// Represents /= operator.
        /// </summary>
        public static readonly Operator SlashAssignment = new Operator(Lexeme.Slash, Lexeme.Assignment);

        /// <summary>
        /// Represents &lt; operator.
        /// </summary>
        public static readonly new Operator LessThan = new Operator(Lexeme.LessThan);

        /// <summary>
        /// Represents &lt;= operator.
        /// </summary>
        public static readonly Operator LessThanOrEqual = new Operator(Lexeme.LessThan, Lexeme.Assignment);

        /// <summary>
        /// Represents &gt; operator.
        /// </summary>
        public static readonly new Operator GreaterThan = new Operator(Lexeme.GreaterThan);

        /// <summary>
        /// Represents &gt;= operator.
        /// </summary>
        public static readonly Operator GreaterThanOrEqual = new Operator(Lexeme.GreaterThan, Lexeme.Assignment);

        /// <summary>
        /// Represents ^ operator.
        /// </summary>
        public static readonly Operator Exclusion = new Operator(Lexeme.Roof);

        /// <summary>
        /// Represents ^= operator.
        /// </summary>
        public static readonly Operator ExclusionAssignment = new Operator(Lexeme.Roof, Lexeme.Assignment);

        /// <summary>
        /// Represents || operator.
        /// </summary>
        public static readonly Operator OrElse = new Operator(Lexeme.Vertical, Lexeme.Vertical);

        /// <summary>
        /// Represents &amp;&amp; operator.
        /// </summary>
        public static readonly Operator AndAlso = new Operator(Lexeme.Ampersand, Lexeme.Ampersand);

        /// <summary>
        /// Represents :: operator.
        /// </summary>
        public static readonly Operator MetadataDiscovery = new Operator(Lexeme.Colon, Lexeme.Colon);

        /// <summary>
        /// Represents ? operator.
        /// </summary>
        public static readonly Operator VoidCheck = new Operator(Lexeme.Question);

        /// <summary>
        /// Represents ?? operator.
        /// </summary>
        public static readonly Operator Coalesce = new Operator(Lexeme.Question, Lexeme.Question);

        /// <summary>
        /// Represents $ operator.
        /// </summary>
        public static readonly Operator TypeOf = new Operator(Lexeme.Dollar);

        /// <summary>
        /// Represents % operator.
        /// </summary>
        public static readonly Operator Modulo = new Operator(Lexeme.Percent);

        /// <summary>
        /// Represents %= operator.
        /// </summary>
        public static readonly Operator ModuloAssign = new Operator(Lexeme.Percent, Lexeme.Assignment);
    }
}
