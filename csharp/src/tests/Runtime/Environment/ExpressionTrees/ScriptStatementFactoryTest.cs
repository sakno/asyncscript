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
            Assert.IsTrue(ReferenceEquals(ScriptStatementFactory.Fault, Run("return stmt.faultdef;")));
        }

        [Test(Description = "Test for fault statement execution.")]
        [ExpectedException(typeof(ScriptFault))]
        public void FaultExecutionTest()
        {
            Run("const f = stmt.faultdef('script exception'); stmt.faultdef.exec(f, void);");
        }

        [Test(Description = "GetError action invocation test.")]
        public void GetErrorActionTest()
        {
            var error = Run("const f = stmt.faultdef('script exception'); return expr.compile(stmt.faultdef.error(f));");
            Assert.IsTrue(Equals(new ScriptString("script exception"), error));
        }

        [Test(Description = "Test for CONTINUE statement contract.")]
        public void ContinueContractTest()
        {
            Assert.IsTrue(ReferenceEquals(ScriptStatementFactory.Continue, Run("return stmt.continuedef;")));
        }

        [Test(Description = "Test for CONTINUE statement compilation.")]
        public void ContinueArgsActionTest()
        {
            string arg0 = Run("const f = stmt.continuedef(['Hello, world!']); return expr.compile(stmt.continuedef.args(f)[0]);");
            Assert.AreEqual("Hello, world!", arg0);
        }

        [Test(Description = "Test for LEAVE statement contract.")]
        public void LeaveContractTest()
        {
            Assert.IsTrue(ReferenceEquals(ScriptStatementFactory.Leave, Run("return stmt.leavedef;")));
        }

        [Test(Description = "Test for LEAVE statement compilation.")]
        public void LeaveArgsActionTest()
        {
            var arg0 = Run("const f = stmt.leavedef(['Hello, world!']); return expr.compile(stmt.leavedef.args(f)[0]);");
            Assert.IsTrue(Equals(new ScriptString("Hello, world!"), arg0));
        }

        [Test(Description = "Test for RETURN statement contract.")]
        public void ReturnContractTest()
        {
            Assert.IsTrue(ReferenceEquals(ScriptStatementFactory.Return, Run("return stmt.returndef;")));
        }

        [Test(Description = "Test for RETURN statement compilation.")]
        public void ReturnGetValueActionTest()
        {
            var arg0 = Run("const f = stmt.returndef('Hello, world!'); return expr.compile(stmt.returndef.value(f));");
            Assert.IsTrue(Equals(new ScriptString("Hello, world!"), arg0));
        }
    }
}
