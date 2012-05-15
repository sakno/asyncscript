using System;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeBinaryOperatorType = System.CodeDom.CodeBinaryOperatorType;

    /// <summary>
    /// Represents type of the DynamicScript binary operator.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public enum ScriptCodeBinaryOperatorType: int
    {
        #region Primary

        /// <summary>
        /// Represents member access operator.
        /// </summary>
        MemberAccess = 0xFF00,

        /// <summary>
        /// Represents metadata discovery operator.
        /// </summary>
        MetadataDiscovery = 0xFF0E,

        #endregion

        #region Conversions

        /// <summary>
        /// Represents typecast operator 'to'.
        /// </summary>
        TypeCast = 0xFF03,

        #endregion

        #region Multiplicative

        /// <summary>
        /// Represents binary * operator.
        /// </summary>
        Multiply = CodeBinaryOperatorType.Multiply,

        /// <summary>
        /// Represents binary / operator.
        /// </summary>
        Divide = CodeBinaryOperatorType.Divide,

        /// <summary>
        /// Represents % operator.
        /// </summary>
        Modulo = CodeBinaryOperatorType.Modulus,

        #endregion

        #region Additive

        /// <summary>
        /// Represents binary + operator.
        /// </summary>
        Add = CodeBinaryOperatorType.Add,

        /// <summary>
        /// Represents binary - operator.
        /// </summary>
        Subtract = CodeBinaryOperatorType.Subtract,

        #endregion

        #region Relational and type testing

        /// <summary>
        /// Represents &lt; operator.
        /// </summary>
        LessThan = CodeBinaryOperatorType.LessThan,

        /// <summary>
        /// Represents &lt;= operator.
        /// </summary>
        LessThanOrEqual = CodeBinaryOperatorType.LessThanOrEqual,

        /// <summary>
        /// Represents &gt; operator.
        /// </summary>
        GreaterThan = CodeBinaryOperatorType.GreaterThan,

        /// <summary>
        /// Represents &gt;= operator.
        /// </summary>
        GreaterThanOrEqual = CodeBinaryOperatorType.GreaterThanOrEqual,

        /// <summary>
        /// Represents instance check operator.
        /// </summary>
        InstanceOf = 0xFF02,

        /// <summary>
        /// Represents 'in' operator.
        /// </summary>
        PartOf = 0xFF0B,

        #endregion

        #region Equality

        /// <summary>
        /// Represents value equality.
        /// </summary>
        ValueEquality = CodeBinaryOperatorType.ValueEquality,

        /// <summary>
        /// Represents value inequality.
        /// </summary>
        ValueInequality = 0xFF01,

        /// <summary>
        /// Represents reference equality.
        /// </summary>
        ReferenceEquality = CodeBinaryOperatorType.IdentityEquality,

        /// <summary>
        /// Represents reference inequality.
        /// </summary>
        ReferenceInequality = CodeBinaryOperatorType.IdentityInequality,

        #endregion

        /// <summary>
        /// Represnts union operator.
        /// </summary>
        Union = CodeBinaryOperatorType.BitwiseOr,

        /// <summary>
        /// Represents intersection operator.
        /// </summary>
        Intersection = CodeBinaryOperatorType.BitwiseAnd,

        /// <summary>
        /// Represents ^ operator.
        /// </summary>
        Exclusion = 0xFF0A,

        /// <summary>
        /// Represents &amp;&amp; operator.
        /// </summary>
        AndAlso = CodeBinaryOperatorType.BooleanOr,

        /// <summary>
        /// Represents || operator.
        /// </summary>
        OrElse = CodeBinaryOperatorType.BooleanAnd,

        #region Assigment

        /// <summary>
        /// Represents value assignment operator.
        /// </summary>
        Assign = CodeBinaryOperatorType.Assign,

        /// <summary>
        /// Represents += operator.
        /// </summary>
        AdditiveAssign = 0xFF04,

        /// <summary>
        /// Represents -= operator.
        /// </summary>
        SubtractiveAssign = 0xFF05,

        /// <summary>
        /// Represents *= operator.
        /// </summary>
        MultiplicativeAssign = 0xFF06,

        /// <summary>
        /// Represents |= operator.
        /// </summary>
        Expansion = 0xFF07,

        /// <summary>
        /// Represents &amp;= operator.
        /// </summary>
        Reduction = 0xFF08,

        /// <summary>
        /// Represents /= operator.
        /// </summary>
        DivideAssign = 0xFF09,

        /// <summary>
        /// Represents ^= operator.
        /// </summary>
        ExclusionAssign = 0xFF0C,

        /// <summary>
        /// Represents %= operator.
        /// </summary>
        ModuloAssign = 0xFF10,

        /// <summary>
        /// Represents ?= operator.
        /// </summary>
        Initializer = 0xFF11,

        #endregion

        /// <summary>
        /// Represents coalesce operator.
        /// </summary>
        Coalesce = 0xFF0F,
    }
}
