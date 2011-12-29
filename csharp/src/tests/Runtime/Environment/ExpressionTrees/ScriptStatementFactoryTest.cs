using System;
using NUnit.Framework;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Testing;
    using Compiler.Ast;

    [SemanticTest]
    [TestClass(typeof(ScriptStatementFactory))]
    sealed class ScriptStatementFactoryTest: SemanticTestBase
    {
        [Test(Description = "Statement contract parsing test.")]
        public void ContractTest()
        {
            Assert.IsTrue(ReferenceEquals(ScriptStatementFactory.Instance, Run("return stmt;")));
        }

        [Test(Description = "Test for fault statement contract.")]
        public void FaultContractTest()
        {
            Assert.IsTrue(ReferenceEquals(ScriptStatementFactory.Fault, Run("return stmt.`fault;")));
        }

        [Test(Description = "Test for fault statement execution.")]
        [ExpectedException(typeof(ScriptFault))]
        public void FaultExecutionTest()
        {
            Run("const f = stmt.`fault('script exception'); stmt.`fault.execute(f, void);");
        }

        [Test(Description = "GetError action invocation test.")]
        public void GetErrorActionTest()
        {
            var error = Run("const f = stmt.`fault('script exception'); return expr.compile(stmt.`fault.error(f));");
            Assert.IsTrue(Equals(new ScriptString("script exception"), error));
        }

        [Test(Description = "Test for CONTINUE statement contract.")]
        public void ContinueContractTest()
        {
            Assert.IsTrue(ReferenceEquals(ScriptStatementFactory.Continue, Run("return stmt.`continue;")));
        }

        [Test(Description = "Test for CONTINUE statement compilation.")]
        public void ContinueArgsActionTest()
        {
            string arg0 = Run("const f = stmt.`continue(['Hello, world!']); return expr.compile(stmt.`continue.args(f)[0]);");
            Assert.AreEqual("Hello, world!", arg0);
        }

        [Test(Description = "Test for LEAVE statement contract.")]
        public void LeaveContractTest()
        {
            Assert.IsTrue(ReferenceEquals(ScriptStatementFactory.Leave, Run("return stmt.`leave;")));
        }

        [Test(Description = "Test for LEAVE statement compilation.")]
        public void LeaveArgsActionTest()
        {
            var arg0 = Run("const f = stmt.`leave(['Hello, world!']); return expr.compile(stmt.`leave.args(f)[0]);");
            Assert.IsTrue(Equals(new ScriptString("Hello, world!"), arg0));
        }

        [Test(Description = "Test for RETURN statement contract.")]
        public void ReturnContractTest()
        {
            Assert.IsTrue(ReferenceEquals(ScriptStatementFactory.Return, Run("return stmt.`return;")));
        }

        [Test(Description = "Test for RETURN statement compilation.")]
        public void ReturnGetValueActionTest()
        {
            var arg0 = Run("const f = stmt.`return('Hello, world!'); return expr.compile(stmt.`return.value(f));");
            Assert.IsTrue(Equals(new ScriptString("Hello, world!"), arg0));
        }
    }
}
