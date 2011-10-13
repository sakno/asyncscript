using System;
using NUnit.Framework;

namespace DynamicScript.Compiler.Ast
{
    using SyntaxTestAttribute = Testing.SyntaxTestAttribute;

    [SyntaxTest]
    [TestFixture(Description="Tests for Abstract Syntax Tree classes.")]
    sealed class AbstractSyntaxTreeTests : SyntaxTreeTestBase
    {
        #region Literals
        [Test(Description="Test for integer literal AST.")]
        public void IntegerLiteralTest()
        {
            TheSameHashCodes(new ScriptCodeIntegerExpression(20L), new ScriptCodeIntegerExpression(20L));
            AreEqual(new ScriptCodeIntegerExpression(20L), new ScriptCodeIntegerExpression(20L));
            AreNotEqual(new ScriptCodeIntegerExpression(21L), new ScriptCodeIntegerExpression(20L));
            AreEqual(new ScriptCodeIntegerExpression(22L), "22");
        }

        [Test(Description = "Test for real literal AST.")]
        public void RealLiteralTest()
        {
            TheSameHashCodes(new ScriptCodeRealExpression(22), new ScriptCodeIntegerExpression(22));
            AreEqual(new ScriptCodeRealExpression(22), new ScriptCodeRealExpression(22));
            AreNotEqual(new ScriptCodeRealExpression(21), new ScriptCodeRealExpression(20));
            AreEqual(new ScriptCodeRealExpression(22.12), "22.12");
        }

        [Test(Description = "Test for boolean literal AST.")]
        public void BooleanLiteralTest()
        {
            TheSameHashCodes(new ScriptCodeBooleanExpression(true), new ScriptCodeBooleanExpression(true));
            AreEqual(new ScriptCodeBooleanExpression(true), new ScriptCodeBooleanExpression(true));
            AreNotEqual(new ScriptCodeBooleanExpression(true), new ScriptCodeBooleanExpression(false));
            AreEqual(new ScriptCodeBooleanExpression(true), "true");
        }

        [Test(Description = "Test for string literal AST.")]
        public void StringLiteralTest()
        {
            const string TestString = "Hello, world!";
            TheSameHashCodes(new ScriptCodeStringExpression(TestString), new ScriptCodeStringExpression(TestString));
            AreEqual(new ScriptCodeStringExpression(TestString), new ScriptCodeStringExpression(TestString));
            AreNotEqual(new ScriptCodeStringExpression(TestString), new ScriptCodeStringExpression(TestString + "!"));
            AreEqual(new ScriptCodeStringExpression(TestString), "'" + TestString + "'");
        }
        #endregion
        #region Built-in contracts

        [Test(Description="Test for integer contract AST representation.")]
        public void IntegerContractTest()
        {
            TheSameHashCodes(ScriptCodeIntegerContractExpression.Instance, ScriptCodeIntegerContractExpression.Instance);
            AreEqual(ScriptCodeIntegerContractExpression.Instance, ScriptCodeIntegerContractExpression.Instance);
            AreNotEqual(ScriptCodeIntegerContractExpression.Instance, ScriptCodeBooleanContractExpression.Instance);
            AreEqual(ScriptCodeIntegerContractExpression.Instance, "integer");
        }

        [Test(Description = "Test for real contract AST representation.")]
        public void RealContractTest()
        {
            TheSameHashCodes(ScriptCodeRealContractExpression.Instance, ScriptCodeRealContractExpression.Instance);
            AreEqual(ScriptCodeRealContractExpression.Instance, ScriptCodeRealContractExpression.Instance);
            AreNotEqual(ScriptCodeRealContractExpression.Instance, ScriptCodeBooleanContractExpression.Instance);
            AreEqual(ScriptCodeRealContractExpression.Instance, "real");
        }

        [Test(Description = "Test for boolean contract AST representation.")]
        public void BooleanContractTest()
        {
            TheSameHashCodes(ScriptCodeBooleanContractExpression.Instance, ScriptCodeBooleanContractExpression.Instance);
            AreEqual(ScriptCodeBooleanContractExpression.Instance, ScriptCodeBooleanContractExpression.Instance);
            AreNotEqual(ScriptCodeBooleanContractExpression.Instance, ScriptCodeRealContractExpression.Instance);
            AreEqual(ScriptCodeBooleanContractExpression.Instance, "boolean");
        }

        [Test(Description = "Test for super contract AST representation.")]
        public void SuperContractTest()
        {
            TheSameHashCodes(ScriptCodeSuperContractExpression.Instance, ScriptCodeSuperContractExpression.Instance);
            AreEqual(ScriptCodeSuperContractExpression.Instance, ScriptCodeSuperContractExpression.Instance);
            AreNotEqual(ScriptCodeSuperContractExpression.Instance, ScriptCodeRealContractExpression.Instance);
            AreEqual(ScriptCodeSuperContractExpression.Instance, "object");
        }

        [Test(Description = "Test for meta contract AST representation.")]
        public void MetaContractTest()
        {
            TheSameHashCodes(ScriptCodeMetaContractExpression.Instance, ScriptCodeMetaContractExpression.Instance);
            AreEqual(ScriptCodeMetaContractExpression.Instance, ScriptCodeMetaContractExpression.Instance);
            AreNotEqual(ScriptCodeSuperContractExpression.Instance, ScriptCodeMetaContractExpression.Instance);
            AreEqual(ScriptCodeMetaContractExpression.Instance, "type");
        }

        [Test(Description = "Test for finset contract AST representation.")]
        public void FinsetContractTest()
        {
            TheSameHashCodes(ScriptCodeFinSetContractExpression.Instance, ScriptCodeFinSetContractExpression.Instance);
            AreEqual(ScriptCodeFinSetContractExpression.Instance, ScriptCodeFinSetContractExpression.Instance);
            AreNotEqual(ScriptCodeSuperContractExpression.Instance, ScriptCodeFinSetContractExpression.Instance);
            AreEqual(ScriptCodeFinSetContractExpression.Instance, "finset");
        }

        [Test(Description = "Test for dimensional contract AST representation.")]
        public void DimensionalContractTest()
        {
            TheSameHashCodes(ScriptCodeDimensionalContractExpression.Instance, ScriptCodeDimensionalContractExpression.Instance);
            AreEqual(ScriptCodeDimensionalContractExpression.Instance, ScriptCodeDimensionalContractExpression.Instance);
            AreNotEqual(ScriptCodeSuperContractExpression.Instance, ScriptCodeDimensionalContractExpression.Instance);
            AreEqual(ScriptCodeDimensionalContractExpression.Instance, "dimensional");
        }

        [Test(Description = "Test for callable contract AST representation.")]
        public void CallableContractTest()
        {
            TheSameHashCodes(ScriptCodeCallableContractExpression.Instance, ScriptCodeCallableContractExpression.Instance);
            AreEqual(ScriptCodeCallableContractExpression.Instance, ScriptCodeCallableContractExpression.Instance);
            AreNotEqual(ScriptCodeSuperContractExpression.Instance, ScriptCodeCallableContractExpression.Instance);
            AreEqual(ScriptCodeCallableContractExpression.Instance, "callable");
        }

        [Test(Description = "Test for expr contract AST representation.")]
        public void ExprContractTest()
        {
            TheSameHashCodes(ScriptCodeExpressionContractExpression.Instance, ScriptCodeExpressionContractExpression.Instance);
            AreEqual(ScriptCodeExpressionContractExpression.Instance, ScriptCodeExpressionContractExpression.Instance);
            AreNotEqual(ScriptCodeSuperContractExpression.Instance, ScriptCodeExpressionContractExpression.Instance);
            AreEqual(ScriptCodeExpressionContractExpression.Instance, "expr");
        }

        [Test(Description = "Test for stmt contract AST representation.")]
        public void StmtContractTest()
        {
            TheSameHashCodes(ScriptCodeStatementContractExpression.Instance, ScriptCodeStatementContractExpression.Instance);
            AreEqual(ScriptCodeStatementContractExpression.Instance, ScriptCodeStatementContractExpression.Instance);
            AreNotEqual(ScriptCodeSuperContractExpression.Instance, ScriptCodeStatementContractExpression.Instance);
            AreEqual(ScriptCodeStatementContractExpression.Instance, "stmt");
        }
        #endregion
        #region Statements

        [Test(Description = "Variable declaration test of its AST representation.")]
        public void VariableDeclarationTest()
        {
            var declaration = new ScriptCodeVariableDeclaration
            {
                IsConst = true,
                ContractBinding = ScriptCodeFinSetContractExpression.Instance,
                Name = "v",
                InitExpression = ScriptCodeVoidExpression.Instance
            }; 
            TheSameHashCodes(declaration, new ScriptCodeVariableDeclaration
            {
                IsConst = true,
                ContractBinding = ScriptCodeFinSetContractExpression.Instance,
                Name = "v",
                InitExpression = ScriptCodeVoidExpression.Instance
            });
            AreEqual(declaration, new ScriptCodeVariableDeclaration
            {
                IsConst = true,
                ContractBinding = ScriptCodeFinSetContractExpression.Instance,
                Name = "v",
                InitExpression = ScriptCodeVoidExpression.Instance
            });
            AreNotEqual(declaration, new ScriptCodeVariableDeclaration
            {
                IsConst = false,
                ContractBinding = ScriptCodeFinSetContractExpression.Instance,
                Name = "v",
                InitExpression = ScriptCodeVoidExpression.Instance
            });
            AreEqual(declaration, "const v=void:finset;");
        }

        [Test(Description = "Fault statement test of its AST representation.")]
        public void FaultTest()
        {
            var fault = new ScriptCodeFaultStatement();
            fault.Error = new ScriptCodeIntegerExpression(2);
            var otherFault = new ScriptCodeFaultStatement();
            otherFault.Error = new ScriptCodeStringExpression("2");
            AreNotEqual(fault, otherFault);
            AreEqual(fault, "fault 2;");
        }

        [Test(Description = "Continue statement test of its AST representation.")]
        public void ContinueTest()
        {
            var @continue = new ScriptCodeContinueStatement();
            @continue.ArgList.Add(new ScriptCodeStringExpression("Hello"));
            @continue.ArgList.Add(new ScriptCodeStringExpression("world"));
            var otherContinue = new ScriptCodeContinueStatement();
            otherContinue.ArgList.Add(new ScriptCodeIntegerExpression(2));
            AreNotEqual(@continue, otherContinue);
            AreEqual(@continue, "continue 'Hello','world';");
        }

        [Test(Description = "Return statement test of its AST representation.")]
        public void ReturnTest()
        {
            var @return = new ScriptCodeReturnStatement();
            @return.Value = new ScriptCodeIntegerExpression(2);
            var otherReturn = new ScriptCodeReturnStatement();
            otherReturn.Value = new ScriptCodeStringExpression("2");
            AreNotEqual(@return, otherReturn);
            AreEqual(@return, "return 2;");
        }

        [Test(Description = "Leave statement test of its AST representation.")]
        public void LeaveTest()
        {
            var leave = new ScriptCodeBreakLexicalScopeStatement();
            leave.ArgList.Add(new ScriptCodeIntegerExpression(2));
            var otherLeave = new ScriptCodeBreakLexicalScopeStatement();
            otherLeave.ArgList.Add(new ScriptCodeStringExpression("2"));
            AreNotEqual(leave, otherLeave);
            AreEqual(leave, "leave 2;");
        }
        #endregion

        #region Expressions
        [Test(Description = "Test for name token AST representation.")]
        public void VariableReferenceTest()
        {
            const string VariableName = "someVar";
            var reference = new ScriptCodeVariableReference { VariableName = VariableName };
            AreEqual(reference, new ScriptCodeVariableReference { VariableName = "SOMEVAR" });
            TheSameHashCodes(reference, new ScriptCodeVariableReference { VariableName = "SOMEVAR" });
            AreNotEqual(reference, new ScriptCodeVariableReference { VariableName = "V" });
            AreEqual(reference, "somevar");
        }

        [Test(Description = "Test for conditional expression tree.")]
        public void ConditionalTest()
        {
            var thenBranch = new ScriptCodeComplexExpression();
            var elseBranch = new ScriptCodeComplexExpression();
            var conditional = new ScriptCodeConditionalExpression(new ScriptCodeIntegerExpression(10), thenBranch, elseBranch);
            thenBranch.Add(new ScriptCodeVariableDeclaration { Name = "a", InitExpression = new ScriptCodeIntegerExpression(2) });
            elseBranch.Add((ScriptCodeExpressionStatement)new ScriptCodeBinaryOperatorExpression
            {
                Left = new ScriptCodeVariableReference { VariableName = "a" },
                Operator = ScriptCodeBinaryOperatorType.Assign,
                Right = new ScriptCodeIntegerExpression(10)
            });
            elseBranch.Add((ScriptCodeExpressionStatement)new ScriptCodeIntegerExpression(20));
            AreEqual(conditional, "if 10 then {\r\nvar a=2;\r\na=10;\r\n}\r\n else 20");
        }

        [Test(Description = "Test for SEH expression tree.")]
        public void TryElseFinallyTest()
        {
            var seh = new ScriptCodeTryElseFinallyExpression();
            seh.DangerousCode.Expression = new ScriptCodeComplexExpression { new ScriptCodeFaultStatement { Error = new ScriptCodeIntegerExpression(0) } };
            var trap = new ScriptCodeTryElseFinallyExpression.FailureTrap
            {
                Filter = new ScriptCodeVariableDeclaration { ContractBinding = ScriptCodeIntegerContractExpression.Instance, Name = "e" }
            };
            trap.Handler.Expression = new ScriptCodeIntegerExpression(10);
            seh.Traps.Add(trap);
            trap = new ScriptCodeTryElseFinallyExpression.FailureTrap();
            trap.Handler.Expression = new ScriptCodeStringExpression("str");
            seh.Traps.Add(trap);
            AreEqual(seh, "try {fault 0;} else (var e:integer)\r\n10 else 'str'");
        }
        #endregion
    }
}
