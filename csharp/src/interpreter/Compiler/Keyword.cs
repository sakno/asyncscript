using System;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents keyword token.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    sealed class Keyword : Token
    {
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
        /// Represents conditional expression.
        /// </summary>
        public static readonly Keyword If = new Keyword("if");

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
        /// Represents if-then branch.
        /// </summary>
        public static readonly Keyword Then = new Keyword("then");

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
        /// Represents synchronization expression.
        /// </summary>
        public static readonly Keyword Await = new Keyword("await");

        /// <summary>
        /// Represents finset contract.
        /// </summary>
        public static readonly Keyword FinSet = new Keyword("finset");

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
    }
}
