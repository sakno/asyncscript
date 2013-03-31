require("../");
var assert = require('assert');

//invocations
exports['Lambda'] = function(test){
	$asyncscript.run("return (@a, b -> a + b)(2, 3);", null, function(err, result){
		assert.strictEqual(result, 5);
		return test.done();
	});
};

exports['JavaScript function'] = function(test){
	$asyncscript.run("return Math.abs(-10);", null, function(err, result){
		assert.strictEqual(result, 10);
		return test.done();
	});
};

exports['String literal'] = function(test){
	$asyncscript.run("return '123'();", null, function(err, result){
		assert.equal(result, '123');
		return test.done();
	});
};

exports['Boolean literal'] = function(test){
	$asyncscript.run("return false();", null, function(err, result){
		assert.equal(result, false);
		return test.done();
	});
};

exports['Integer literal'] = function(test){
	$asyncscript.run("return 12();", null, function(err, result){
		assert.equal(result, 12);
		return test.done();
	});
};

exports['Real literal'] = function(test){
	$asyncscript.run("return 12.2();", null, function(err, result){
		assert.strictEqual(result, 12.2);
		return test.done();
	});
};

exports['String contract'] = function(test){
	$asyncscript.run("return string(12);", null, function(err, result){
		assert.strictEqual(result, "12");
		return test.done();
	});
};

exports['Integer contract'] = function(test){
	$asyncscript.run("return integer(12.1);", null, function(err, result){
		assert.strictEqual(result, 12);
		return test.done();
	});
};

exports['Typedef contract'] = function(test){
	$asyncscript.run("return typedef(real);", null, function(err, result){
		assert.strictEqual(result, Number);
		return test.done();
	});
};

exports['Typedef filter contract'] = function(test){
	$asyncscript.run("return typedef.filter(@a: integer -> a % 2 == 0);", null, function(err, result){
		assert(result instanceof $asyncscript.FilterContract);
		return test.done();
	});
};

exports['Container contract invocation'] = function(test){
	$asyncscript.run("return <<integer, integer>>(1, 2);", null, function(err, result){
		assert(result.__$c$__);
		assert(result.__$size$__, 2);
		assert.strictEqual(result[0], 1);
		assert.strictEqual(result[1], 2);
		return test.done();
	});
};

exports['Array contract invocation'] = function(test){
	$asyncscript.run("return (integer ^ 2)(1, 2);", null, function(err, result){
		assert(result.__$c$__);
		assert(result.__$size$__, 2);
		assert.strictEqual(result[0], 1);
		assert.strictEqual(result[1], 2);
		return test.done();
	});
};

exports['Signature invocation'] = function(test){
	$asyncscript.run("return (@a, b)('_0 + _1');", null, function(err, result){
		assert(result.isStandaloneLambda);
		assert(result instanceof Function);
		assert(result.__$contract$__ instanceof $asyncscript.Signature);
		assert.strictEqual(result.__$contract$__.__$size$__, 2);
		return test.done();
	});
};

exports['Function contract invocation'] = function(test){
	$asyncscript.run("return function('_0 + _1', true, integer, integer);", null, function(err, result){
		assert(result.isStandaloneLambda);
			assert(result instanceof Function);
			assert(result.__$contract$__ instanceof $asyncscript.Signature);
			assert.strictEqual(result.__$contract$__.__$size$__, 2);
			return test.done();
	});
};

exports['Method invocation'] = function(test){
	$asyncscript.run("return < let a = 1, let b = 2, let sum = @-> this.a + this.b >.sum();", null, function(err, result){
		assert.strictEqual(result, 3);
			return test.done();
	});
};

exports['Invocation overloading'] = function(test){
	$asyncscript.run("return < let `()` = @a, b -> a + b > (10, 12);", null, function(err, result){
		assert.strictEqual(result, 22);
			return test.done();
	});
};
