require("../");
var assert = require('assert');

exports['Identifier definition'] = function(test){
	$asyncscript.run("let a = 2; return a;", null, function(err, result){
		assert.strictEqual(result, 2);
			return test.done();	
	});
};

exports['Inline JavaScript code'] = function(test){
	$asyncscript.run("'extension inlinejs'; return #javascript 'return {a: 2, b: 3}';", null, function(err, result){
		assert.strictEqual(result.a, 2);
		assert.strictEqual(result.b, 3);
		return test.done();	
	});
};

exports['Quouted identifier'] = function(test){
	$asyncscript.run("let `#` = 2; return `#`;", null, function(err, result){
		assert.strictEqual(result, 2);
		return test.done();	
	});
};

exports['Fork'] = function(test){
	$asyncscript.run("'extension std'; return fork 2 + 3;", null, function(err, result){
		assert.strictEqual(result, 5);
		return test.done();	
	});
};

exports['JS callback'] = function(test){
	$asyncscript.run("'extension std'; 'extension inlinejs'; let jsfn = #javascript 'return function(callback){ return callback(undefined, 20); }'; return fork jsfn();", null, function(err, result){
		assert.strictEqual(result, 20);
		return test.done();	
	});
};

exports['Condition with fork'] = function(test){
	$asyncscript.run("'extension std'; let a = fork true; return a ? 10 : 20;", null, function(err, result){
		assert.strictEqual(result, 10);
		return test.done();	
	});
};

exports['False condition with fork'] = function(test){
	$asyncscript.run("'extension std'; let a = fork 0; return a ? 10 : 20;", null, function(err, result){
		assert.strictEqual(result, 20);
		return test.done();	
	});
};

exports['Success async'] = function(test){
	$asyncscript.run("'extension std'; let a = future integer; return 12 => a; return a;", null, function(err, result){
		assert.strictEqual(result, 12);
		return test.done();	
	});
};

exports['Fault async'] = function(test){
	$asyncscript.run("'extension std'; let a = future integer; fault 12 => a; return a;", null, function(err, result){
		assert.strictEqual(err, 12);
		return test.done();
	});
};

exports['Await'] = function(test){
	$asyncscript.run("'extension std'; let a = fork 12; return await(let sa = a) -> sa + 2;", null, function(err, result){
		assert.strictEqual(result, 14);
		return test.done();	
	});
};

exports['Await with handled fault'] = function(test){
	$asyncscript.run("'extension std'; let a = future integer; fork fault 'flt' => a; return await(let sa = a) -> sa + 2 : error;", null, function(err, result){
		assert.strictEqual(result, 'flt');
		return test.done();
	});
};

exports['Await with unhandled fault'] = function(test){
	$asyncscript.run("'extension std'; let a = future integer; fork fault 'flt' => a; return await(let sa = a) -> sa + 2;", null, function(err, result){
		assert.strictEqual(err, 'flt');
		return test.done();
	});
};

exports['Exception handling without exception'] = function(test){
	$asyncscript.run("let a = 2; return a + 2 !! 56;", null, function(err, result){
		assert.strictEqual(result, 4);
		return test.done();
	});
};

exports['Exception handling with exception'] = function(test){
	$asyncscript.run("'extension std'; let a = future object; fork fault 'flt' => a; return a + 2 !! error;", null, function(err, result){
		assert.strictEqual(result, 'flt');
		return test.done();
	});
};

exports['Exception handling without exception with finally'] = function(test){
	$asyncscript.run("'extension std'; let a = future object; 10 + 2 !! error : return 10 => a; return a;", null, function(err, result){
		assert.strictEqual(result, 10);
		return test.done();
	});
};

exports['Exception handling with exception with finally'] = function(test){
	$asyncscript.run("let a = future object; let b = future object; fault 10 => b; b !! error : return 40 => a; return a;", null, function(err, result){
		assert.strictEqual(result, 40);
		return test.done();
	});
};

exports['Selector 1'] = function(test){
	$asyncscript.run("return 10 ?? 1, 2, 10: 40, any: 30;", null, function(err, result){
		assert.strictEqual(result, 40);
		return test.done();
	});
};

exports['Selector 2'] = function(test){
	$asyncscript.run("return 0 ?? 1, 2, 10: 40, any: 30;", null, function(err, result){
		assert.strictEqual(result, 30);
		return test.done();
	});
};

exports['Selector with comparer'] = function(test){
	$asyncscript.run("let comparer = @src, val -> src - 1 == val; return 11 ?? == : comparer, 1, 2, 10: 40, any: 30;", null, function(err, result){
		assert.strictEqual(result, 40);
		return test.done();
	});
};

exports['Invoke lambda without fault'] = function(test){
	$asyncscript.run("let fn = @a, b -> {let c = a + b; return c}; return fn(1, 2);", null, function(err, result){
		assert.strictEqual(result, 3);
		return test.done();
	});
};

exports['Invoke lambda without fault'] = function(test){
	$asyncscript.run("let fn = @a, b -> {let c = a + b; fault 'flt'}; return fn(1, 2);", null, function(err, result){
		assert.strictEqual(err, 'flt');
		return test.done();
	});
};

exports['Invoke lambda with result redirection'] = function(test){
	$asyncscript.run("let fn = @a, b -> a + b; let res = future object; fn(1, 2) => res; return res;", null, function(err, result){
		assert.strictEqual(result, 3);
		return test.done();
	});
};

exports['Reactive write'] = function(test){
	$asyncscript.run("'extension std'; let a = future object; let r{get 2, set return value => a}; r = 30; return a;", null, function(err, result){
		assert.strictEqual(err, undefined);
		assert.strictEqual(result, 30);
		return test.done();
	});
};

exports['Reactive read'] = function(test){
	$asyncscript.run("let r{get 2}; return r + 8;", null, function(err, result){
		assert.strictEqual(result, 10);
		return test.done();
	});
};

exports['Recursive call'] = function(test){
	$asyncscript.run("let fact = @a -> a > 1 ? a * callee(a - 1) : 1; return fact(3);", null, function(err, result){
		assert.strictEqual(result, 6);
		return test.done();
	});
};

exports['Member read'] = function(test){
	$asyncscript.run("let v = <let a = 2, let b = 1>; return v.a;", null, function(err, result){
		assert.strictEqual(result, 2);
		return test.done();
	});
};

exports['Indexer read from container'] = function(test){
	$asyncscript.run("let v = <1, 2>; return v[1];", null, function(err, result){
		assert.strictEqual(result, 2);
		return test.done();
	});
};

exports['Indexer read from signature'] = function(test){
	$asyncscript.run("let v = @a: real, b: string; return v[0];", null, function(err, result){
		assert.strictEqual(result, Number);
		return test.done();
	});
};

exports['Indexer read from array'] = function(test){
	$asyncscript.run("let v = [42, 43]; return v[0];", null, function(err, result){
		assert.strictEqual(result, 42);
		return test.done();
	});
};

exports['Repeat loop'] = function(test){
	$asyncscript.run("return repeat rstate -> {break 12, 13, 14 => rstate};", null, function(err, result){
		assert(result instanceof Array);
		assert.strictEqual(result[0], 12);
		assert.strictEqual(result[1], 13);
		assert.strictEqual(result[2], 14);
		return test.done();
	});
};

exports['Repeat loop with aggregator'] = function(test){
	$asyncscript.run("return repeat rstate -> {break 10, 20, 30 => rstate}, +;", null, function(err, result){
		assert.strictEqual(result, 60);
		return test.done();
	});
};

exports['Repeat loop with aggregator'] = function(test){
	$asyncscript.run("return repeat rstate -> {break 10, 20, 30 => rstate}, +;", null, function(err, result){
		assert.strictEqual(result, 60);
		return test.done();
	});
};

exports['Repeat loop with aggregator'] = function(test){
	$asyncscript.run("return repeat rstate -> {break 10, 20, 30 => rstate}, +;", null, function(err, result){
		assert.strictEqual(result, 60);
		return test.done();
	});
};

exports['For-each loop through array'] = function(test){
	$asyncscript.run("return for i in [1, 2] -> {continue i + 1};", null, function(err, result){
		assert(result instanceof Array);
		assert.strictEqual(result[0], 2);
		assert.strictEqual(result[1], 3);
		return test.done();
	});
};

exports['For-each loop through array with aggregation'] = function(test){
	$asyncscript.run("return for i in [1, 2] -> {continue i + 1}, +;", null, function(err, result){
		assert.strictEqual(result, 5);
		return test.done();
	});
};

exports['Export from object'] = function(test){
	$asyncscript.run("return with a, b in <let a = 2, let b = 10> -> a + b;", null, function(err, result){
		assert.strictEqual(result, 12);
		return test.done();
	});
};

exports['JSON serialization'] = function(test){
	$asyncscript.run("return JSON.stringify(<let a = 1, let b = 2>);", null, function(err, result){
		assert.strictEqual(result, "{\"a\":1,\"b\":2}");
		result = JSON.parse(result);
		assert.strictEqual(result.a, 1);
		return test.done();
	});
};

exports['Filter contract binding 1'] = function(test){
	$asyncscript.run("let c = typedef.filter(@a: integer -> a % 2 == 0); return 10 is c;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['Filter contract binding 2'] = function(test){
	$asyncscript.run("let c = typedef.filter(@a: integer -> a % 2 == 0); return 11 is c;", null, function(err, result){
		assert.strictEqual(result, false);
		return test.done();
	});
};

exports['Filter contract binding 2'] = function(test){
	$asyncscript.run("let c = typedef.filter(@a: integer -> a % 2 == 0); return 11 is c;", null, function(err, result){
		assert.strictEqual(result, false);
		return test.done();
	});
};

exports['Transformation #1'] = function(test){
	$asyncscript.run("let obj = <let a = 2 with object>; return obj;", null, function(err, result){
		assert.ok(result);
		assert.ok("with" in result);
		assert.ok(("a" in result) === false);
		return test.done();
	});
};

exports['Transformation #2'] = function(test){
	$asyncscript.run("let obj = <let a = (2 + _ctx_.b) with object>; return obj with <let b = 20>;", null, function(err, result){
		assert.ok(result);
		assert.ok("a" in result);
		assert.strictEqual(result.a, 22);
		return test.done();
	});
};
