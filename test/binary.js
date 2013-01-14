require("../");
var assert = require('assert');

exports['Plus(integer, integer)'] = function(test){
	$asyncscript.run("return 2 + 3;", null, function(err, result){
		assert.strictEqual(result, 5);
		test.done();	
	});
};

exports['Plus(integer, boolean)'] = function(test){
	$asyncscript.run("return 2 + true;", null, function(err, result){
		assert.strictEqual(result, 3);
		$asyncscript.run("return 2 + false;", null, function(err, result){
			assert.strictEqual(result, 2);
			test.done();
		});
	});
};

exports['Plus(integer, void)'] = function(test){
	$asyncscript.run("return 2 + void;", null, function(err, result){
		assert.strictEqual(result, 2);
		return test.done();
	});
};

exports['Plus(string, string)'] = function(test){
	$asyncscript.run("return '12' + '34';", null, function(err, result){
		assert.strictEqual(result, '1234');
		return test.done();
	});
};

exports['Plus(string, integer)'] = function(test){
	$asyncscript.run("return '12' + 34;", null, function(err, result){
		assert.strictEqual(result, '1234');
		return test.done();
	});
};

exports['Plus(real, real)'] = function(test){
	$asyncscript.run("return 12.1 + 23.3;", null, function(err, result){
		assert.strictEqual(result, 35.4);
		return test.done();
	});
};

exports['Plus(integer, real)'] = function(test){
	$asyncscript.run("return 12.1 + 23;", null, function(err, result){
		assert.strictEqual(result, 35.1);
		return test.done();
	});
};

//minus
exports['Minus(integer, integer)'] = function(test){
	$asyncscript.run("return 2 - 3;", null, function(err, result){
		assert.strictEqual(result, -1);
		return test.done();
	});
};

exports['Minus(integer, boolean)'] = function(test){
	$asyncscript.run("return 2 - true;", null, function(err, result){
		assert.strictEqual(result, 1);
		$asyncscript.run("return 2 - false;", null, function(err, result){
			assert.strictEqual(result, 2);
			return test.done();
		});
	});
};

exports['Minus(integer, void)'] = function(test){
	$asyncscript.run("return 2 - void;", null, function(err, result){
		assert.strictEqual(result, 2);
		return test.done();
	});
};

exports['Minus(string, string)'] = function(test){
	$asyncscript.run("return '1234' - '34';", null, function(err, result){
		assert.strictEqual(result, '12');
		return test.done();
	});
};

exports['Minus(real, real)'] = function(test){
	$asyncscript.run("return 12.2 - 12.1;", null, function(err, result){
		assert.strictEqual(result, 12.2 - 12.1);
		return test.done();
	});
};

exports['Minus(integer, real)'] = function(test){
	$asyncscript.run("return 12.1 - 12;", null, function(err, result){
		assert.strictEqual(result, 12.1 - 12);
		return test.done();
	});
};

//multiplication
exports['Multiply(integer, integer)'] = function(test){
	$asyncscript.run("return 2 * 3;", null, function(err, result){
		assert.strictEqual(result, 6);
		return test.done();
	});
};

exports['Multiply(integer, boolean)'] = function(test){
	$asyncscript.run("return 2 * true;", null, function(err, result){
		assert.strictEqual(result, 2);
		$asyncscript.run("return 2 * false;", null, function(err, result){
			assert.strictEqual(result, 0);
			return test.done();
		});
	});
};

exports['Multiply(integer, void)'] = function(test){
	$asyncscript.run("return 2 * void;", null, function(err, result){
		assert.strictEqual(result, 0);
		return test.done();
	});
};

exports['Multiply(string, string)'] = function(test){
	$asyncscript.run("return '12' * 2;", null, function(err, result){
		assert.strictEqual(result, '1212');
		return test.done();
	});
};

exports['Multiply(real, real)'] = function(test){
	$asyncscript.run("return 12.2 * 12.1;", null, function(err, result){
		assert.strictEqual(result, 12.2 * 12.1);
		return test.done();
	});
};

exports['Multiply(integer, real)'] = function(test){
	$asyncscript.run("return 12.1 * 12;", null, function(err, result){
		assert.strictEqual(result, 12.1 * 12);
		return test.done();
	});
};

exports['Equality(integer, integer)'] = function(test){
	$asyncscript.run("return 12 == 12;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['Equality(integer, any)'] = function(test){
	$asyncscript.run("return 12 == any;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['Inequality(integer, any)'] = function(test){
	$asyncscript.run("return 12 != any;", null, function(err, result){
		assert(!result);
		return test.done();
	});
};

exports['Reference equality(any, any)'] = function(test){
	$asyncscript.run("return any === any;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['Reference equality(integer, integer)'] = function(test){
	$asyncscript.run("return 12 === 12;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['Shift left(integer, integer)'] = function(test){
	$asyncscript.run("return 1 << 3;", null, function(err, result){
		assert.strictEqual(result, 8);
		return test.done();
	});
};

exports['Shift right(integer, integer)'] = function(test){
	$asyncscript.run("return 8 >> 3;", null, function(err, result){
		assert.strictEqual(result, 1);
		return test.done();
	});
};

exports['Shift right(container-contract, integer)'] = function(test){
	$asyncscript.run("return <<object, integer>> >> 1;", null, function(err, result){
		assert(result.__$cc$__);
		assert.strictEqual(result.__$size$__, 1);
		assert.strictEqual(result.__$contracts$__[0], Object);
		//out of bound shift
		$asyncscript.run("return <<object, integer>> >> 3;", null, function(err, result){
			assert(result.__$cc$__);
			assert.strictEqual(result.__$size$__, 0);
			return test.done();
		});
	});
};

exports['Shift left(container-contract, integer)'] = function(test){
	$asyncscript.run("return <<object, integer>> << 1;", null, function(err, result){
		assert(result.__$cc$__);
		assert.strictEqual(result.__$size$__, 1);
		assert.strictEqual(result.__$contracts$__[0], $asyncscript.integer);
		//out of bound shift
		$asyncscript.run("return <<object, integer>> << 3;", null, function(err, result){
			assert(result.__$cc$__);
			assert.strictEqual(result.__$size$__, 0);
			return test.done();
		});
	});
};

exports['Shift right(signature, integer)'] = function(test){
	$asyncscript.run("return (@a: object, b: integer) >> 1;", null, function(err, result){
		assert(result instanceof $asyncscript.Signature);
		assert.strictEqual(result.__$size$__, 1);
		assert.strictEqual(result[0], Object);
		//out of bound shift
		$asyncscript.run("return (@a: object, b: integer) >> 3;", null, function(err, result){
			assert(result instanceof $asyncscript.Signature);
			assert.strictEqual(result.__$size$__, 0);
			return test.done();
		});
	});
};

exports['Shift left(signature, integer)'] = function(test){
	$asyncscript.run("return (@a: object, b: integer) << 1;", null, function(err, result){
		assert(result instanceof $asyncscript.Signature);
		assert.strictEqual(result.__$size$__, 1);
		assert.strictEqual(result[0], $asyncscript.integer);
		//out of bound shift
		$asyncscript.run("return (@a: object, b: integer) << 3;", null, function(err, result){
			assert(result instanceof $asyncscript.Signature);
			assert.strictEqual(result.__$size$__, 0);
			return test.done();
		});
	});
};

//contracts
exports['And(typedef, typedef)'] =  function(test){
	$asyncscript.run("return integer & string;", null, function(err, result){
		assert.strictEqual(result, null);
		$asyncscript.run("return object & string;", null, function(err, result){
			assert.strictEqual(result, String);
			return test.done();
		});
	});
};

exports['Or(typedef, typedef)'] =  function(test){
	$asyncscript.run("return integer | string;", null, function(err, result){
		assert(result instanceof $asyncscript.Union);
		return test.done();
	});
};

exports['Xor(typedef, integer)'] = function(test){
	$asyncscript.run("return integer ^ 3;", null, function(err, result){
		assert(result instanceof $asyncscript.Vector);
		assert(result.contract === $asyncscript.integer);
		assert.strictEqual(result.__$size$__, 3);
		return test.done();
	});
};

//is
exports['IS(integer, typedef = integer)'] = function(test){
	$asyncscript.run("return 3 is integer;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(integer, typedef = object)'] = function(test){
	$asyncscript.run("return 3 is object;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(integer, typedef = string)'] = function(test){
	$asyncscript.run("return 3 is string;", null, function(err, result){
		assert(result === false);
		return test.done();
	});
};

exports['IS(void, void)'] = function(test){
	$asyncscript.run("return void is void;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(boolean, typedef = integer)'] = function(test){
	$asyncscript.run("return false is integer;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(boolean, typedef = boolean)'] = function(test){
	$asyncscript.run("return false is boolean;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(boolean, typedef = real)'] = function(test){
	$asyncscript.run("return false is real;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(boolean, typedef = object)'] = function(test){
	$asyncscript.run("return false is object;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(string, typedef = string)'] = function(test){
	$asyncscript.run("return '' is string;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(string, typedef = object)'] = function(test){
	$asyncscript.run("return '' is object;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(string, typedef = typedef)'] = function(test){
	$asyncscript.run("return '' is typedef;", null, function(err, result){
		assert(result === false);
		return test.done();
	});
};

exports['IS(typedef = function, typedef = typedef)'] = function(test){
	$asyncscript.run("return function is typedef;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(void, typedef = typedef)'] = function(test){
	$asyncscript.run("return void is typedef;", null, function(err, result){
		assert(result == false);
		return test.done();
	});
};

exports['IS(function, typedef = function)'] = function(test){
	$asyncscript.run("return (@a, b -> a + b) is function;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(function, typedef = SIGNATURE1)'] = function(test){
	$asyncscript.run("return (@a, b -> a + b) is @a, b;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(function, typedef = SIGNATURE2)'] = function(test){
	$asyncscript.run("return (@a, b -> a + b) is @a, b, c;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(function, typedef = SIGNATURE2)'] = function(test){
	$asyncscript.run("return (@a, b, c -> a + b) is @a, b;", null, function(err, result){
		assert(result === false);
		return test.done();
	});
};

exports['IS(container, container-contract1)'] = function(test){
	$asyncscript.run("return <1, 2> is << >>;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(container, container-contract2)'] = function(test){
	$asyncscript.run("return <1, 2> is <<object, object>>;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(container, container-contract3)'] = function(test){
	$asyncscript.run("return <1 to integer, 2 to integer> is <<integer, integer>>;", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(container, vector)'] = function(test){
	$asyncscript.run("return <1 to integer, 2 to integer> is (integer ^ 2);", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(container, array-contract)'] = function(test){
	$asyncscript.run("return <1 to integer, 2 to integer> is integer[];", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(array, vector)'] = function(test){
	$asyncscript.run("return [1, 2] is (object ^ 2);", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(array, vector)'] = function(test){
	$asyncscript.run("return [1, 2] is (object ^ 2);", null, function(err, result){
		assert(result);
		return test.done();
	});
};

exports['IS(array, array-contract)'] = function(test){
	$asyncscript.run("return [1, 2] is object[];", null, function(err, result){
		assert(result);
		return test.done();
	});
};
