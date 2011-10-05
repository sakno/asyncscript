using System;

namespace DynamicScript.Compiler.Ast
{
    using SyntaxTestAttribute = DynamicScript.Testing.SyntaxTestAttribute;
    using Assert = NUnit.Framework.Assert;
    using CodeStatement = System.CodeDom.CodeStatement;

    [SyntaxTest]
    abstract class SyntaxTreeTestBase
    {
        protected static void AreEqual(ScriptCodeExpression expression1, ScriptCodeExpression expression2)
        {
            Assert.AreEqual(expression1, expression2);
        }

        protected static void AreEqual(CodeStatement statement1, CodeStatement statement2)
        {
            Assert.AreEqual(statement1, statement2);
        }

        protected static void AreNotEqual(ScriptCodeExpression expression1, ScriptCodeExpression expression2)
        {
            Assert.AreNotEqual(expression1, expression2);
        }

        protected static void AreNotEqual(CodeStatement statement1, CodeStatement statement2)
        {
            Assert.AreNotEqual(statement1, statement2);
        }

        protected static void TheSameHashCodes(ScriptCodeExpression expression1, ScriptCodeExpression expression2)
        {
            Assert.IsNotNull(expression1);
            Assert.IsNotNull(expression2);
            Assert.AreEqual(expression1.GetHashCode(), expression2.GetHashCode());
        }

        protected static void TheSameHashCodes(CodeStatement statement1, CodeStatement statement2)
        {
            Assert.IsNotNull(statement1);
            Assert.IsNotNull(statement2);
            Assert.AreEqual(statement1.GetHashCode(), statement2.GetHashCode());
        }

        protected static void AreEqual(ScriptCodeExpression expression, string scriptCode)
        {
            Assert.IsNotNull(expression);
            Assert.IsNotNull(scriptCode);
            Assert.AreEqual(scriptCode, expression.ToString());
        }

        protected static void AreEqual(CodeStatement statement, string scriptCode)
        {
            Assert.IsNotNull(statement);
            Assert.IsNotNull(scriptCode);
            Assert.AreEqual(statement.ToString(), scriptCode);
        }

    }
}
