using System;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using BindingFlags = System.Reflection.BindingFlags;

    /// <summary>
    /// Represents an operator. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    sealed class Operator: Token
    {
        #region Nested Types
        internal static class HashCodes
        {
#if DEBUG
            internal static void PrintOperatorValues(System.IO.TextWriter output)
            {
                const BindingFlags PublicFields = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
                var operatorTokenType = typeof(Operator);
                foreach (var field in operatorTokenType.GetFields(PublicFields))
                    if (field.FieldType.Equals(operatorTokenType))
                    {
                        var p = field.GetValue(null);
                        output.WriteLine("/// <summary>");
                        output.WriteLine("/// Hash code of {0} operator", p);
                        output.WriteLine("/// </summary>");
                        output.WriteLine("public const int lxm{0} = {1};", field.Name, p.GetHashCode());
                    }
                foreach (var field in operatorTokenType.GetFields(PublicFields))
                    if (field.FieldType.Equals(operatorTokenType))
                    {
                        output.WriteLine("case HashCodes.lxm{0}: return {1};", field.Name, field.Name);
                    }
            }
#endif
            /// <summary>
            /// Hash code of + operator
            /// </summary>
            public const int lxmPlus = 43;
            /// <summary>
            /// Hash code of ++ operator
            /// </summary>
            public const int lxmDoublePlus = 42656;
            /// <summary>
            /// Hash code of * operator
            /// </summary>
            public const int lxmAsterisk = 42;
            /// <summary>
            /// Hash code of ** operator
            /// </summary>
            public const int lxmDoubleAsterisk = 41664;
            /// <summary>
            /// Hash code of / operator
            /// </summary>
            public const int lxmSlash = 47;
            /// <summary>
            /// Hash code of = operator
            /// </summary>
            public const int lxmAssignment = 61;
            /// <summary>
            /// Hash code of . operator
            /// </summary>
            public const int lxmMemberAccess = 46;
            /// <summary>
            /// Hash code of - operator
            /// </summary>
            public const int lxmMinus = 45;
            /// <summary>
            /// Hash code of -- operator
            /// </summary>
            public const int lxmDoubleMinus = 44640;
            /// <summary>
            /// Hash code of == operator
            /// </summary>
            public const int lxmValueEquality = 60512;
            /// <summary>
            /// Hash code of === operator
            /// </summary>
            public const int lxmReferenceEquality = 59967453;
            /// <summary>
            /// Hash code of ! operator
            /// </summary>
            public const int lxmNegotiation = 33;
            /// <summary>
            /// Hash code of &amp; operator
            /// </summary>
            public const int lxmIntersection = 38;
            /// <summary>
            /// Hash code of | operator
            /// </summary>
            public const int lxmUnion = 124;
            /// <summary>
            /// Hash code of != operator
            /// </summary>
            public const int lxmValueInequality = 32764;
            /// <summary>
            /// Hash code of !== operator
            /// </summary>
            public const int lxmReferenceInequality = 32469185;
            /// <summary>
            /// Hash code of &amp;= operator
            /// </summary>
            public const int lxmIntersectionAssignment = 37719;
            /// <summary>
            /// Hash code of |= operator
            /// </summary>
            public const int lxmUnionAssignment = 122945;
            /// <summary>
            /// Hash code of += operator
            /// </summary>
            public const int lxmPlusAssignment = 42674;
            /// <summary>
            /// Hash code of -= operator
            /// </summary>
            public const int lxmMinusAssignment = 44656;
            /// <summary>
            /// Hash code of *= operator
            /// </summary>
            public const int lxmAsteriskAssignment = 41683;
            /// <summary>
            /// Hash code of /= operator
            /// </summary>
            public const int lxmSlashAssignment = 46638;
            /// <summary>
            /// Hash code of &lt; operator
            /// </summary>
            public const int lxmLessThan = 60;
            /// <summary>
            /// Hash code of &lt;= operator
            /// </summary>
            public const int lxmLessThanOrEqual = 59521;
            /// <summary>
            /// Hash code of &gt; operator
            /// </summary>
            public const int lxmGreaterThan = 62;
            /// <summary>
            /// Hash code of &gt;= operator
            /// </summary>
            public const int lxmGreaterThanOrEqual = 61503;
            /// <summary>
            /// Hash code of ^ operator
            /// </summary>
            public const int lxmExclusion = 94;
            /// <summary>
            /// Hash code of ^= operator
            /// </summary>
            public const int lxmExclusionAssignment = 93215;
            /// <summary>
            /// Hash code of || operator
            /// </summary>
            public const int lxmOrElse = 123008;
            /// <summary>
            /// Hash code of &amp;&amp; operator
            /// </summary>
            public const int lxmAndAlso = 37696;
            /// <summary>
            /// Hash code of :: operator
            /// </summary>
            public const int lxmMetadataDiscovery = 57536;
            /// <summary>
            /// Hash code of ? operator
            /// </summary>
            public const int lxmVoidCheck = 63;
            /// <summary>
            /// Hash code of ?? operator
            /// </summary>
            public const int lxmCoalesce = 62496;
            /// <summary>
            /// Hash code of $ operator
            /// </summary>
            public const int lxmTypeOf = 36;
            /// <summary>
            /// Hash code of % operator
            /// </summary>
            public const int lxmModulo = 37;
            /// <summary>
            /// Hash code of %= operator
            /// </summary>
            public const int lxmModuloAssign = 36728;
            /// <summary>
            /// Hash code of ?= operator
            /// </summary>
            public const int lxmInitializer = 62494;
        }
        #endregion
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

        /// <summary>
        /// Represents ?= operator.
        /// </summary>
        public static readonly Operator Initializer = new Operator(Lexeme.Question, Lexeme.Assignment);

        internal static Operator FromHashCode(int hashCode)
        {
            switch (hashCode)
            {

                case HashCodes.lxmPlus: return Plus;
                case HashCodes.lxmDoublePlus: return DoublePlus;
                case HashCodes.lxmAsterisk: return Asterisk;
                case HashCodes.lxmDoubleAsterisk: return DoubleAsterisk;
                case HashCodes.lxmSlash: return Slash;
                case HashCodes.lxmAssignment: return Assignment;
                case HashCodes.lxmMemberAccess: return MemberAccess;
                case HashCodes.lxmMinus: return Minus;
                case HashCodes.lxmDoubleMinus: return DoubleMinus;
                case HashCodes.lxmValueEquality: return ValueEquality;
                case HashCodes.lxmReferenceEquality: return ReferenceEquality;
                case HashCodes.lxmNegotiation: return Negotiation;
                case HashCodes.lxmIntersection: return Intersection;
                case HashCodes.lxmUnion: return Union;
                case HashCodes.lxmValueInequality: return ValueInequality;
                case HashCodes.lxmReferenceInequality: return ReferenceInequality;
                case HashCodes.lxmIntersectionAssignment: return IntersectionAssignment;
                case HashCodes.lxmUnionAssignment: return UnionAssignment;
                case HashCodes.lxmPlusAssignment: return PlusAssignment;
                case HashCodes.lxmMinusAssignment: return MinusAssignment;
                case HashCodes.lxmAsteriskAssignment: return AsteriskAssignment;
                case HashCodes.lxmSlashAssignment: return SlashAssignment;
                case HashCodes.lxmLessThan: return LessThan;
                case HashCodes.lxmLessThanOrEqual: return LessThanOrEqual;
                case HashCodes.lxmGreaterThan: return GreaterThan;
                case HashCodes.lxmGreaterThanOrEqual: return GreaterThanOrEqual;
                case HashCodes.lxmExclusion: return Exclusion;
                case HashCodes.lxmExclusionAssignment: return ExclusionAssignment;
                case HashCodes.lxmOrElse: return OrElse;
                case HashCodes.lxmAndAlso: return AndAlso;
                case HashCodes.lxmMetadataDiscovery: return MetadataDiscovery;
                case HashCodes.lxmVoidCheck: return VoidCheck;
                case HashCodes.lxmCoalesce: return Coalesce;
                case HashCodes.lxmTypeOf: return TypeOf;
                case HashCodes.lxmModulo: return Modulo;
                case HashCodes.lxmModuloAssign: return ModuloAssign;
                case HashCodes.lxmInitializer: return Initializer;
                default: return null;
            }
        }
    }
}
