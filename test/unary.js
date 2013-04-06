require("../");
var assert = require('assert');

exports['Plus(integer)'] = function(test){
	$asyncscript.run("return +3;", null, function(err, result){
		assert.equal(result, 3);
		return test.done();
	});
};

exports['Typeof(integer)'] = function(test){
	$asyncscript.run("return $3;", null, function(err, result){
		assert.strictEqual(result, $asyncscript.integer);
			return test.done();	
	});
};

exports['Minus(integer)'] = function(test){
	$asyncscript.run("return -3;", null, function(err, result){
		assert.equal(result, -3);
			return test.done();	
	});
};

exports['Typeof(real)'] = function(test){
	$asyncscript.run("return $3.1;", null, function(err, result){
		assert.strictEqual(result, Number);
			return test.done();	
	});
};

exports['Typeof(void)'] = function(test){
	$asyncscript.run("return $void;", null, function(err, result){
		assert.strictEqual(result, null);
			return test.done();
	});
};

exports['Typeof(string)'] = function(test){
	$asyncscript.run("return $'123';", null, function(err, result){
		assert.strictEqual(result, String);
			return test.done();	
	});
};

exports['Typeof(boolean)'] = function(test){
	$asyncscript.run("return $true;", null, function(err, result){
		assert.strictEqual(result, Boolean);
			return test.done();
	});
};

exports['Typeof(typedef = integer)'] = function(test){
	$asyncscript.run("return $integer;", null, function(err, result){
		assert.strictEqual(result, $asyncscript.typedef);
			return test.done();	
	});
};

exports['Typeof(typedef = object)'] = function(test){
	$asyncscript.run("return $object;", null, function(err, result){
		assert.strictEqual(result, $asyncscript.typedef);
			return test.done();
	});
};

exports['Typeof(typedef = string)'] = function(test){
	$asyncscript.run("return $string;", null, function(err, result){
		assert.strictEqual(result, $asyncscript.typedef);
			return test.done();	
	});
};

exports['Typeof(typedef = boolean)'] = function(test){
	$asyncscript.run("return $boolean;", null, function(err, result){
		assert.strictEqual(result, $asyncscript.typedef);
			return test.done();	
	});
};

exports['Typeof(typedef = function)'] = function(test){
	$asyncscript.run("return $function;", null, function(err, result){
		assert.strictEqual(result, $asyncscript.typedef);
			return test.done();	
	});
};

exports['Typeof(typedef = << >>)'] = function(test){
	$asyncscript.run("return $<<object, object>>;", null, function(err, result){
		assert.strictEqual(result, $asyncscript.typedef);
			return test.done();	
	});
};

exports['Typeof(<< >>)'] = function(test){
	$asyncscript.run("return $<1, 2>;", null, function(err, result){
		assert(result.__$cc$__);
			assert.strictEqual(result.__$size$__, 2);
			assert.strictEqual(result.__$contracts$__[0], Object);
			return test.done();	
	});
};

exports['Typeof(typedef = SIGNATURE)'] = function(test){
	$asyncscript.run("return $(@a, b);", null, function(err, result){
		assert.ok(result === $asyncscript.typedef);
		return test.done();	
	});
};

exports['Typeof(function)'] = function(test){
	$asyncscript.run("return $(@a, b -> a + b);", null, function(err, result){
		assert(result instanceof $asyncscript.Signature);
			return test.done();	
	});
};
