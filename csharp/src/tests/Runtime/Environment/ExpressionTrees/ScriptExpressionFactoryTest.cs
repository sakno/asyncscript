using System;
using NUnit.Framework;
using DynamicScript.Testing;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;

    [SemanticTest]
    [TestClass(typeof(ScriptExpressionFactory))]
    sealed class ScriptExpressionFactoryTest: SemanticTestBase
    {
        [Test(Description = "@ expression test.")]
        public void CurrentActionExpressionContractTest()
        {
            Assert.IsTrue(ReferenceEquals(ScriptExpressionFactory.CurrentAction, Run("return expr.ca;")));
        }

        [Test(Description = "Expression equality test.")]
        public void ExpressionEqualityTest()
        {
            var result = Run("return expr.equ(expr.constant(2), expr.constant(2));");
            Assert.IsTrue(result);
        }

        [Test(Description ="Expression equality-by-reference test.")]
        public void ExpressionReferenceEqualityTest()
        {
            var result = Run("return expr.requ(expr.constant(2), expr.constant(2));");
            Assert.IsFalse(result);
        }

        [Test(Description = "Expression contract parsing test.")]
        public void ContractTest()
        {
            Assert.IsTrue(ReferenceEquals(ScriptExpressionFactory.Instance, Run("return expr;")));
        }

        [Test(Description="Test for constant expression contract.")]
        public void ConstantTest()
        {
            Assert.IsTrue(ReferenceEquals(ScriptExpressionFactory.Constant, Run("return expr.constant;")));
        }

        [Test(Description="Test for constant expression compilation.")]
        public void ConstantCompilationTest()
        {
            var value = Run("return expr.compile(expr.constant(2));");
            Assert.AreEqual(2, value);
        }

        [Test(Description="Test for constant expression construction.")]
        public void ConstantConversionTest()
        {
            IScriptExpression<ScriptCodePrimitiveExpression> constant = Run("return 10 to expr.constant;");
            Assert.AreEqual(new ScriptCodeIntegerExpression(10), constant.CodeObject);
        }

        [Test(Description = "Test for binary operation contract.")]
        public void BinaryOperationContractTest()
        {
            Assert.IsTrue(ReferenceEquals(ScriptExpressionFactory.Binary, Run("return expr.binop;")));
        }

        [Test(Description = "Binary operatio compilation.")]
        public void BinaryOperationCompilationTest()
        {
            var r = Run("const tree= expr.binop(expr.binop(expr.constant(2), '+', expr.constant(3)), '*', expr.constant(5)); return expr.compile(tree);");
            Assert.AreEqual(25, r);
            r = Run("const tree = expr.binop(2, '/', 5); return expr.binop.operator(tree);");
            Assert.IsTrue(Equals(new ScriptString("/"), r));
        }

        [Test(Description = "Test for unary operation contract.")]
        public void UnaryOperationContractTest()
        {
            Assert.IsTrue(ReferenceEquals(ScriptExpressionFactory.Unary, Run("return expr.unop;")));
        }

        [Test(Description = "Test for async expression contract.")]
        public void AsyncExpressionContractTest()
        {
            Assert.IsTrue(ReferenceEquals(ScriptExpressionFactory.Async, Run("return expr.asyncdef;")));
        }
    }
}
