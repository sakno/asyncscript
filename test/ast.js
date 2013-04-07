var assert = require('assert'), SyntaxAnalyzer = require('../lib/SyntaxAnalyzer.js'), ast = require('../lib/ast.js');

exports['integer'] = function(test){
	SyntaxAnalyzer.parse("integer;", function(err, tree){
		assert(tree instanceof ast.CodeBuiltInContractExpression);
		assert.strictEqual(tree.value, "integer");
		test.done();
	});
};

exports['integer literal'] = function(test){
	SyntaxAnalyzer.parse("10;", function(err, tree){
		assert(tree instanceof ast.CodeIntegerExpression);
		assert.strictEqual(tree.value, 10);
		test.done();
	});
};

exports['Binary expression'] = function(test){
	SyntaxAnalyzer.parse("10 + 2 * 3;", function(err, tree){
		assert(tree instanceof ast.CodeBinaryExpression);
		assert.strictEqual(tree.left.value, 10);
		assert(tree.right instanceof ast.CodeBinaryExpression);
		assert.strictEqual(tree.right.left.value, 2);
		assert.strictEqual(tree.right.right.value, 3);
		assert.strictEqual(tree.operator.value, "+");
		test.done();
	});
};

exports['Invocation'] = function(test){
	SyntaxAnalyzer.parse("a(b, c) => d;", function(err, tree){
		tree = tree.reduce(true);
		assert(tree instanceof ast.CodeInvocationExpression);
		assert.strictEqual(tree.target.name, "a");
		assert.strictEqual(tree.arguments.length, 2);
		assert.strictEqual(tree.arguments[0].name, "b");
		assert.strictEqual(tree.destination.name, "d"); 
		test.done();
	});
};

exports['Unary expression'] = function(test){
	SyntaxAnalyzer.parse("-20;", function(err, tree){
		assert(tree instanceof ast.CodeUnaryExpression);
		assert.strictEqual(tree.operand.value, 20);
		assert.strictEqual(tree.operator.value, "-");
		test.done();
	});
};

exports['Member invocation'] = function(test){
	SyntaxAnalyzer.parse("a.b(-10);", function(err, tree){
		assert(tree instanceof ast.CodeInvocationExpression);
		assert.strictEqual(tree.self.name, "a");
		assert.strictEqual(tree.arguments.length, 1);
		test.done();
	});
};

exports['Indexer'] = function(test){
	SyntaxAnalyzer.parse("a[b, c] => d;", function(err, tree){
		tree = tree.reduce(true);
		assert(tree instanceof ast.CodeIndexerExpression);
		assert.strictEqual(tree.target.name, "a");
		assert.strictEqual(tree.indicies.length, 2);
		assert.strictEqual(tree.indicies[0].name, "b");
		assert.strictEqual(tree.destination.name, "d"); 
		test.done();
	});
};

exports['Expansion'] = function(test){
	SyntaxAnalyzer.parse("expandq a(b, c);", function(err, tree){
		assert(tree instanceof ast.CodeExpansionExpression);
		assert.strictEqual(tree.target.name, "a");
		assert.strictEqual(tree.arguments.length, 2);
		assert.strictEqual(tree.arguments[0].name, "b");
		test.done();
	});
};

exports['Scope'] = function(test){
	SyntaxAnalyzer.parse("{a; b};", function(err, tree){
		assert(tree instanceof Array);
		assert.strictEqual(tree.length, 2);
		assert.strictEqual(tree[0].name, "a");
		test.done();
	});
};

exports['For'] = function(test){
	SyntaxAnalyzer.parse("for a in b -> {c; d};", function(err, tree){
		assert(tree instanceof ast.CodeForExpression);
		assert.strictEqual(tree.source.name, "b");
		assert.strictEqual(tree.loopVar.name, "a");
		assert.strictEqual(tree.body.length, 2);
		assert.strictEqual(tree.body[0].name, "c");
		test.done();
	});
};

exports['Switch-case'] = function(test){
	SyntaxAnalyzer.parse("a ?? ==: c, 0: d, any: e;", function(err, tree){
		assert(tree instanceof ast.CodeSwitcherExpression);
		assert.strictEqual(tree.comparer.name, "c");
		assert.strictEqual(tree.target.name, "a");
		assert.strictEqual(tree.cases.length, 1);
		assert.strictEqual(tree['else'].name, "e");
		test.done();
	});
};

exports['Conditional'] = function(test){
	SyntaxAnalyzer.parse("a ? b : c;", function(err, tree){
		assert(tree instanceof ast.CodeConditionalExpression);
		assert.strictEqual(tree.condition.name, "a");
		assert.strictEqual(tree['then'].name, "b");
		assert.strictEqual(tree['else'].name, "c");
		test.done();
	});
};

exports['SEH'] = function(test){
	SyntaxAnalyzer.parse("a !! b : c;", function(err, tree){
		assert(tree instanceof ast.CodeSehExpression);
		assert.strictEqual(tree['try'].name, "a");
		assert.strictEqual(tree['catch'].name, "b");
		assert.strictEqual(tree['finally'].name, "c");
		test.done();
	});
};

exports['Repeat'] = function(test){
	SyntaxAnalyzer.parse("repeat a -> b, +;", function(err, tree){
		assert(tree instanceof ast.CodeRepeatExpression);
		assert.strictEqual(tree.loopVar, "a");
		assert.strictEqual(tree.body.name, "b");
		assert.strictEqual(tree.aggregator, "+");
		test.done();
	});
};

exports['With'] = function(test){
	SyntaxAnalyzer.parse('with a, b in c -> d;', function(err, tree){
		assert(tree instanceof ast.CodeWithExpression);
		assert.strictEqual(tree.source.name, "c");
		assert.strictEqual(tree.fields.length, 2);
		assert.strictEqual(tree.fields[0], "a");
		assert.strictEqual(tree.body.name, "d");
		test.done();
	});
};

exports['Let'] = function(test){
	SyntaxAnalyzer.parse('let a = 2;', function(err, tree){
		assert(tree instanceof ast.CodeLetExpression);
		assert.strictEqual(tree.name, "a");
		assert.strictEqual(tree.value.value, 2);
		test.done();
	});
};

exports['Function'] = function(test){
	SyntaxAnalyzer.parse('@a, b: integer -> c;', function(err, tree){
		assert(tree instanceof ast.CodeFunctionExpression);
		assert.strictEqual(tree.parameters.length, 2);
		assert.strictEqual(tree.parameters[1].contract.value, "integer");
		test.done();
	});
};

exports['Signature'] = function(test){
	SyntaxAnalyzer.parse('@a, b: integer;', function(err, tree){
		assert(tree instanceof ast.CodeFunctionExpression);
		assert.strictEqual(tree.parameters.length, 2);
		assert.strictEqual(tree.parameters[1].contract.value, "integer");
		SyntaxAnalyzer.parse('@a: string, b: object;', function(err, tree){
			assert(tree instanceof ast.CodeFunctionExpression);
			assert.strictEqual(tree.parameters.length, 2);
			assert.strictEqual(tree.parameters[1].contract.value, "object");
			test.done();
		});
	});
};

exports['Braces'] = function(test){
	SyntaxAnalyzer.parse('(2+3) + 6;', function(err, tree){
		assert(tree instanceof ast.CodeBinaryExpression);
		assert.strictEqual(tree.right.value, 6);
		test.done();
	});
};

exports['Array contract'] = function(test){
	SyntaxAnalyzer.parse('integer[];', function(err, tree){
		assert(tree instanceof ast.CodeArrayContractExpression);
		assert.strictEqual(tree.element.value, "integer");
		test.done();
	});
};

exports['Quouted id'] = function(test){
	SyntaxAnalyzer.parse('`()`;', function(err, tree){
		assert(tree instanceof ast.CodeIdentifierExpression);
		assert.strictEqual(tree.name, "()");
		test.done();
	});
};

exports['Custom binary operator'] = function(test){
	SyntaxAnalyzer.parse('a contains b;', function(err, tree){
		assert(tree instanceof ast.CodeInvocationExpression);
		assert.strictEqual(tree.self.name, "a");
		assert.strictEqual(tree.method.name, "contains");
		assert.strictEqual(tree.arguments.length, 1);
		assert.strictEqual(tree.arguments[0].name, 'b');
		test.done();
	});
};
