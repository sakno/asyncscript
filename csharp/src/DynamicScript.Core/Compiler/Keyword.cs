using System;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using BindingFlags = System.Reflection.BindingFlags;

    /// <summary>
    /// Represents keyword token.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    sealed class Keyword : Token
    {
        #region Nested Types
        [ComVisible(false)]
        internal static class HashCodes
        {
#if DEBUG
            internal static void PrintKeywordValues(System.IO.TextWriter output)
            {
                const BindingFlags PublicFields = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
                var keywordTokenType = typeof(Keyword);
                foreach (var field in keywordTokenType.GetFields(PublicFields))
                    if (field.FieldType.Equals(keywordTokenType))
                    {
                        var keyword = field.GetValue(null);
                        output.WriteLine("/// <summary>");
                        output.WriteLine("/// Hash code of '{0}' keyword", keyword);
                        output.WriteLine("/// </summary>");
                        output.WriteLine("public const int lxm{0} = {1};", field.Name, keyword.GetHashCode());
                    }
                foreach (var field in keywordTokenType.GetFields(PublicFields))
                    if (field.FieldType.Equals(keywordTokenType))
                    {
                        output.WriteLine("case HashCodes.lxm{0}: return {1};", field.Name, field.Name);
                    }
            }
#endif
            /// <summary>
            /// Hash code of 'var' keyword
            /// </summary>
            public const int lxmVar = 115981799;
            /// <summary>
            /// Hash code of 'const' keyword
            /// </summary>
            public const int lxmConst = -1123106461;
            /// <summary>
            /// Hash code of 'is' keyword
            /// </summary>
            public const int lxmIs = 104170;
            /// <summary>
            /// Hash code of 'in' keyword
            /// </summary>
            public const int lxmIn = 104165;
            /// <summary>
            /// Hash code of 'to' keyword
            /// </summary>
            public const int lxmTo = 115067;
            /// <summary>
            /// Hash code of 'object' keyword
            /// </summary>
            public const int lxmObject = -1425986369;
            /// <summary>
            /// Hash code of 'type' keyword
            /// </summary>
            public const int lxmType = 1345896634;
            /// <summary>
            /// Hash code of 'this' keyword
            /// </summary>
            public const int lxmThis = 1329194334;
            /// <summary>
            /// Hash code of 'integer' keyword
            /// </summary>
            public const int lxmInteger = -581406786;
            /// <summary>
            /// Hash code of 'for' keyword
            /// </summary>
            public const int lxmFor = 100282377;
            /// <summary>
            /// Hash code of 'leave' keyword
            /// </summary>
            public const int lxmLeave = -2059448841;
            /// <summary>
            /// Hash code of 'continue' keyword
            /// </summary>
            public const int lxmContinue = -958206105;
            /// <summary>
            /// Hash code of 'return' keyword
            /// </summary>
            public const int lxmReturn = 535626416;
            /// <summary>
            /// Hash code of 'void' keyword
            /// </summary>
            public const int lxmVoid = -1012413868;
            /// <summary>
            /// Hash code of 'true' keyword
            /// </summary>
            public const int lxmTrue = 1339027022;
            /// <summary>
            /// Hash code of 'false' keyword
            /// </summary>
            public const int lxmFalse = 1070720931;
            /// <summary>
            /// Hash code of 'boolean' keyword
            /// </summary>
            public const int lxmBoolean = -920214360;
            /// <summary>
            /// Hash code of 'real' keyword
            /// </summary>
            public const int lxmReal = -620244386;
            /// <summary>
            /// Hash code of 'string' keyword
            /// </summary>
            public const int lxmString = 1094552529;
            /// <summary>
            /// Hash code of 'callable' keyword
            /// </summary>
            public const int lxmCallable = 357883704;
            /// <summary>
            /// Hash code of 'do' keyword
            /// </summary>
            public const int lxmDo = 99211;
            /// <summary>
            /// Hash code of 'while' keyword
            /// </summary>
            public const int lxmWhile = 1612899761;
            /// <summary>
            /// Hash code of 'groupby' keyword
            /// </summary>
            public const int lxmGroupBy = -951552554;
            /// <summary>
            /// Hash code of 'else' keyword
            /// </summary>
            public const int lxmElse = -380599623;
            /// <summary>
            /// Hash code of 'fault' keyword
            /// </summary>
            public const int lxmFault = 1079552738;
            /// <summary>
            /// Hash code of 'try' keyword
            /// </summary>
            public const int lxmTry = 114034491;
            /// <summary>
            /// Hash code of 'finally' keyword
            /// </summary>
            public const int lxmFinally = -495501437;
            /// <summary>
            /// Hash code of 'checked' keyword
            /// </summary>
            public const int lxmChecked = 1075478823;
            /// <summary>
            /// Hash code of 'unchecked' keyword
            /// </summary>
            public const int lxmUnchecked = -358083090;
            /// <summary>
            /// Hash code of 'caseof' keyword
            /// </summary>
            public const int lxmCaseof = 569427655;
            /// <summary>
            /// Hash code of 'fork' keyword
            /// </summary>
            public const int lxmFork = 595587906;
            /// <summary>
            /// Hash code of 'dimensional' keyword
            /// </summary>
            public const int lxmDimensional = 846474449;
            /// <summary>
            /// Hash code of 'async' keyword
            /// </summary>
            public const int lxmAsync = -2040171972;
            /// <summary>
            /// Hash code of 'expr' keyword
            /// </summary>
            public const int lxmExpr = -368817611;
            /// <summary>
            /// Hash code of 'stmt' keyword
            /// </summary>
            public const int lxmStmt = 367741000;
            /// <summary>
            /// Hash code of 'expandq' keyword
            /// </summary>
            public const int lxmExpandq = 1009504215;
            /// <summary>
            /// Hash code of 'global' keyword
            /// </summary>
            public const int lxmGlobal = 301049955;
            /// <summary>
            /// Hash code of 'base' keyword
            /// </summary>
            public const int lxmBase = 983837969;
        }
        #endregion
        private Keyword(string keyword)
            : base(keyword)
        {
        }

        /// <summary>
        /// Represents variable declarator.
        /// </summary>
        public static readonly Keyword Var = new Keyword("var");

        /// <summary>
        /// Represents constant declarator.
        /// </summary>
        public static readonly Keyword Const = new Keyword("const");

        /// <summary>
        /// Represents type-check operator.
        /// </summary>
        public static readonly Keyword Is = new Keyword("is");

        /// <summary>
        /// Represents subset test.
        /// </summary>
        public static readonly Keyword In = new Keyword("in");

        /// <summary>
        /// Represents typecast operator.
        /// </summary>
        public static readonly Keyword To = new Keyword("to");

        /// <summary>
        /// Represents object type.
        /// </summary>
        public static readonly Keyword Object = new Keyword("object");

        /// <summary>
        /// Represents generic type.
        /// </summary>
        public static readonly Keyword Type = new Keyword("type");

        /// <summary>
        /// Represents 'this' keyword.
        /// </summary>
        public static readonly Keyword This = new Keyword("this");

        /// <summary>
        /// Represents integer data type.
        /// </summary>
        public static readonly Keyword Integer = new Keyword("integer");

        /// <summary>
        /// Represents for-loop.
        /// </summary>
        public static readonly Keyword For = new Keyword("for");

        /// <summary>
        /// Represents instruction that pass the program control out of the lexical scope.
        /// </summary>
        /// <remarks>
        /// If this instruction is used to break the loop, return from the action.
        /// </remarks>
        public static readonly Keyword Leave = new Keyword("leave");

        /// <summary>
        /// Represents instruction that initiates a new loop iteration. 
        /// </summary>
        public static readonly Keyword Continue = new Keyword("continue");

        /// <summary>
        /// Represents instruction that breaks program flow of the action.
        /// </summary>
        public static readonly Keyword Return = new Keyword("return");

        /// <summary>
        /// Represents null value or type that represents an empty set.
        /// </summary>
        public static readonly Keyword Void = new Keyword("void");
        
        /// <summary>
        /// Represents 'true' constant expression.
        /// </summary>
        public static readonly Keyword True = new Keyword("true");

        /// <summary>
        /// Represents 'false' constant expression.
        /// </summary>
        public static readonly Keyword False = new Keyword("false");

        /// <summary>
        /// Represents boolean data type.
        /// </summary>
        public static readonly Keyword Boolean = new Keyword("boolean");

        /// <summary>
        /// Represents floating-point contract.
        /// </summary>
        public static readonly Keyword Real = new Keyword("real");

        /// <summary>
        /// Represents string contract.
        /// </summary>
        public static readonly Keyword String = new Keyword("string");

        /// <summary>
        /// Represents callable contract.
        /// </summary>
        public static readonly Keyword Callable = new Keyword("callable");

        /// <summary>
        /// Represents loop body indicator.
        /// </summary>
        public static readonly Keyword Do = new Keyword("do");

        /// <summary>
        /// Represents while loop.
        /// </summary>
        public static readonly Keyword While = new Keyword("while");

        /// <summary>
        /// Represents loop grouping method.
        /// </summary>
        public static readonly Keyword GroupBy = new Keyword("groupby");

        /// <summary>
        /// Represents if-else branch.
        /// </summary>
        public static readonly Keyword Else = new Keyword("else");

        /// <summary>
        /// Represents expression throwing statement keyword.
        /// </summary>
        public static readonly Keyword Fault = new Keyword("fault");

        /// <summary>
        /// Represents potentially dangerous code block statement.
        /// </summary>
        public static readonly Keyword Try = new Keyword("try");

        /// <summary>
        /// Represents finally block in SEH.
        /// </summary>
        public static readonly Keyword Finally = new Keyword("finally");

        /// <summary>
        /// Represents checked context identifier.
        /// </summary>
        public static readonly Keyword Checked = new Keyword("checked");

        /// <summary>
        /// Represents unchecked context identifier.
        /// </summary>
        public static readonly Keyword Unchecked = new Keyword("unchecked");

        /// <summary>
        /// Represents case handler.
        /// </summary>
        public static readonly Keyword Caseof = new Keyword("caseof");

        /// <summary>
        /// Represents asynchronous task producer.
        /// </summary>
        public static readonly Keyword Fork = new Keyword("fork");

        /// <summary>
        /// Represents dimensional contract.
        /// </summary>
        public static readonly Keyword Dimensional = new Keyword("dimensional");

        /// <summary>
        /// Represents 'async' keyword.
        /// </summary>
        public static readonly Keyword Async = new Keyword("async");

        /// <summary>
        /// Represents expression contract.
        /// </summary>
        public static readonly Keyword Expr = new Keyword("expr");

        /// <summary>
        /// Represents statement contract.
        /// </summary>
        public static readonly Keyword Stmt = new Keyword("stmt");

        /// <summary>
        /// Represents an operator that expands quouted expression.
        /// </summary>
        public static readonly Keyword Expandq = new Keyword("expandq");

        /// <summary>
        /// Represents reference to the global object.
        /// </summary>
        public static readonly Keyword Global = new Keyword("global");

        /// <summary>
        /// Represents reference to the parent scope object.
        /// </summary>
        public static readonly Keyword Base = new Keyword("base");

        internal static Keyword FromHashCode(int hashCode)
        {
            switch (hashCode)
            {

                case HashCodes.lxmVar: return Var;
                case HashCodes.lxmConst: return Const;
                case HashCodes.lxmIs: return Is;
                case HashCodes.lxmIn: return In;
                case HashCodes.lxmTo: return To;
                case HashCodes.lxmObject: return Object;
                case HashCodes.lxmType: return Type;
                case HashCodes.lxmThis: return This;
                case HashCodes.lxmInteger: return Integer;
                case HashCodes.lxmFor: return For;
                case HashCodes.lxmLeave: return Leave;
                case HashCodes.lxmContinue: return Continue;
                case HashCodes.lxmReturn: return Return;
                case HashCodes.lxmVoid: return Void;
                case HashCodes.lxmTrue: return True;
                case HashCodes.lxmFalse: return False;
                case HashCodes.lxmBoolean: return Boolean;
                case HashCodes.lxmReal: return Real;
                case HashCodes.lxmString: return String;
                case HashCodes.lxmCallable: return Callable;
                case HashCodes.lxmDo: return Do;
                case HashCodes.lxmWhile: return While;
                case HashCodes.lxmGroupBy: return GroupBy;
                case HashCodes.lxmElse: return Else;
                case HashCodes.lxmFault: return Fault;
                case HashCodes.lxmTry: return Try;
                case HashCodes.lxmFinally: return Finally;
                case HashCodes.lxmChecked: return Checked;
                case HashCodes.lxmUnchecked: return Unchecked;
                case HashCodes.lxmCaseof: return Caseof;
                case HashCodes.lxmFork: return Fork;
                case HashCodes.lxmDimensional: return Dimensional;
                case HashCodes.lxmAsync: return Async;
                case HashCodes.lxmExpr: return Expr;
                case HashCodes.lxmStmt: return Stmt;
                case HashCodes.lxmExpandq: return Expandq;
                case HashCodes.lxmGlobal: return Global;
                case HashCodes.lxmBase: return Base;
                default: return null;
            }
        }

        internal static Keyword FromString(string lexeme)
        {
            return FromHashCode(StringEqualityComparer.GetHashCode(lexeme));
        }

        /// <summary>
        /// Determines whether the specified string is a language keyword.
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns></returns>
        public static bool IsKeyword(string lexeme)
        {
            return FromString(lexeme) != null;
        }
    }
}
