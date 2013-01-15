require("../");
var assert = require('assert');

exports['sizeof function'] = function(test){
	$asyncscript.run("return sizeof(@a, b);", null, function(err, result){
		assert.strictEqual(result, 2);
		return test.done();
	});
};

exports['valueof function'] = function(test){
	$asyncscript.run("return valueof(let r{get 2});", null, function(err, result){
		assert.strictEqual(result, 2);
		return test.done();
	});
};

exports['isError function'] = function(test){
	$asyncscript.run("let p = async object; fault 10 => p; return isError(p);", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['isError function'] = function(test){
	$asyncscript.run("let p = async object; fault 10 => p; return isError(p);", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['isCompleted function'] = function(test){
	$asyncscript.run("let p = async object; return 10 => p; return isCompleted(p);", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['isPromise function'] = function(test){
	$asyncscript.run("let p = async object; return isPromise(p);", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['evaluate function'] = function(test){
	$asyncscript.run("return evaluate('return 42;');", null, function(err, result){
		assert.strictEqual(result, 42);
		return test.done();
	});
};

exports['evaluate function'] = function(test){
	$asyncscript.run("return evaluate('return 42;');", null, function(err, result){
		assert.strictEqual(result, 42);
		return test.done();
	});
};
