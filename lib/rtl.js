var utils = require('util');

if(typeof ASYNCSCRIPT_SYNTAX_EXTENSIONS === "undefined") global.ASYNCSCRIPT_SYNTAX_EXTENSIONS = {};

Object.defineProperty(global, "$asyncscript", 
{
	writable: false,
	configurable: false,
	value: module.exports = {
		anyvalue: Object.preventExtensions({__$contract$__: Object}),
		states: new Array,
		get state(){ return this.states[this.states.length - 1]; },
		extensions: ASYNCSCRIPT_SYNTAX_EXTENSIONS,
		enterLambdaBody: function(){
			var destination = this.destination;
			delete this.destination;
			return destination === undefined ? new this.Promise() : destination;
		},
		prepareLambdaInvocation: function(destination){ this.destination = destination;	},
		get layer(){ return this.state.layer; }
	}
});

$asyncscript.enterState = function(state){ return this.states.push(state); };
$asyncscript.exitState = function(){ return this.states.pop(); };
$asyncscript.pushLayer = function(layer){ return this.state.pushLayer(layer); };
$asyncscript.popLayer = function(){ return this.state.popLayer(); };
$asyncscript.inLayers = function(name){ return this.state.inLayers(name); };
$asyncscript.fromLayer = function(name){ return this.state.fromLayer(name); };

/**
 * Computes SDBM hash code from the string.
 * @param {String} str A string to be hashed.
 * @return {Number} A hash code of the string.
 */
function hashCode(str){
    var hash = 0;
    for (var i = 0; i < str.length; i++) {
        var ch = str.charCodeAt(i);
        hash = ch + (hash << 6) + (hash << 16) - hash;
    }
    return hash;
}

function inverseRelationship(rel){
	switch(rel){
		case "subset": return "superset";
		case "superset": return "subset";
		default: return rel;
	}
}

/**
 * Initializes a new native NodeJS/Browser event queue.
 * @class Represents a native NodeJS/Browser event queue.
 */
function TaskQueue(){
	this.count = 0;
	this.suspended = false;
	this.buffer = [];
}

TaskQueue.prototype = {
	get bufferedTasks(){ return this.buffer.map(function(t){ return t.taskName; }); }
};

/**
 * Prevents queue from execution of work items.
 * @return {Boolean} true, if execution is suspended successfully.
 */
TaskQueue.prototype.suspend = function(){
	return this.suspended ? false : this.suspended = true;
};

/**
 * Re-execute all work items saved into the internal buffer after suspension.
 * @return {Boolean} true if execution is resumed successfully; otherwise, false.
 */
TaskQueue.prototype.resume = function(i){
	if(this.suspended){
		switch(arguments.length){
			case 0:
				//enqueues all items to the native NodeJS queue
				while(this.buffer.length) process.nextTick(this.buffer.pop());
			break;
			default:
				//resuming specified count of tasks
				while(i-- && this.buffer.length) process.nextTick(this.buffer.pop());
			break;
		}
		this.suspended = false;
		return true;
	}
	else return false;
};

/**
 * Enqueues a function an associates the execution state with the promise object.
 * @param {Promise} A promise object that represents state of the asynchronous execution.
 * @param {Function} A function that implements asynchronous task.
 * @param {Object} The first argument of the function.
 */
TaskQueue.prototype.nextTick = function(promise, action, target){
	var currentState = $asyncscript.state;	//saves the current rutime state
	process.nextTick(function(){
		//if queue is suspended then save task into the buffer
		if(this.suspended) {
			arguments.callee.taskName = promise.name;
			this.buffer.unshift(arguments.callee);
		}
		var result;
		try{
			$asyncscript.enterState(currentState);	//pushes a new runtime state
			this.count += 1; //count of tasks
			result = action(target);
		}
		catch(e){
			promise.fault(e);
		}
		finally{
			this.count -= 1;
			$asyncscript.exitState();	//removes the current runtime state
		}
		//sets result to the promise
		return result instanceof Promise ?
			result.route(promise) :
			promise.success(result);
	}.bind(this));
};

/**
 * Executes the specified function asynchronously and wraps asynchronous state into the promise.
 * @param {Function} action A function that implements asynchronous task.
 * @param {Object} target The first argument of the function.
 * @return {Function} fault A function that handles an error.
 */
TaskQueue.prototype.enqueue = function(action, target, fault){
	var promise = new Promise();
	if(target instanceof Promise)
		if(target.isError) 
			if(fault instanceof Function) this.nextTick(promise, fault, target.result); else throw target.result;
	 	else if(target.isCompleted) this.nextTick(promise, action, target.result);
		else {
			promise.name = "Depends on " + target.name;
			target
			.on('success', function(result){
	    			return this.nextTick(promise, action, result);
	   		}.bind(this))
	   		.on('error', function(error){
	   			return fault instanceof Function ? this.nextTick(promise, fault, error) : promise.fault(error);
	   		}.bind(this));
	}
	else this.nextTick(promise, action, target);
	return promise;
};
$asyncscript.queue = new TaskQueue();

/**
 * Initializes a new promise.
 * @class Represents promise.
 * @param {Function} Represents contract of the promise.
 */
function Promise(contract){
	Object.defineProperty(this, "__$contract$__", {value: contract === undefined ? Object : contract});
	this.events = {'success': [], 'error': []};
	this.name = "Promise";
	this.isCompleted = false;
	this.isError = false;
}
$asyncscript.Promise = Promise;

Promise.prototype.setContract = function(contract){
	switch($asyncscript.relationship(contract, this.__$contract$__)){
		case "equal":
		case "superset": return this;
		case "subset": this.__$contract$__ = contract; return this;
		default: throw runtimeErrors.failedContractBinding;
	}
};

/**
 * Sets the promise to the signaled state and associates the real value with it.
 * @param {Object} result A real value.
 */
Promise.prototype.success = function(result){ return this.complete(undefined, result); };

/**
 * Sets the promise to the signaled state and associates the error with it.
 * @param {Object} An error to be associated with the promise.
 */
Promise.prototype.fault = function(err){ return this.complete(err); };

Promise.prototype.complete = function(err, result){
	function proceed(){
		this.events[this.isError ? "error" : "success"].forEach(function(handler){ handler(this.result); }, this);
	};
	if(this.isCompleted) return false;
	else if(this.isError = (err !== undefined)) this.result = err;
	else this.result = $asyncscript.binding(result, this.__$contract$__);
	//process all dependencies
	proceed.call(this);
	delete this.events;
	return this.isCompleted = true; 
};

/**
 * Established one-way connection between two promises.
 * The state of the current promise will be tracked by the specified promise. An error or successful completion of this promise will be redirected to the specified promise.
 */
Promise.prototype.route = function(promise){
	if(promise.isCompleted) return false;
	else if(this.isError) promise.fault(this.result);
	else if(this.isCompleted) promise.success(this.result);
	else this.on('error', promise.fault.bind(promise)).on('success', promise.success.bind(promise));
	return true;
};

Promise.prototype.on = function(name, handler){
	if(name === 'error' && this.isError) handler(this.result);
	else if(name === 'success' && this.isCompleted) handler(this.result);
	else if(name = this.events[name]) name.push(handler);
	return this;
};

Promise.prototype.toString = function(){
	return this.isCompleted ? toScriptString(this.result) : "Incompleted " + this.name;
};

/**
 * Provides logic for 'return' operator.
 * @param {Object} value A value to return.
 * @param {Object} destination The result destination.
 * @return {Boolean} true, if value is successfully passed to the destination; otherwise, false.
 */
$asyncscript.ret = function(value, destination){
	if(destination instanceof Promise)
		if(destination.isError) throw destination.result;
		else if(destination.isCompleted) return false;
		else if(value instanceof Promise)
			if(value.isError) destination.fault(value.result);
			else if(value.isCompleted) destination.success(value.result);
			else value.route(destination);
		else destination.success(value);
	return true;
};

/**
 * Provides logic for 'fault' operator.
 * @param {Object} err An error to raise.
 * @param {Object} destination The error destination.
 * @return {Boolean} true, if error is successfully passed to the destination; otherwise, false.
 */
$asyncscript.fault = function(err, destination){
	if(destination instanceof Promise)
		if(destination.isError) throw destination.result;
		else if(destination.isCompleted) return false;
		else if(err instanceof Promise)
			if(err.isError || err.isCompleted) destination.fault(err.result);
			else $asyncscript.fork(destination = destination.fault.bind(destination), err, destination);
		else destination.fault(err);
	return true;
};

var runtimeErrors = $asyncscript.errors = {
	create: function(code, message){ 
		var result = new Error(utils.format("AsyncScript Runtime Error (%s): %s", code, message));
		result.code = code;
		return result;
	},
	get voidref(){
		return this.create('ASRUNTIME_VOIDREF', 'Attempts to operate with void object');
	},
	get invalidArgCount(){ 
		return this.create('ASRUNTIME_INV_ARG_COUNT', 'Invalid count of lambda arguments'); 
	},
	get failedContractBinding(){ 
		return this.create('ASRUNTIME_CONTRACT_BINDING', 'The value is not compatible with the specified contract'); 
	},
	unsupportedOp: function(opname){ 
		return this.create('ASRUNTIME_UNSUPPORTED', 'Operation ' + opname + ' is not supported'); 
	},
	get contractExpected(){ 
		return this.create('ASRUNTIME_CONTRACT_EXPECTED', 'Contract expected'); 
	},
	get missingMember(){ 
		return this.create('ASRUNTIME_MISSING_MEMBER', 'Object member is missing'); 
	},
	cannotReadMember: function(member){
		return this.create('ASRUNTIME_MISSING_MEMBER', utils.format("Cannot read '%s' member", member));
	},
	cannotWriteMember: function(member){
		return this.create('ASRUNTIME_MISSING_MEMBER', utils.format("Cannot write '%s' member", member));
	},
	get writeonlyProperty(){
		return this.create('ASRUNTIME_WRITEONLY_PROPERTY', 'Reactive value is write-only');	
	}
};

function invisibleField(obj, name, value, writable){
	if(arguments.length > 4)
		for(var i = 0; i <= arguments.length - 4; i++) invisibleField(arguments[i], arguments[arguments.length - 3], arguments[arguments.length - 2], arguments[arguments.length - 1]);	
	else if(name instanceof Array)
		return name.forEach(function(name){ return invisibleField(obj, name, value, writable); });
	else return Object.defineProperty(obj, name, {enumerable: false, 'writable': writable ? true : false, 'value': value});
}

function invisibleProperty(obj, name, getter, setter){
	if(name instanceof Array)
		return name.forEach(function(name){ return invisibleProperty(this, name, getter, setter); }, obj);
	var options = {enumerable: false};
	if(getter instanceof Function) options.get = getter;
	if(setter instanceof Function) options.set = setter;
	Object.defineProperty(obj, name, options);
}

/**
 * Initializes a new runtime state of the AsyncScript program.
 * @class Represents runtime state of the AsyncScript program.
 * @param {Array} layers Layer stack.
 * @param {Boolean} checked true for strict runtime check; otherwose, false.
 */
function RuntimeState(layers, checked){
	//copy constructor
	if(layers instanceof RuntimeState){
		this.checked = layers.checked;
		this.layers = layers.layers;
	}
	else {
		this.checked = checked;
		this.layers = layers || new Array();
	}
}

RuntimeState.prototype.setChecked = function(checked){
	var result = new RuntimeState(this);
	result.checked = checked;
	return result;
};

$asyncscript.states.push(new RuntimeState([], true));

/**
 * Creates a new task in the queue.
 * @param {Function} action A function to be executed asynchronously.
 * @param {Object} target An argument that will be passed to the action.
 * @param {Function} fault A callback that is used to handle an asynchronous error.
 * @return {Promise} A promise object that represents future result of the action.
 */
$asyncscript.fork = function(action, target, fault){
	return this.queue.enqueue(action, target, fault);
};

/**
 * Synchronizes with all values from an array.
 * @param {Array} values An array of promises.
 * @param {Function} action A function that receives an array of synchronized values.
 * @param {Function} fault A function that handles an error.
 * @return {Promise} A promise objec that represents future value of the action function.
 */
$asyncscript.waitAll = function(values, action, fault){
	function waitAll(values, action, fault, output){
		return values.length === 0 ? action(output) :		
			this.enqueue(function(value){
			output.push(value);
			return waitAll.call(this, values, action, fault, output);
		}.bind(this), values.shift(), fault);
	}
	
	return waitAll.call(this.queue, values, action, fault, new Array());
};

function toScriptString(obj){
	switch(obj){
		case Number: return "real";
		case String: return "string";
		case undefined:
		case null: return "void";
		case Function: return "function";
		case RegExp: return "regexp";
		case Boolean: return "boolean";
		case Object: return "object";
		default: return obj.toString();
	}
}

$asyncscript.toString = function(){
	return arguments.length > 0 ? toScriptString(arguments[0]) : Object.prototype.toString.call(this); 
};

/**
 * Compiles a new lambda-function.
 * @param {Function} implementation Synchronous implementation of the lambda-function.
 * @param {Boolean} oneWay one-way implementation of the function.
 * @return {Function} A new lambda function.
 */
var newLambda = $asyncscript.newLambda = function(implementation, oneWay){
	//optimize lambda function for different signature sizes
	switch(arguments.length){
		//zero arg
		case 2: return (oneWay ?
		function(){
			$asyncscript.enterLambdaBody();
			//call implementation
			return implementation.call(this), null;
		}: function(){
			var destination = $asyncscript.enterLambdaBody(),
			//call implementation
			result = implementation.call(this);
			$asyncscript[result instanceof Error ? "fault" : "ret"](result, destination);
			//return result
			if(destination instanceof Promise)
				if(destination.isError) throw destination.result;
				else if(destination.isCompleted) return destination.result;
			return destination;	
		}).toLambda(oneWay);
		//with single argument
		case 3:
		return (oneWay ?
		function(arg0){
			var contract1 = arguments.callee.__$contract$__[0];
			$asyncscript.enterLambdaBody();
			//check argument count
			if(arguments.length < 1) throw $asyncscript.errors.invalidArgCount;
			//synchronize arguments
			if(arg0 instanceof Promise)
				if(arg0.isError) throw arg0.result;
				else if(arg0.isCompleted) arg0 = arg0.result;
				else return $asyncscript.fork(function(arg0){
					return this.method.call(this.target, arg0);
				}.bind({method: arguments.callee, target: this}), arg0);
			arg0 = $asyncscript.binding(arg0, contract1);
			return implementation.call(this, arg0), null;
		}:
		function(arg0){
			var contract1 = arguments.callee.__$contract$__[0], destination = $asyncscript.enterLambdaBody();
			//check argument count
			if(arguments.length < 1){
				$asyncscript.fault($asyncscript.errors.invalidArgCount, destination);
				throw $asyncscript.errors.invalidArgCount;
			}
			//synchronize arguments
			if(arg0 instanceof Promise)
				if(arg0.isError){ $asyncscript.fault(arg0.result, destination); throw arg0.result; }
				else if(arg0.isCompleted) arg0 = arg0.result;
				else return $asyncscript.fork(function(arg0){
					$asyncscript.prepareLambdaInvocation(destination);
					return this.method.call(this.target, arg0);
				}.bind({method: arguments.callee, target: this}), arg0);
			arg0 = $asyncscript.binding(arg0, contract1);
			var result = implementation.call(this, arg0);
			$asyncscript[result instanceof Error ? "fault" : "ret"](result, destination);
			//return result
			if(destination instanceof Promise)
				if(destination.isError) throw destination.result;
				else if(destination.isCompleted) return destination.result;
			return destination;
		 }).toLambda(oneWay, arguments[2]);
		//with two arguments
		case 4:
		return (oneWay ? 
		function(arg0, arg1){
			$asyncscript.enterLambdaBody();
			var contract1 = arguments.callee.__$contract$__[0],
				contract2 = arguments.callee.__$contract$__[1];
			//check argument count
			if(arguments.length < 2) throw $asyncscript.errors.invalidArgCount;
			//synchronize arguments
			if(arg0 instanceof Promise)
				if(arg0.isError) throw arg0.result;
				else if(arg0.isCompleted) arg0 = arg0.result;
				else return $asyncscript.fork(function(arg0){
					return this.method.call(this.target, arg0, arg1);
				}.bind({method: arguments.callee, target: this}), arg0);
			arg0 = $asyncscript.binding(arg0, contract1);
			if(arg1 instanceof Promise)
				if(arg1.isError) throw arg1.result;
				else if(arg1.isCompleted) arg1 = arg1.result;
				else return $asyncscript.fork(function(arg1){
					return this.method.call(this.target, arg0, arg1);
				}.bind({method: arguments.callee, target: this}), arg1);
			arg1 = $asyncscript.binding(arg1, contract2);			
			return implementation.call(this, arg0, arg1), null;
		}:
		function(arg0, arg1){
			var destination = $asyncscript.enterLambdaBody(),
				contract1 = arguments.callee.__$contract$__[0],
				contract2 = arguments.callee.__$contract$__[1];
			//check argument count
			if(arguments.length < 2){
				$asyncscript.fault($asyncscript.errors.invalidArgCount, destination);
				throw $asyncscript.errors.invalidArgCount;
			}
			//synchronize arguments
			if(arg0 instanceof Promise)
				if(arg0.isError){ $asyncscript.fault(arg0.result, destination); throw arg0.result; }
				else if(arg0.isCompleted) arg0 = arg0.result;
				else return $asyncscript.fork(function(arg0){
					$asyncscript.prepareLambdaInvocation(destination);
					return this.method.call(this.target, arg0, arg1);
				}.bind({method: arguments.callee, target: this}), arg0);
			arg0 = $asyncscript.binding(arg0, contract1);
			if(arg1 instanceof Promise)
				if(arg1.isError){ $asyncscript.fault(arg1.result, destination); throw arg1.result; }
				else if(arg1.isCompleted) arg1 = arg1.result;
				else return $asyncscript.fork(function(arg1){
					$asyncscript.prepareLambdaInvocation(destination);
					return this.method.call(this.target, arg0, arg1);
				}.bind({method: arguments.callee, target: this}), arg1);
			arg1 = $asyncscript.binding(arg1, contract2);			
			var result = implementation.call(this, arg0, arg1);
			$asyncscript[result instanceof Error ? "fault" : "ret"](result, destination);
			//return result
			if(destination instanceof Promise)
				if(destination.isError) throw destination.result;
				else if(destination.isCompleted) return destination.result;
			return destination;
		}).toLambda(oneWay, arguments[2], arguments[3]);
		//generic lambda
		default:
		signature = new Array(arguments.length - 2);
		for(var i = 2; i < arguments.length; i++) signature[i - 2] = arguments[i];
		signature.unshift(oneWay);
		return Function.prototype.toLambda.apply(oneWay ? 
		function(){
			$asyncscript.enterLambdaBody();
			//checks argument count
			if(arguments.length < signature.length) throw $asyncscript.errors.invalidArgCount;
			//synchronize arguments
			for(var i = 0; i < arguments.length; i++){
				var a = arguments[i];
				if(a instanceof Promise)
					if(a.isError) throw a.result;
					else if(a.isCompleted) arguments[i] = a.result;
					else return ($asyncscript.fork(function(a){
						this.args[this.position] = a;
						return this.args.callee.apply(this.target, this.args);
					}.bind({args: arguments, position: i, target: this}), a), destination);
				arguments[i] = a = $asyncscript.binding(a, signature[i]);
			}
			//invoke implementation
			return implementation.apply(this, arguments), null;
		} :
		function(){
			var destination = $asyncscript.enterLambdaBody();
			//checks argument count
			if(arguments.length < signature.length){
				$asyncscript.fault($asyncscript.errors.invalidArgCount, destination);
				throw $asyncscript.errors.invalidArgCount;
			}
			//synchronize arguments
			for(var i = 0; i < arguments.length; i++){
				var a = arguments[i];
				if(a instanceof Promise)
					if(a.isError) { $asyncscript.fault(a.result, destination); throw a.result; }
					else if(a.isCompleted) arguments[i] = a.result;
					else return ($asyncscript.fork(function(a){
						$asyncscript.prepareLambdaInvocation(destination);
						this.args[this.position] = a;
						return this.args.callee.apply(this.target, this.args);
					}.bind({args: arguments, position: i, target: this}), a), destination);
				arguments[i] = a = $asyncscript.binding(a, signature[i]);
			}
			//invoke implementation
			var result = implementation.apply(this, arguments);
			$asyncscript[result instanceof Error ? "fault" : "ret"](result, destination);
			//return result
			if(destination instanceof Promise)
				if(destination.isError) throw destination.result;
				else if(destination.isCompleted) return destination.result;
			return destination;
		}, signature);
	}
};

//NON STANDARD CONTRACTS

//represents built-in typedef contract
$asyncscript.typedef = {
	get __$contract$__(){ return this; },
	toString: function(){ return "typedef"; }
};

//represents built-in integer contract
$asyncscript.integer = {
	get __$contract$__(){ return $asyncscript.typedef; },
	toString: function(){ return "integer"; }
};

/**
 * Initializes a new predicate-based contract.
 * @class Predicate-based contract.
 * @param {Object} contract Base contract from which the current contract derives.
 * @param {Function} predicate A predicate used to validate input value.
 * 
 */
function FilterContract(contract, predicate){
	this.contract = contract;
	this.predicate = predicate;
}
$asyncscript.FilterContract = FilterContract;

FilterContract.prototype = {
	__$contract$__: $asyncscript.typedef,
	__$asrtl_contractfor: function(value){
		return $asyncscript.invoke(undefined, this.predicate, [value]);
	},
	__$asrtl_implicit: function(value, successful){
		if(arguments.length === 1) successful = this.__$asrtl_contractfor(value);
		if(successful instanceof Property) return this.__$asrtl_implicit(value, successful.value);
		else if(successful instanceof Promise)
			if(successful.isError) throw successful.result;
			else if(successful.isCompleted) return this.__$asrtl_implicit(value, successful.result);
			else return $asyncscript.fork(function(successful){
				return this.__$asrtl_implicit(value, successful);
			}.bind(this), successful);
		else if(successful) return $asyncscript.binding(value, this.contract);
		else throw new Error("The value doesn't match to the predicate");
	},
	__$asrtl_relationship: function(contract){
		switch($asyncscript.relationship(this.contract, contract)){
			case "equal":			
			case "subset": return "subset";
			default: return "different";
		}
	}
};

/**
 * Constructs a new runtime representation of the specified expression tree.
 * @class Represents expression tree.
 * @param {Object} tree Serialized representation of the tree.
 */
function Expression(tree, deserialized){
	var ast = require('./ast.js');
	this.tree = deserialized ? tree : (tree = ast.restore(tree, $asyncscript.loadScript));
	if(tree.quoted) tree.quoted = false;
}

Expression.convert = function(value, position){
	var ast = require('./ast.js');
	if(value === null || value === undefined) return new ast.CodeBuiltInContractExpression("void", position.column, position.line);
	else if(typeof value === "number" || value instanceof Number)
		return value % 1 === 0 ? new ast.CodeIntegerExpression(value.toString(), position.column, position.line) : new ast.CodeRealExpression(value.toString(), position.column, position.line);
	else if(typeof value === "string" || value instanceof String)
		return new ast.CodeStringExpression(value, position.column, position.line);
	else if(typeof value === "boolean" || value instanceof Boolean)
		return new ast.CodeBooleanExpression(value.toString(), position.column, position.line);
	else if(value instanceof RegExp)
		return new ast.CodeRegularExpression(value.source, position.column, position.line);
	else if(value instanceof this) return this.tree;
	else throw new Error("Quouted expression expected");
};

Expression.toString = function(){ return "expression"; };

Expression.prototype.toString = function(){ return this.tree.toString(); };
Expression.prototype.__$contract$__ = $asyncscript.Expression = Expression;

/**
 * Initializes a new list of overloaded functions.
 * @class Represents set of overloaded functions.
 * @param {Array} An array of functions.
 */
function OverloadList(){
	var idx = 0;
	function eachfn(f){
		if(f.__$contract$__ instanceof Signature){
			idx += 1;
			this[f.__$contract$__.__$size$__] = f;				
		}
	}
	for(var i = 0; i < arguments.length; i++){
		var f = arguments[i];
		if(f instanceof OverloadList)
			Object.keys(f).forEach(eachfn, this);
		else if(f instanceof Array) f.forEach(eachfn, this);
		else eachfn.apply(this, f);
	}
	invisibleField(this, "__$size$__", idx, true);
}

/**
 * Creates a new lambda-function which result will be combined with the specified value.
 * @param {Function} previous A lambda-function to combine.
 * @param {String} operation The name of the operation used to combine lambda invocation result with the specified value.
 * @param {Object} right A value to be combined with lambda invocation result.
 * @return {Function} A new function that represents combination 
 */
function delayRightOperand(previous, operation, right){
	return previous.__$contract$__ instanceof Signature ? 
		function(){	//calls the current function and applies sum operator to the result
			//PREAMBLE			
			var destination = $asyncscript.enterLambdaBody();			
			//END PREAMBLE
			return ($asyncscript.ret($asyncscript[operation](previous.apply(null, arguments), right), destination), destination);
		}.toLambda(previous.__$contract$__) :
		function(){	//regular js function
			return $asyncscript[operation](previous.apply(null, arguments), right);
		};
}

/**
 * Creates a new lambda-function which result will be combined with the specified value.
 * @param {Function} previous A lambda-function to combine.
 * @param {String} operation The name of the operation used to combine lambda invocation result with the specified value.
 * @param {Object} left A value to be combined with lambda invocation result.
 * @return {Function} A new function that represents combination 
 */
function delayLeftOperand(previous, operation, left){
	return previous.__$contract$__ instanceof Signature ? 
		function(){	//calls the current function and applies sum operator to the result
			//PREAMBLE			
			var destination = $asyncscript.enterLambdaBody();		
			//END PREAMBLE
			return ($asyncscript.ret($asyncscript[operation](left, previous.apply(null, arguments)), destination), destination);
		}.toLambda(previous.__$contract$__) :
		function(){	//regular js function
			return $asyncscript[operation](left, previous.apply(null, arguments));
		};
}

function delayUnaryOperand(previous, operation, postfix){
	return previous.__$contract$__ instanceof Signature ?
		function(){
			//PREAMBLE			
			var destination = $asyncscript.enterLambdaBody();
			return ($asyncscropt.ret($asyncscript[operation](previous.apply(null, arguments), postfix), destination), destination);
			//END PREAMBLE
		}.toLambda(previous.__$contract$__) :
		function(){
			return $asyncscript[operation](previous.apply(null, arguments));
		};
}

OverloadList.prototype.delayRightOperand = function(operation, right){
	var result = new OverloadList();
	result.__$size$__ = this.__$size$__;
	Object.keys(this).forEach(function(i){
		result[i] = delayRightOperand(this[i], operation, right);
	}, this);
	return result;
};

OverloadList.prototype.delayLeftOperand = function(operation, left){
	var result = new OverloadList();
	result.__$size$__ = this.__$size$__;
	Object.keys(this).forEach(function(i){
		result[i] = delayLeftOperand(this[i], operation, left);
	}, this);
	return result;
};

OverloadList.prototype.delayUnaryOperand = function(operation, postfix){
	var result = new OverloadList();
	result.__$size$__ = this.__$size$__;
	Object.keys(this).forEach(function(i){
		result[i] = delayUnaryOperand(this[i], operation, postfix);
	}, this);
	return result;
};

OverloadList.prototype.toString = function(){
	var result = '';
	Object.keys(this).forEach(function(len, idx, array){
		result += '(' + toScriptString(this[len]) + ')';
		if(idx < array.length - 1) result += " + ";	
	}, this);
	return result;
};

$asyncscript.OverloadList = OverloadList;

OverloadList.prototype.__$contract$__ = Function;

/**
 * Initializes a new array contract.
 * @class Represents a new array contract.
 * @param {Object} An array contract.
 */
function ArrayContract(element){
	if(element === null) throw runtimeErrors.voidref;
	else if(element === undefined) element = Object;
	this.contract = element;
}

ArrayContract.prototype.toString = function(){
	return toScriptString(this.contract) + "[]"; 
};

$asyncscript.ArrayContract = ArrayContract;

ArrayContract.prototype.__$contract$__ = $asyncscript.typedef;

/**
 * Initializes a new complementation of the specified contract.
 * @class Represents complementation of the specified contract.
 * @param {Object} contract A contract that should be complemented.
 */
function Complementation(contract){
	this.contract = contract;
}

Complementation.prototype.toString = function(){
	return '!(' + toScriptString(this.contract) + ')';
};

Complementation.prototype.__$contract$__ = $asyncscript.typedef;

/**
 * @class Represents union of two or more contracts.
 */
function Union(contract1, contract2){
	this.first = contract1;
	this.second = contract2;
}
$asyncscript.Union = Union;

Union.prototype.toString = function(){
	return toScriptString(this.first) + '|' + toScriptString(this.second);
};

Union.prototype.__$contract$__ = $asyncscript.typedef;

/**
 * @class Represents vector contract.
 */
function Vector(contract, length){
	if(contract === null) throw runtimeErrors.voidref;
	else if(contract === undefined) contract = Object;
	Object.defineProperty(this, "__$size$__", {value: length});
	this.contract = contract;
}

Vector.prototype.toString = function(){
	return toScriptString(this.contract) + " ^ " + this.__$size$__;
};

Vector.prototype.__$contract$__ = $asyncscript.typedef;

$asyncscript.Vector = Vector;

/**
 * Initializes a new AsyncScript lambda signature.
 * @class Represents signature of the lambda.
 */
function Signature(oneWay){
	for(var i = 1; i < arguments.length; i++){
		var contract = arguments[i];
		this[i - 1] = contract === undefined ? Object : contract;	
	}
	invisibleField(this, "__$size$__", arguments.length - 1, true);
	this.oneWay = oneWay;
}

Signature.empty = new Signature(false);
Signature.emptyOneWay = new Signature(true);
Signature.prototype.__$contract$__ = $asyncscript.typedef;

Signature.prototype.clone = function(filter){
	var result = new Signature(this.oneWay), length = 0;
	for(var i = 0; i < this.__$size$__; i++){
		var c = this[i];
		if((filter instanceof Function && filter(i, c)) || filter === undefined) result[length++] = c;	
	}
	switch(length){
		case 0: return Signature.empty;
		default:
			result.__$size$__ = length;
			return result;	
	}
};

Signature.prototype.composition = function(right){
	if(right.__$size$__ === 0) return this;
	else if(this.__$size$__ === 0) return right;	
	var result = new Signature(this.oneWay | right.oneWay);
	result.__$size$__ = this.__$size$__ + right.__$size$__;
	for(var i = 0; i < this.__$size$__; i++)
		result[i] = this[i];
	for(var j = 1; j < right.__$size$__; j++)
		result[i + j] = right[j];
	return result;
};

/**
 * Inserts a new parameter to the lambda signature.
 * @param {Object} contract The contract of the parameter.
 * @param {Number} position Position of the new parameter in the signature.
 * @return {Signature} A new instance of the signature.
 */
Signature.prototype.__$asrtl_insert = function(contract, position){
	if(position > this.__$size$__) return this;
	var result = new Signature(this.oneWay);
	result.__$size$__ = this.__$size$__ + 1;	
	if(position === undefined || position === this.__$size$__){	//add a new parameter to the end
		for(var i = 0; i < this.__$size$__; i++)
			result[i] = this[i];
		result[this.__$size$__] = contract;
	}
	else for(var i = 0; i < this.__$size$__; i++)
		if(i === position) result[i] = contract;
		else if(i < position) result[i] = this[i];
		else result[i + 1] = this[i];
	return result;
};

Signature.prototype.toString = function(){
	var result = this.oneWay ? "@@" : "@";
	for(var i = 0; i < this.__$size$__; i++)
		result += "_" + i + ": " + toScriptString(this[i]) + (i < this.__$size$__ - 1 ? ', ' : '');
	return result;
};

$asyncscript.Signature = Signature;

$asyncscript.wrapLambda = function wrapLambda(f, signature, binding){
	var result;
	if(binding === undefined)
		result = function(){ return arguments.callee.wrapped.apply(this, arguments); };
	else {
		result = function(){ return arguments.callee.wrapped.apply(arguments.callee.binding, arguments); };
		result.binding = binding;
	}
	result.wrapped = f;	//this property allows to override __$contract$__ property
	result.toString = function(){ return this.wrapped.toString(); };
	Object.defineProperty(result, "__$contract$__", {value: signature === undefined ? f.__$contract$__ : signature});
	result.isStandaloneLambda = true;
	return result;
}

/**
 * Converts the function to AsyncScript lambda.
 * 
 */
invisibleField(Function.prototype, "toLambda", function(){
	//current lambda function is wrapped, change the signature
	if(this.wrapped) return wrapLambda(this, arguments[0], this.binding);
	//wraps lambda function and overrides signature
	else if(this.__$contract$__ instanceof Signature) return wrapLambda(this, arguments[0]);
	else {//parse signature
		var signature;
		switch(arguments.length){
			case 0: throw new Error("Invalid arg count");
			case 1: signature = arguments[1] instanceof Signature ? arguments[1] : Signature[arguments[1] ? "emptyOneWay" : "empty"]; break;
			default:
				(signature = new Signature(arguments[0])).__$size$__ = arguments.length - 1;
				for(var i = 1; i < arguments.length; i++) signature[i - 1] = (arguments[i] !== undefined) ? arguments[i] : Object;
			break;	
		}
		this.toString = function(){ return this.__$contract$__ + ' -> [[Compiled Lambda]]'}
		//this is a special property that allows to override contract from the caller lambda
		Object.defineProperty(this, "__$contract$__", {get: function(){
			return this.caller && this.caller.wrapped ? this.caller.__$contract$__ : signature;
		}});
		this.isStandaloneLambda = true;
		return this;
	}
});

function thisProvider(){ return this; }

var containerContract = $asyncscript.containerContract = {
	"base":{
		__$contract$__: $asyncscript.typedef,
		__$cc$__: true,	//identifies an object as a container contract
		get __$empty$__(){ return this["__$size$__"] === 0; },
	},
	putNameIndexMap: function(idx, name, map){
		if(name){ map[idx] = name; map[name] = idx; }
		return name;
	},
	contractAt: function(cc, idx){ return cc["__$contracts$__"][idx] || cc["__$contracts$__"][cc[idx]]; },
	setMetadata: function(cc, meta){
		if(meta !== null || meta!== undefined) cc["__$meta$__"] = meta;	
	},
	sizeOf: function(cc){ return cc["__$size$__"]; },
	getMetadata: function(cc){ return cc["__$meta$__"]; },
};

containerContract.base.__$asrtl_insert = function(contract, idx){
	if(idx > this.__$size$__) return this;
	if(contract === undefined) contract = Object;
	var result = {
		__$size$__: this.__$size$__ + 1,
		__$contracts$__: {},
		__proto__: this.__proto__
	};
	if(idx > this.__$size$__) return this;
	else if(idx === undefined || idx === this.__$size$__){
		for(var i = 0; i < this.__$size$__; i++) {
			containerContract.putNameIndexMap(i, this[i], result);
			result.__$contracts$__[i] = this.__$contracts$__[i];
		}
		result.__$contracts$__[idx] = contract;
	}
	else for(var i = 0; i < this.__$size$__; i++)
			if(i === idx) cc[i] = contract;
			else if(i < idx){
				containerContract.putNameIndexMap(i, this[i], result);
				result.__$contracts$__[i] = this.__$contracts$__[i];
			}
			else{
				containerContract.putNameIndexMap(i + 1, this[i], result);
				result.__$contracts$__[i + 1] = this.__$contracts$__[i];
			}
	return result;
};

/*container structure:
	{
		__$contracts$__: [Object, Integer],
		__$size$__: 2,
		"a": 0,
		"b": 1,
		"0": "a",
		"1": "b"
	}
*/

/**
 * Creates a new container contract.
 * @return {Object} A new container contract.
 */
containerContract.create = function(){
	var result = {
		__$size$__: arguments.length,
		__$contracts$__: {},
		__proto__: this.base
	};
	for(var i = 0; i < arguments.length; i++){
		var desc = arguments[i];
		result.__$contracts$__[i] = desc.contract === undefined ? Object : desc.contract;
		this.putNameIndexMap(i, desc.name, result);
	}
	return result;
};

/**
 * Creates a clone of the specified container contract.
 * @param {Object} cc A container contract to clone.
 * @param {Function} filter A function that is used as field filer (with three args: 0 - position in the source contract, 1 - the name of the field, 2 - the contract of the field). Optional.
 * @return {Object} A new container contract.
 */
containerContract.clone = function(cc, filter){
	var result = {
		"__$contracts$__": {},	
		__proto__: cc.__proto__	
	},
	length = 0;	//size of the new contract
	for(var i = 0; i < cc.__$size$__; i++){
		var name = cc[i], c = cc.__$contracts$__[i];
		if(filter instanceof Function && filter(i, name, c) || filter === undefined){
			this.putNameIndexMap(length, name, result);
			result.__$contracts$__[length++] = c;
		}	
	}
	switch(length){
		case 0: return this.empty;
		default:
			result.__$size$__ = length;
			return result;		
	}	
};

containerContract.empty = containerContract.create();

containerContract.toString = function(){
	var result = '<<';
	this.contracts.forEach(function(c, i, contracts){
		var name = this[i];
		if(name) result += "let " + name + ": ";
		result += toScriptString(contracts[i]) + (i < contracts.length - 1 ? ", " : "");
	}, this);
	result += ">>";
	return result;
};

/**
 * Represents reactive value (property in AsyncScript).
 * @param {Function} getter Represents value getter.
 * @param {Function} setter Represents value setter. Optional.
 * @param {Boolean} cached Use cache for returning real value.
 * @param {Object} contract The contract of the reactive value. Optional.
 * @param {Object} _this This-reference for the getter and setter. Optional.
 */
function Property(getter, setter, cached, contract, _this){
	if(arguments.length === 5) Object.defineProperty(this, "this", {writable: false, value: _this});
	Object.defineProperty(this, "getter", {writable: false, value: getter});
	Object.defineProperty(this, "setter", {writable: false, value: setter});
	Object.defineProperty(this, "isCached", {writable: false, value: cached ? true : false});
	function throwWriteonly(){ 
		if($asyncscript.state.checked) throw runtimeErrors.writeonlyProperty;
		else return null; 
	}
	function empty(){ }
	if(contract === undefined || contract === Object){
		Object.defineProperty(this, "__$contract$__", {writable: false, value: Object});
		Object.defineProperty(this, "value", {
			get: getter instanceof Function ? (cached ? function(){ 
				return "cache" in this ? this.cache : this.cache = this.getter.call(this['this']); 
			} : function(){
				return this.getter.call(this['this']);
			}) : throwWriteonly,
			set: setter instanceof Function ? (cached ? function(value){
				delete this.cache;
				return this.setter.call(this['this'], value);			
			} : function(value){
				return this.setter.call(this['this'], value);
			}) : empty
		});
	}
	else {
		Object.defineProperty(this, "__$contract$__", {writable: false, value: contract});
		Object.defineProperty(this, "value", {
			get: getter instanceof Function ? (cached ? function(){
					return "cache" in this ? this.cache : this.cache = $asyncscript.binding(this.getter.call(this["this"]), this["__$contract$__"]);
				}: 
				function(){
					return $asyncscript.binding(this.getter.call(this["this"]), this["__$contract$__"]);
			}) : throwWriteonly,
			set: setter instanceof Function ? (cached ? function(value){
					delete this.cache;
					return this.setter.call(this["this"], $asyncscript.binding(value, this["__$contract$__"]));
				} : 
				function(value){
					return this.setter.call(this["this"], $asyncscript.binding(value, this["__$contract$__"]));			
			}) : empty
		});
	}
}

$asyncscript.Property = Property;
Property.prototype = {
	bind: function(_this){
		return new Property(this.getter, this.setter, this["__$contract$__"], this.isCached, _this);	
	},
	get draft(){ return "this" in this; },
	get canRead(){ return this.getter instanceof Function; },
	get canWrite(){ return this.setter instanceof Function; },
	toString: function(){ return this.canRead ? this.value : "<WRITEONLY PROPERTY>"; },
	valueOf: function(){ return this.value; }
};

Property.prototype.__$asrtl_getvalue = newLambda(function(){
	return this.value;
}, false);

Property.prototype.__$asrtl_setvalue

var container = $asyncscript.container = {
	base: {},
	byNameAccessor: function(name){ new Function(utils.format("return this['%s'];", name)); },
	sizeAccessor: function(){ return this.__$contract$__["__$size$__"]; }
};
invisibleField(container.base, "__$c$__", true);

/* container structure (optimized for named access)
	{
		__$contract$__: containerContract,
		__$size$__(){ return this["__$contract$__"]["__$size$__"]; },
		get "0"(){ return this["a"]; },
		"a": 10,
		"1": 2
	}
*/

/**
 * Creates a new container object.
 * @return {Object} A new container object.
 */
container.create = function(){
	var properties = {
		__$contract$__: {writable: false, configurable: false, enumerable: false, value: containerContract.create.apply(containerContract, arguments)},
		__$size$__: {configurable: false, enumerable: false, get: this.sizeAccessor}
	};
	for(var i = 0; i < arguments.length; i++){
		var desc = arguments[i], value;
		if(desc.get || desc.set)
			value = new Property(desc.get, desc.set, desc.contract);//draft property
		else value = $asyncscript.binding(desc.value, desc.contract);
		if(desc.name){	//if slot has name the optimize perfomance for named access
			properties[desc.name] = {configurable: false, writable: false, "value": value};
			properties[i] = {configurable: false, enumerable: false, get: this.byNameAccessor(desc.name)};				
		}
		else properties[i] = {enumerable: false, configurable: false, writable: false, "value": value};
	}
	return Object.create(this.base, properties);
};

container.empty = container.create();

/**
 * Clones the specified container.
 * @param {Object} container container A container to clone.
 * @param {Function} filter A field filter that accepts the following arguments: 0 - position of the field in the source container, 1 - the name of the field, 2 - the value of the field, 3 - the static contract of the field.
 * @return {Object} A new container object.
 */
container.clone = function(container, filter){
	var result = {
		__proto__: this.base,
		__$size$__: this.sizeAccessor,
	}, 
	length = 0;
	Object.defineProperty(cprops, "__$contract$__", {value: { __$contracts$__: {}, __proto__: containerContract.base }, enumerable: false });
	for(var i = 0; i < container["__$size$__"]; i++){
		var name = container["__$contract$__"][i], 
		c = container["__$contract$__"]["__$contracts$__"][i], 
		v = container[i];
		if(filter instanceof Function && filter(i, name, v, c) || filter === undefined){
			containerContract.putNameIndexMap(i, name, result.__$contract$__);			
			result.__$contract$__["__$contracts$__"][length] = c;
			if(name){
				result[name] = v;
				v = {get: this.byNameAccessor(name), enumerable: false};
			}
			else v = {value: v, enumerable: false};
			Object.defineProperty(result, length++, v);
		}	
	}
	switch(length){
		case 0: return this.empty;
		default:
			result.__$contract$__["__$size$__"] = length;
			return result;
	}
};

//Other contract bindings

invisibleField(RegExp, Object, String, Number, Boolean, Function, "__$contract$__", $asyncscript.typedef, false);
invisibleField(String.prototype, '__$contract$__', String);
invisibleField(Boolean.prototype, '__$contract$__', Boolean);
invisibleField(Function.prototype, '__$contract$__', Function);
invisibleField(RegExp.prototype, '__$contract$__', RegExp);
invisibleProperty(Number.prototype, '__$contract$__', function(){ return this % 1 === 0 ? $asyncscript.integer : Number; });

/**
 * Determines whether the specified object is AsyncScript Real.
 * @param {Object} value A value to test.
 * @return {Boolean} true, if the specified value is AsyncScript Real; otherwise, false.
 */
function isReal(value){
	return typeof value === 'number' || value instanceof Number;
}
$asyncscript.isReal = isReal;

/**
 * Determines whether the specified object is AsyncScript Integer.
 * @param {Object} value A value to test.
 * @return {Boolean} true, if the specified value is AsyncScript Integer; otherwise, false.
 */
function isInteger(value){
	return (typeof value === 'number' || value instanceof Number) && (value % 1 === 0);
}
$asyncscript.isInteger = isInteger;

/**
 * Determines whether the specified object is AsyncScript Integer.
 * @param {Object} value A value to test.
 * @return {Boolean} true, if the specified value is AsyncScript Integer; otherwise, false.
 */
function isString(value){
	return typeof value === 'string' || value instanceof String;
}
$asyncscript.isString = isString;

function isBoolean(value){
	return typeof value === 'boolean' || value instanceof Boolean;
}

$asyncscript.isBoolean = isBoolean;

//UNARY OPERATIONS

invisibleField(Number.prototype, "__$asrtl_unary_plus", function(){ return this.valueOf(); });
invisibleField(Number.prototype, "__$asrtl_unary_minus", function(){ return -this; });
invisibleField(Number.prototype, "__$asrtl_unary_inc", function(postfix){ return postfix ? this.valueOf() : this + 1; });
invisibleField(Number.prototype, "__$asrtl_unary_dec", function(postfix){ return postfix ? this.valueOf() : this - 1; });
invisibleField(Number.prototype, "__$asrtl_unary_square", function(postfix){ return postfix ? this.valueOf() : this * this; });
invisibleField(Number.prototype, "__$asrtl_unary_neg", function(postfix){ return ~this; });

Property.prototype.__$asrtl_unary_plus = function(){ return $asyncscript.unaryPlus(this.value); };
Property.prototype.__$asrtl_unary_minus = function(){ return $asyncscript.unaryMinus(this.value); };
Property.prototype.__$asrtl_unary_inc = function(postfix){
	var result;
	if(postfix){
		result = this.value;
		this.value = $asyncscript.increment(this.value);
	}
	else result = $asyncscript.increment(this.value);
	return result;
};
Property.prototype.__$asrtl_unary_dec = function(postfix){
	var result;
	if(postfix){
		result = this.value;
		this.value = $asyncscript.decrement(this.value);
	}
	else result = $asyncscript.decrement(this.value);
	return result;
};
Property.prototype.__$asrtl_unary_neg = function(){ return $asyncscript.negation(this.value); };
Property.prototype.__$asrtl_unary_square = function(postfix){
	var result;
	if(postfix){
		result = this.value;
		this.value = $asyncscript.square(this.value);
	}
	else result = $asyncscript.square(this.value);
	return result;
};
invisibleField(Number, Boolean, String, RegExp, Function, Object, "__$asrtl_unary_neg",
Union.prototype.__$asrtl_unary_neg = 
Signature.prototype.__$asrtl_unary_neg =
Complementation.prototype.__$asrtl_unary_neg =
Vector.prototype.__$asrtl_unary_neg = 
ArrayContract.prototype.__$asrtl_unary_neg = 
$asyncscript.typedef.__$asrtl_unary_neg = 
$asyncscript.integer.__$asrtl_unary_neg = 
FilterContract.prototype.__$asrtl_unary_neg =
containerContract.base.__$asrtl_unary_neg = 
Expression.__$asrtl_unary_neg = function(){  return $asyncscript.complementation(this); },
false);

invisibleField(Boolean.prototype, ["__$asrtl_unary_neg", "__$asrtl_unary_minus"], function(){ return !this; });

invisibleField(Boolean.prototype, ["__$asrtl_unary_plus", "__$asrt_unary_square"], function(){ return this.valueOf(); });

invisibleField(Boolean.prototype, "__$asrtl_unary_dec", function(postfix){ return postfix ? this.valueOf() : false; });

invisibleField(Boolean.prototype, "__$asrtl_unary_inc", function(postfix){ return postfix ? this.valueOf() : true; });

invisibleField(String.prototype, "__$asrtl_unary_dec", function(postfix){
	return postfix ? this.valueOf() : this.substr(0, this.length - 1);
});

invisibleField(String.prototype, "__$asrtl_unary_square", function(postfix){
	return postfix ? this.valueOf() : this + this;
});

invisibleField(String.prototype, "__$asrtl_unary_neg", function(postfix){
	var result = '';
	for(var i = this.length - 1; i >= 0; i--) result += this[i];
	return result;
});

Expression.prototype.__$asrtl_unary_neg = function(){ 
	var ast = require('./ast.js'), CodeUnaryExpression = ast.CodeUnaryExpression;
	return new Expression(new CodeUnaryExpression(ast.parseOperator('!'), this.expr, this.expr.position.column, this.expr.position.line));
};

Expression.prototype.__$asrtl_unary_plus = function(){ 
	var ast = require('./ast.js'), CodeUnaryExpression = ast.CodeUnaryExpression;
	return new Expression(new CodeUnaryExpression(ast.parseOperator('+'), this.expr, this.expr.position.column, this.expr.position.line));
};

Expression.prototype.__$asrtl_unary_minus = function(){ 
	var ast = require('./ast.js'), CodeUnaryExpression = ast.CodeUnaryExpression;
	return new Expression(new CodeUnaryExpression(ast.parseOperator('-'), this.expr, this.expr.position.column, this.expr.position.line));
};

Expression.prototype.__$asrtl_unary_square = function(postfix){ 
	var ast = require('./ast.js'), CodeUnaryExpression = ast.CodeUnaryExpression;
	return new Expression(new CodeUnaryExpression(ast.parseOperator('**', postfix), this.expr, this.expr.position.column, this.expr.position.line));
};

Expression.prototype.__$asrtl_unary_inc = function(postfix){ 
	var ast = require('./ast.js'), CodeUnaryExpression = ast.CodeUnaryExpression;
	return new Expression(new CodeUnaryExpression(ast.parseOperator('++', postfix), this.expr, this.expr.position.column, this.expr.position.line));
};

Expression.prototype.__$asrtl_unary_dec = function(postfix){ 
	var ast = require('./ast.js'), CodeUnaryExpression = ast.CodeUnaryExpression;
	return new Expression(new CodeUnaryExpression(ast.parseOperator('--', postfix), this.expr, this.expr.position.column, this.expr.position.line));
};

containerContract.base.__$asrtl_unary_neg = function(){
	var operator;
	if(operator = this["!"]) return $asyncscript.invoke(this, operator, []);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("negation");
	else return this;
};

containerContract.base.__$asrtl_unary_plus = function(){
	var operator;
	if(operator = this["unary+"]) return $asyncscript.invoke(this, operator, []);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("unaryPlus");
	else return this;
};

containerContract.base.__$asrtl_unary_minus = function(){
	var operator;
	if(operator = this["unary-"]) return $asyncscript.invoke(this, operator, []);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("unaryMinus");
	else return this;
};

containerContract.base.__$asrtl_unary_square = function(postfix){
	var operator;
	if(operator = this["**"]) return $asyncscript.invoke(this, operator, [postfix]);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("negation");
	else return this;
};

containerContract.base.__$asrtl_unary_inc = function(postfix){
	var operator;
	if(operator = this["++"]) return $asyncscript.invoke(this, operator, [postfix]);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("increment");
	else return this;
};

containerContract.base.__$asrtl_unary_dec = function(postfix){
	var operator;
	if(operator = this["--"]) return $asyncscript.invoke(this, operator, [postfix]);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("decrement");
	else return this;
};

invisibleField(Function.prototype, "__$asrtl_unary_neg", function(){ return delayUnaryOperand(this, "negation"); });
OverloadList.prototype.__$asrtl_unary_neg = function(){ return this.delayUnaryOperand("negation"); };

invisibleField(Function.prototype, "__$asrtl_unary_plus", function(){ return delayUnaryOperand(this, "unaryPlus"); });
OverloadList.prototype.__$asrtl_unary_plus = function(){ return this.delayUnaryOperand("unaryPlus"); };

invisibleField(Function.prototype, "__$asrtl_unary_minus", function(){ return delayUnaryOperand(this, "unaryMinus"); });
OverloadList.prototype.__$asrtl_unary_minus = function(){ return this.delayUnaryOperand("unaryMinus"); };

invisibleField(Function.prototype, "__$asrtl_unary_square", function(postfix){ return delayUnaryOperand(this, "square", postfix); });
OverloadList.prototype.__$asrtl_unary_square = function(postfix){ return this.delayUnaryOperand("square", postfix); };

invisibleField(Function.prototype, "__$asrtl_unary_inc", function(postfix){ return delayUnaryOperand(this, "increment", postfix); });
OverloadList.prototype.__$asrtl_unary_inc = function(postfix){ return this.delayUnaryOperand("increment", postfix); };

invisibleField(Function.prototype, "__$asrtl_unary_dec", function(postfix){ return delayUnaryOperand(this, "unaryPlus", postfix); });
OverloadList.prototype.__$asrtl_unary_plus = function(postfix){ return this.delayUnaryOperand("unaryPlus", postfix); };

//unsupported operations
invisibleField(Array.prototype, RegExp.prototype, "__$asrtl_unary_neg", OverloadList.prototype.__$asrtl_unary_neg = function(){
	if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("negation");
	else return this;
}, false);

invisibleField(Number, Boolean, String, RegExp, Function, Object, Array.prototype, RegExp.prototype, String.prototype, "__$asrtl_unary_inc",
Union.prototype.__$asrtl_unary_inc = 
Signature.prototype.__$asrtl_unary_inc =
Complementation.prototype.__$asrtl_unary_inc =
Vector.prototype.__$asrtl_unary_inc = 
ArrayContract.prototype.__$asrtl_unary_inc = 
$asyncscript.typedef.__$asrtl_unary_inc = 
$asyncscript.integer.__$asrtl_unary_inc = 
FilterContract.prototype.__$asrtl_unary_inc =
containerContract.base.__$asrtl_unary_inc = 
Expression.__$asrtl_unary_inc = function(){
	if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("increment");
	else return this.valueOf();
},
false);

invisibleField(Number, Boolean, String, RegExp, Function, Object, Array.prototype, RegExp.prototype, "__$asrtl_unary_dec", 
Union.prototype.__$asrtl_unary_dec = 
Complementation.prototype.__$asrtl_unary_dec =
Vector.prototype.__$asrtl_unary_dec = 
ArrayContract.prototype.__$asrtl_unary_dec = 
$asyncscript.typedef.__$asrtl_unary_dec = 
FilterContract.prototype.__$asrtl_unary_dec =
$asyncscript.integer.__$asrtl_unary_dec =
Expression.__$asrtl_unary_dec = function(){
	if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("decrement");
	else return this.valueOf();
},
false);

Signature.prototype.__$asrtl_unary_dec = function(postfix){
	return this.__$size$__ === 0 ? this :
	this.clone(postfix ? function(i){ return i > (this - 1); }.bind(this.__$size$__) : function(i){ return i > 0; });
};

containerContract.base.__$asrtl_unary_dec = function(postfix){
	return this.__$size$__ === 0 ? this :
	containerContract.clone(this, postfix ? function(i){ return i > (this - 1); }.bind(this.__$size$__) : function(i){ return i > 0; });
};

invisibleField(Number, Boolean, String, RegExp, Function, Object, Array.prototype, RegExp.prototype, String.prototype, "__$asrtl_unary_plus",
Union.prototype.__$asrtl_unary_plus = 
FilterContract.prototype.__$asrtl_unary_plus =
Signature.prototype.__$asrtl_unary_plus =
Complementation.prototype.__$asrtl_unary_plus =
Vector.prototype.__$asrtl_unary_plus = 
ArrayContract.prototype.__$asrtl_unary_plus = 
$asyncscript.typedef.__$asrtl_unary_plus = 
$asyncscript.integer.__$asrtl_unary_plus = 
containerContract.base.__$asrtl_unary_plus = 
Expression.__$asrtl_unary_plus = function(){
	if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("unaryPlus");
	else return this.valueOf();
},
false);

invisibleField(Number, Boolean, String, RegExp, Function, Object, Array.prototype, RegExp.prototype, String.prototype, "__$asrtl_unary_minus", 
Union.prototype.__$asrtl_unary_minus = 
FilterContract.prototype.__$asrtl_unary_minus =
Signature.prototype.__$asrtl_unary_minus =
Complementation.prototype.__$asrtl_unary_minus =
Vector.prototype.__$asrtl_unary_minus = 
ArrayContract.prototype.__$asrtl_unary_minus = 
$asyncscript.typedef.__$asrtl_unary_minus = 
$asyncscript.integer.__$asrtl_unary_minus = 
containerContract.base.__$asrtl_unary_minus = 
Expression.__$asrtl_unary_minus = function(){
	if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("unaryMinus");
	else return this.valueOf();
},
false);

invisibleField(Number, Boolean, String, RegExp, Function, Object, Array.prototype, RegExp.prototype,
Union.prototype.__$asrtl_unary_square = 
Signature.prototype.__$asrtl_unary_square =
Complementation.prototype.__$asrtl_unary_square =
Vector.prototype.__$asrtl_unary_square = 
ArrayContract.prototype.__$asrtl_unary_square = 
$asyncscript.typedef.__$asrtl_unary_square = 
$asyncscript.integer.__$asrtl_unary_square = 
FilterContract.prototype.__$asrtl_unary_square =
containerContract.base.__$asrtl_unary_square =
Expression.__$asrtl_unary_square = function(){
	if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("square");
	else return this.valueOf();
},
false);

//Load unary operators
[{name: "negation", voidHandler: "return Object;", operation: "__$asrtl_unary_neg"},
{name: "unaryPlus", voidHandler: "return null;", operation: "__$asrtl_unary_plus"},
{name: "unaryMinus", voidHandler: "if(this.state.checked) throw this.errors.unsupportedOp('unaryMinus'); else return null;", operation: "__$asrtl_unary_minus"},
{name: "square", voidHandler: "if(this.state.checked) throw this.errors.unsupportedOp('square'); else return null;", operation: "__$asrtl_unary_square"},
{name: "increment", voidHandler: "if(this.state.checked) throw this.errors.unsupportedOp('increment'); else return null;", operation: "__$asrtl_unary_inc"},
{name: "decrement", voidHandler: "if(this.state.checked) throw this.errors.unsupportedOp('decrement'); else return null;", operation: "__$asrtl_unary_dec"},
].forEach(function(op){
	var impl = 
	utils.format("if(obj === null || obj == undefined) %s", op.voidHandler) +
	utils.format("else if(obj.%s) return obj.%s(postfix);", op.operation, op.operation) +
	"else if(obj instanceof this.Promise)" +
	"\t if(obj.isError) throw obj.result;" +
	utils.format("\t else if(obj.isCompleted) return this.%s(obj.result, postfix);", op.name) +
	utils.format("\t else return this.fork(function(obj){ return $asyncscript.%s(obj, postfix); }, obj);", op.name) +
	utils.format("else if(this.state.checked) throw this.errors.unsupportedOp('%s');", op.name) +
	"else return obj;";
	$asyncscript[op.name] = new Function("obj", "postfix", impl);
});

//RELATIONSHIP
function inverseRelationship(rels){
	switch(rels){
		case "subset": return "superset";
		case "superset": return "subset";
		default: return rels;
	}
}

/**
 * Creates a new union of the two contracts.
 * @param {Object} contract1 The first contract to unite.
 * @param {Object} contract2 The second contract to unite.
 * @return {Object} A union of the two contracts.
 */
Union.create = function(contract1, contract2){
	switch(arguments.length){
		case 0: return null;
		case 1: return arguments[0];
		case 2:
			switch($asyncscript.relationship(contract1, contract2)){
				case "superset":
				case "equal": return contract1;
				case "subset": return contract2;
				case "different": return new Union(contract1, contract2);
			}
		default:
			var contracts = new Array();
			for(var i = 0; i < arguments.length; i++)
				contracts.push(arguments[i]);
			var result = contracts.shift();
			while(contracts.length > 0)
				result = this.create(result, contracts.shift());
			return result;
	}
};

invisibleField(Object, '__$asrtl_relationship', function(contract){
	return contract === this ? "equal" : "superset";
}, false);

$asyncscript.typedef.__$asrtl_relationship = function(contract){
	switch(contract){
		case Object: return "subset";
		case this: return "equal";
		case undefined:
		case null: return "superset";
		default:
			if(contract instanceof Signature || contract['__$cc$__'] || contract instanceof ArrayContract || contract instanceof Vector) 
				return "superset";
			else if(contract instanceof Union || contract instanceof Complementation) 
				return inverseRelationship(contract.__$asrtl_relationship(this));
			else return "different";
	}
};

ArrayContract.prototype.__$asrtl_relationship = function(contract){
	switch(contract){
		case $asyncscript.typedef:
		case Object: return "subset";
		case this: return "equal";
		case undefined:
		case null: return "superset";
		default:
			if(contract instanceof Vector)
				switch($asyncscript.relationship(this.contract, contract.contract)){
					case "equal":
					case "superset": return "superset";
					default: return "different";
				}
			else if(contract.__$cc$__)
				return inverseRelationship(contract.__$asrtl_relationship(this));
			else if(contract instanceof Union || contract instanceof Complementation)
				return inverseRelationship(contract.__$asrtl_relationship(this));
			else return "different"; 
	}
};

$asyncscript.integer.__$asrtl_relationship = function(contract){
	switch(contract){
		case this: return "equal";
		case Number:
		case Object: return "subset";
		case Boolean:
		case undefined:
		case null: return "superset";
		default:
			if(contract instanceof Union || contract instanceof Complementation) 
				return inverseRelationship(contract.__$asrtl_relationship(this));
			else return "different";
	}
};

invisibleField(Number, '__$asrtl_relationship', function(contract){
	switch(contract){
		case this: return "equal";
		case $asyncscript.integer:
		case null:
		case undefined:		
		case Boolean: return "superset";
		case Object: return "subset";
		default: 
			return contract instanceof Union || contract instanceof Complementation ?
				inverseRelationship(contract.__$asrtl_relationship(this)):
				"different";
	}
});

invisibleField(String, '__$asrtl_relationship', function(contract){
	switch(contract){
		case this: return "equal";
		case Object: return "subset";
		case undefined:
		case null: return "superset";
		default: 
			return contract instanceof Union || contract instanceof Complementation ?
				inverseRelationship(contract.__$asrtl_relationship(this)):
				"different";
	}
}, false);

invisibleField(Boolean, '__$asrtl_relationship', function(contract){
	switch(contract){
		case this: return "equal";
		case Number:
		case $asyncscript.integer:
		case Object: return "subset";
		case undefined:
		case null: return "superset";
		default: 
			return contract instanceof Union || contract instanceof Complementation ?
				inverseRelationship(contract.__$asrtl_relationship(this)):
				"different";
	}
}, false);

invisibleField(RegExp, '__$asrtl_relationship', function(contract){
	switch(contract){
		case this: return "equal";
		case Object: return "subset";
		case null:
		case undefined: return "superset";
		default: 
			return contract instanceof Union || contract instanceof Complementation ?
				inverseRelationship(contract.__$asrtl_relationship(this)):
				"different";
	}
}, false);

Union.prototype.__$asrtl_relationship = function(contract){
	if(this === contract) return "equal";
	switch($asyncscript.relationship(this.first)){
		case "equal":
		case "superset": return "superset";
		case "subset": return "different";
	}
	switch($asyncscript.relationship(this.second)){
		case "equal":
		case "superset": return "superset";
		default: return "different";
	}
};

Complementation.prototype.__$asrtl_relationship = function(contract){
	if(contract instanceof Complementation)
		switch($asyncscript.relationship(this.contract, contract.contract)){
			case "equal": return "equal";
			case "subset": return "subset";
			case "superset": return "superset";
			default: return "different";
		}
	else if(contract === Object) 
		return "subset";
	else if($asyncscript.relationship(this.contract, contract) === "equal") 
		return "different";
	else return "superset";
};

Vector.prototype.__$asrtl_relationship = function(contract){
	if(contract === $asyncscript.typedef || contract === Object) return "subset";
	else if(contract === null || contract === undefined) return "superset";
	//is container contract?
	else if(contract.__$cc$__ || contract instanceof ArrayContract) return inverseRelationship(contract.__$asrtl_relationship(this));
	else if(contract instanceof Vector)
		switch($asyncscript.relationship(this.contract, contract.contract)){
			case "equal": 
				if(this.__$size$__ === contract.__$size$__) return "equal";
				else if(this.__$size$__ > contract.__$size$__) return "subset";
				else return "superset";
			case "different":
			case "superset": 
				return "different";
			case "subset":
				return this.__$size$__ >= contract.__$size$__ ? "subset" : "different";
		}
	else return contract instanceof Union || contract instanceof Complementation ?
				inverseRelationship(contract.__$asrtl_relationship(this)):
				"different";
};

containerContract.base.__$asrtl_relationship = function(contract){
	var result =  "equal";
	if(contract === $asyncscript.typedef || contract === Object) result = "subset";
	else if(contract === null || contract === undefined) result = "superset";
	else if(contract["__$cc$__"])
		if(this.__$size$__ === 0) result = contract.__$size$__ === 0 ? "equal" : "superset";
		else if(contract.__$size$__ === 0) result = "subset";
		//check boundary conditions
		else for(var i = 0; i < this.__$size$__; i++){
			var name1 = this[i], idx2;
			//the named field exists in both contracts
			if(name1 && (idx2 = contract[name1]))
				switch($asyncscript.relationship(this.__$contracts$__[i], contract.__$contracts$__[idx2])){
					case "equal": continue;
					case "subset":
						if(result === "subset" || result === "equal") result = "subset";
						else return "different";
					continue;
					case "superset":
						if(result === "superset" || result === "equal") result = "superset";
						else return "different";
					continue;
					default: return "different";
				}
			//named property is omitted, compare layout
			else if(i < contract.__$size$__)
				switch($asyncscript.relationship(this.__$contracts$__[i], contract.__$contracts$__[i])){
					case "equal": continue;
					case "subset":
						if(result === "subset" || result === "equal") result = "subset";
						else return "different";
					continue;
					case "superset":
						if(result === "superset" || result === "equal") result = "superset";
						else return "different";
					continue;
					default: return "different";				
				}
			//out of bounds in the second contract
			else switch(result){
				case "equal":
				case "subset": return "subset";
				default: return "different";			
			}
		}
	else if(contract instanceof ArrayContract){
		if(contract.__$size$__ === 0) return "subset";
		var field, result;
		for(var i = 0; (field = this.__$contracts$__[i], i < this.__$size$__); i++)
			switch($asyncscript.relationship(field, contract.contract)){
				case "equal": continue;
				case "superset":
					if(result === "superset" || result === "equal")
						result = "superset";
					else return "different";
				continue;
				case "subset":
					if(result === "subset" || result === "equal")
						result = "subset";
					else return "different";
				default: return "different";
			}
	}
	else if(contract instanceof Vector) {
			if(this.__$size$__ < contract.__$size$__) result = "superset";
			else if(this.__$size$__ > contract.__$size$__) result = "subset";
			
			for(var i = 0; i < Math.min(contract.__$size$__, this.__$size$__); i++)
				switch($asyncscript.relationship(this.__$contracts$__[i], contract.contract)){
					case "superset": 
						if(result === "equal" || result === "superset")
							result = "superset";
						else return "different";
					continue;
					case "different": return "different";
					case "subset": 
						if(result === "equal" || result === "subset")
							result = "subset"; 
						else return "different";
					continue;
				}
		}
	else result = (contract instanceof Union || contract instanceof Complementation) ?
				inverseRelationship(contract.__$asrtl_relationship(this)):
				"different";
	return result;
};

Signature.prototype.__$asrtl_relationship = function(contract){
	switch(contract){
		case $asyncscript.typedef:
		case Object:
		case Function: return "subset";
		default:
			var result = "equal";
			if(contract instanceof Signature)
				if(this.__$size$__ === 0) result = contract.__$size$__ === 0 ? "equal" : "subset";
				else if(contract.__$size$__ === 0) result = "superset";
				else for(var i = 0; i < this.__$size$__; i++)
					if(i < contract.__$size$__)
						switch($asyncscript.relationship(this[i], contract[i])){
							case "equal": continue;
							case "subset":
								if(result === "equal" || result === "superset") result = "superset";
								else return "different";
							continue;
							case "superset":
								if(result === "equal" || result === "subset") result = "subset";
								else return "different";
							continue;
							default: return "different";					
						}
					else switch(result){
						case "equal":
						case "superset": return "superset";
						default: return "different";					
					}
			else result = (contract instanceof Union || contract instanceof Complementation) ?
				inverseRelationship(contract.__$asrtl_relationship(this)):
				"different";
		return result;
	}
};

invisibleField(Function, '__$asrtl_relationship', function(contract){
	switch(contract){
		case undefined:
		case null: return "superset";
		case this: return "equal";
		case Object: return "subset";
		default:
			if(contract instanceof Signature) return "superset";
			else if(contract instanceof Union || contract instanceof Complementation) 
				return inverseRelationship(contract.__$asrtl_relationship(this));
			else return "different";
	}
}, false);

invisibleProperty(Array.prototype, '__$contract$__', function(){
	return this.length === 0 ? containerContract.empty : new Vector(Object, this.length);
});

invisibleProperty(Object.prototype, '__$contract$__', function(){
	var names = Object.getOwnPropertyNames(this), result;
	if(names.length === 0) result = containerContract.empty;
	else {
		result = {__proto__: containerContract.base, __$size$__: names.length, __$contracts$__: {}};
		names.forEach(function(field, i){
			this[i] = field;
			this[field] = i;
			this.__$contracts$__[i] = Object;
		}, result);
	}
	return result;
});

invisibleProperty(Object.prototype, '__$size$__', function(){ return Object.getOwnPropertyNames(this).length; });

$asyncscript.relationship = function(contract1, contract2){	
	if(contract1 === contract2) return "equal";
	else if(contract1 === null || contract1 === undefined)
		switch(contract2){
			case null:
			case undefined: return "equal";
			default: return "different";
		}
	else if(contract1.__$asrtl_relationship instanceof Function) return contract1.__$asrtl_relationship(contract2);
	else return "different";
};

//BINARY

$asyncscript.instanceOf = function(left, right){
	if(left === null || left === undefined) return right === null || right === undefined;
	else if(left instanceof Property) return this.instanceOf(left.value, right);
	else if(right instanceof Property) return this.instanceOf(right.value, right);
	else if(left instanceof Promise)
		if(left.isError) throw left.result;
		else if(left.isCompleted) left = this.instanceOf(left.result, right);
		else return this.fork(function(left){ return $asyncscript.instanceOf(left, right); }, left);
	else if(right instanceof Promise)
		if(right.isError) throw right.result;
		else if(right.isCompleted) return this.instanceOf(left, right.value);
		else return this.fork(function(right){ return $asyncscript.instanceOf(left, right); }, right);
	else if(right && right.__$asrtl_contractfor) return right.__$asrtl_contractfor(left);
	else switch(this.relationship(left.__$contract$__, right)){
			case "equal":
			case "subset": return true;
			default: return false;
	}
};

$asyncscript.contains = function(left, right){
	if(right === null || right === undefined) return false;
	else if(right instanceof Promise)
		if(right.isError) throw right.result;
		else if(right.isCompleted) return this.contains(left, right.result);
		else return this.fork(function(right){ return $asyncscript.contains(left, right); }, right);
	else if(left instanceof Promise)
		if(left.isError) throw left.result;
		else if(left.isCompleted) return this.contains(left.result, right);
		else return this.fork(function(left){ return $asyncscript.contains(left, right); }, left);
	else if(right.__$asrtl_contains instanceof Function) return right.__$asrtl_contains(left);
	else return false;
};

$asyncscript.areEqual = function(left, right){
	if(left === this.anyvalue || right === this.anyvalue) return true;
	else if(left instanceof this.Promise)
		if(left.isError) throw left.result;
		else if(left.isCompleted) return this.areEqual(left.result, right); 
		else return this.fork(function(left){ return $asyncscript.areEqual(left, right); }, left);
	else if(right instanceof this.Promise)
		if(right.isError) throw right.result;
		else if(right.isCompleted) return this.areEqual(left, right.result);
		else return this.fork(function(right){ return $asyncscript.areEqual(left, right); }, right);
	else if(left === null || left === undefined) return right === null || right === undefined;
	else if(left.__$asrtl_binary_equ) return left.__$asrtl_binary_equ(right);
	else if(this.state.checked) throw this.runtimeErrors.unsupportedOp('areEqual');
	else return false;
};

$asyncscript.redirectTo = function(value, destination){
	return this.ret(value, destination), value;
};

$asyncscript.areNotEqual = function(left, right){ 
	if(left === null || left === undefined) return right !== null && right !== undefined;
	else if(left === this.anyvalue) return false;
	else if(left.__$asrtl_binary_neq) return left.__$asrtl_binary_neq(right);
	else return this.negation(this.areEqual(left, right));
};

//load binary operators
[{name: "binaryPlus", leftVoid: "return (right === null || right === undefined)? null : this.binaryPlus(right, null);", operation: "__$asrtl_binary_plus", operator: "+"},
{name: "binaryMinus", leftVoid: "if(this.state.checked) throw this.runtimeErrors.unsupportedOp('binaryMinus'); else return null;", operation: "__$asrtl_binary_minus", operator: "-"},
{name: "multiplication", leftVoid: "return (right === null || right === undefined)? null : this.multiplication(right, null);", operation: "__$asrtl_binary_mul", operator: "*"},
{name: "xor", leftVoid: "return (right === null || right === undefined)? null : this.xor(right, null);", operation: "__$asrtl_binary_xor", operator: "^"},
{name: "division", leftVoid: "if(this.state.checked) throw this.runtimeErrors.unsupportedOp('division'); else return null;", operation: "__$asrtl_binary_div", operator: "/"},
{name: "and", leftVoid: "return (right === null || right === undefined)? null : this.and(right, null);", operation: "__$asrtl_binary_and", operator: "&"},
{name: "or", leftVoid: "return (right === null || right === undefined)? null : this.or(right, null);", operation: "__$asrtl_binary_or", operator: "|"},
{name: "greaterThan", leftVoid: "return (right === null || right === undefined)? false : this.lessThan(right, null);", operation: "__$asrtl_binary_gt", operator: ">"},
{name: "lessThan", leftVoid: "return (right === null || right === undefined)? false : this.greaterThan(right, null);", operation: "__$asrtl_binary_lt", operator: "<"},
{name: "greaterThanOrEqual", leftVoid: "return (right === null || right === undefined)? true : this.lessThanOrEqual(right, null);", operation: "__$asrtl_binary_gte", operator: ">="},
{name: "lessThanOrEqual", leftVoid: "return (right === null || right === undefined)? true : this.greaterThanOrEqual(right, null);", operation: "__$asrtl_binary_lte", operator: "<="},
{name: "shiftLeft", leftVoid: "if(this.state.checked) throw this.runtimeErrors.unsupportedOp('shiftLeft'); else return null;", operation: "__$asrtl_binary_shl", operator: "<<"},
{name: "shiftRight", leftVoid: "if(this.state.checked) throw this.runtimeErrors.unsupportedOp('shiftRight'); else return null;", operation: "__$asrtl_binary_shr", operator: ">>"},
{name: "modulo", leftVoid: "if(this.state.checked) throw this.runtimeErrors.unsupportedOp('modulo'); else return null;", operation: "__$asrtl_binary_mod", operator: "%"},
{name: "typecast", leftVoid: "return  (right === null || right === undefined) ? null : this.invoke(undefined, right, []);", operation: "__$asrtl_typecast", operator: "to"}
].forEach(function(op){
	var impl = 
	"if(left instanceof this.Promise)" +
	"\t if(left.isError) throw left.result;" +
	utils.format("\t else if(left.isCompleted) return this.%s(left.result, right);", op.name) +
	utils.format("\t else return this.fork(function(left){ return $asyncscript.%s(left, right); }, left);", op.name) +
	"else if(right instanceof this.Promise)" +
	"\t if(right.isError) throw right.result;" +
	utils.format("\t else if(right.isCompleted) return this.%s(left, right.result);", op.name) +
	utils.format("\t else return this.fork(function(right){ return $asyncscript.%s(left, right); }, right);", op.name) +
	utils.format("else if(left === null || left === undefined) %s", op.leftVoid) +
	utils.format("else if(left.%s) return left.%s(right);", op.operation, op.operation) +
	utils.format("else if(this.state.checked) throw this.runtimeErrors.unsupportedOp('%s');", op.name) +
	"else return left;";
	$asyncscript[op.name] = new Function("left", "right", impl);
	//load binary operators for Property
	Property.prototype[op.operation] = new Function("right", utils.format("return $asyncscript.%s(this.value, right);", op.name));
	//load binary operators for Expression
	Expression.prototype[op.operation] = new Function("right", utils.format("right = $asyncscript.Expression.convert(right, this.tree.position);" +
		"var ast = require('./ast.js'), CodeBinaryExpression = ast.CodeBinaryExpression;" +
		"right = new CodeBinaryExpression(this.tree, ast.parseOperator('%s', true), right, this.tree.position.column, this.tree.position.line);" +
		"return new $asyncscript.Expression(right, true);", op.operator));
});

invisibleField(Boolean.prototype, "__$asrtl_binary_xor", function(right){
	if(right === null || right === undefined) return this.valueOf();
	else if(isBoolean(right) || isInteger(right)) return this ^ right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "xor", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("xor", this);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('xor');
	else return this.valueOf();
});

invisibleField(Boolean.prototype, "__$asrtl_binary_plus", function(right){
	if(right === null || right === undefined) return this.valueOf();
	else if(isBoolean(right) || isReal(right) || isString(right)) return this + right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "binaryPlus", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("binaryPlus", this);	
	else if(right.__$asrtl_insert instanceof Function) return right.__$asrtl_insert(this, 0);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryPlus');
	else return this.valueOf();
});

invisibleField(Boolean.prototype, "__$asrtl_binary_minus", function(right){
	if(right === null || right === undefined) return Number(this);
	else if(isBoolean(right) || isReal(right)) return this - right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "binaryMinus", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("binaryMinus", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryMinus');
	else return this.valueOf();
});

invisibleField(Boolean.prototype, "__$asrtl_binary_mul", function(right){
	if(right === null || right === undefined) return 0;
	else if(isBoolean(right) || isReal(right)) return this * right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "multiplication", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("multiplication", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('multiplication');
	else return this.valueOf();
});

invisibleField(Boolean.prototype, "__$asrtl_binary_div", function(right){
	if(right === null || right === undefined) return NaN;
	else if(isBoolean(right) || isReal(right)) return this / right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "division", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("division", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('division');
	else return this.valueOf();
});

invisibleField(Boolean.prototype, "__$asrtl_binary_and", function(right){
	if(right === null || right === undefined) return false;
	else if(isBoolean(right) || isInteger(right)) return this & right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "and", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("and", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('AND');
	else return this.valueOf();
});

invisibleField(Boolean.prototype, "__$asrtl_binary_or", function(right){
	if(right === null || right === undefined) return this.valueOf();
	else if(isBoolean(right) || isInteger(right)) return this | right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "or", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("or", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('OR');
	else return this.valueOf();
});

invisibleField(Boolean.prototype, "__$asrtl_binary_mod", function(right){
	if(right === null || right === undefined) return NaN;
	else if(isBoolean(right) || isReal(right)) return this % right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "modulo", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("modulo", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('modulo');
	else return this.valueOf();
});

invisibleField(Boolean.prototype, "__$asrtl_binary_shr", function(right){
	if(right === null || right === undefined) return Number(this);
	else if(isBoolean(right) || isInteger(right)) return this >> right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "shiftRight", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("shiftRight", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('shiftRight');
	else return this.valueOf();
});

invisibleField(Boolean.prototype, "__$asrtl_binary_shl", function(right){
	if(right === null || right === undefined) return Number(this);
	else if(isBoolean(right) || isInteger(right)) return this << right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "shiftLeft", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("shiftLeft", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('shiftLeft');
	else return this.valueOf();
});

invisibleField(Boolean.prototype, "__$asrtl_binary_equ", function(right){
	if(right === null || right === undefined) return !this;
	else if(isBoolean(right) || isInteger(right)) return this == right;
	else return false;
});

invisibleField(Boolean.prototype, "__$asrtl_binary_lte", function(right){
	if(right === null || right === undefined) return !this;
	else if(isBoolean(right) || isReal(right)) return this <= right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "lessThanOrEqual", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("lessThanOrEqual", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('lessThanOrEqual');
	else return false;
});

invisibleField(Boolean.prototype, "__$asrtl_binary_gte", function(right){
	if(right === null || right === undefined) return true;
	else if(isBoolean(right) || isReal(right)) return this >= right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "greaterThanOrEqual", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("greaterThanOrEqual", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('greaterThanOrEqual');
	else return false;
});

invisibleField(Boolean.prototype, "__$asrtl_binary_gt", function(right){
	if(right === null || right === undefined) return this.valueOf();
	else if(isBoolean(right) || isReal(right)) return this > right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "greaterThan", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("greaterThan", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('greaterThan');
	else return false;
});

invisibleField(Boolean.prototype, "__$asrtl_binary_lt", function(right){
	if(right === null || right === undefined) return false;
	else if(isBoolean(right) || isReal(right)) return this < right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "lessThan", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("lessThan", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('lessThan');
	else return false;
});

invisibleField(Boolean.prototype, "__$asrtl_typecast", function(type){
	switch(type){
		case String: return this.toString();
		case $asyncscript.integer:
		case Number: return this ? 1 : 0;
		case Boolean: return this.valueOf();
		case undefined:
		case null: return false;
		case RegExp: return new RegExp(this.toString());
		case Function: return thisProvider.bind(this).toLambda(false);
		default:
			if(type instanceof Signature) return thisProvider.bind(this).toLambda(type);
			else if(type instanceof FilterContract) return type.typecast(this);
			if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("typecast");
			else return null;
	}
});

invisibleField(Boolean, "__$asrtl_implicit", Boolean);

invisibleField(Number.prototype, '__$asrtl_binary_xor', function(right){
	if(right === null || right === undefined) return this;
	else if(isInteger(this) && (isBoolean(right) || isInteger(right))) return this ^ right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "xor", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("xor", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('XOR');
	else return this.valueOf();
});

invisibleField(Number.prototype, '__$asrtl_binary_plus', function(right){
	if(right === null || right === undefined) return this.valueOf();
	else if(isBoolean(right) || isReal(right) || isString(right)) return this + right;
	else if(right.__$asrtl_insert instanceof Function) return right.__$asrtl_insert(this, 0);
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "binaryPlus", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("binaryPlus", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryPlus');
	else return this.valueOf();
});

invisibleField(Number.prototype, '__$asrtl_binary_minus', function(right){
	if(right === null || right === undefined) return this.valueOf();
	else if(isBoolean(right) || isReal(right)) return this - right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "binaryMinus", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("binaryMinus", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryMinus');
	else return this.valueOf();
});

invisibleField(Number.prototype, '__$asrtl_binary_mul', function(right){
	if(right === null || right === undefined) return 0;
	else if(isBoolean(right) || isReal(right)) return this * right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "multiplication", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("multiplication", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('multiplication');
	else return this.valueOf();
});

invisibleField(Number.prototype, '__$asrtl_binary_or', function(right){
	if(right === null || right === undefined) return this.valueOf();
	else if(isInteger(this) && (isBoolean(right) || isInteger(right))) return this | right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "or", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("or", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('OR');
	else return this.valueOf();
});

invisibleField(Number.prototype, '__$asrtl_binary_and', function(right){
	if(right === null || right === undefined) return 0;
	else if(isInteger(this) && isBoolean(right) || isInteger(right)) return this & right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "and", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("and", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('AND');
	else return this.valueOf();
});

invisibleField(Number.prototype, '__$asrtl_binary_div', function(right){
	if(right === null || right === undefined) return NaN;
	else if(isBoolean(right) || isReal(right)) return this / right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "division", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("division", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('division');
	else return this.valueOf();
});

invisibleField(Number.prototype, '__$asrtl_binary_mod', function(right){
	if(right === null || right === undefined) return NaN;
	else if(isBoolean(right) || isReal(right)) return this % right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "modulo", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("modulo", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('modulo');
	else return this.valueOf();
});

invisibleField(Number.prototype, '__$asrtl_binary_shr', function(right){
	if(right === null || right === undefined) return this.valueOf();
	else if(isInteger(this) && (isBoolean(right) || isInteger(right))) return this >> right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "shiftRight", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("shiftRight", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('shiftRight');
	else return this.valueOf();
});

invisibleField(Number.prototype, '__$asrtl_binary_shl', function(right){
	if(right === null || right === undefined) return this.valueOf();
	else if(isInteger(this) && (isBoolean(right) || isInteger(right))) return this << right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "shiftLeft", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("shiftLeft", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('shiftLeft');
	else return this.valueOf();
});

invisibleField(Number.prototype, '__$asrtl_binary_equ', function(right){
	if(right === null || right === undefined) return this.valueOf() === 0;
	else if(isBoolean(right) || isReal(right)) return this == right;
	else return false;
});

invisibleField(Number.prototype, '__$asrtl_binary_lte', function(right){
	if(right === null || right === undefined) return this <= 0;
	else if(isBoolean(right) || isReal(right)) return this <= right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "lessThanOrEqual", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("lessThanOrEqual", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('lessThanOrEqual');
	else return false;
});

invisibleField(Number.prototype, '__$asrtl_binary_gte', function(right){
	if(right === null || right === undefined) return this >= 0;
	else if(isBoolean(right) || isReal(right)) return this >= right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "greaterThanOrEqual", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("greaterThanOrEqual", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('greaterThanOrEqual');
	else return false;
});

invisibleField(Number.prototype, '__$asrtl_binary_gt', function(right){
	if(right === null || right === undefined) return this > 0;
	else if(isBoolean(right) || isReal(right)) return this > right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "greaterThan", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("greaterThan", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('greaterThan');
	else return false;
});

invisibleField(Number.prototype, '__$asrtl_binary_lt', function(right){
	if(right === null || right === undefined) return this < 0;
	else if(isBoolean(right) || isReal(right)) return this < right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "lessThan", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("lessThan", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('lessThan');
	else return false;
});

invisibleField(Number.prototype, '__$asrtl_typecast', function(right){
	switch(right){
		case String: return this.toString();
		case Number: return this.valueOf();
		case Boolean: return this.valueOf() !== 0;
		case undefined:
		case null: return 0;
		case RegExp: return new RegExp(this.toString());
		case $asyncscript.integer: return Math.round(this);
		case Function: return thisProvider.bind(this).toLambda(false);
		default:
			if(right instanceof Signature) return thisProvider.bind(this).toLambda(right);
			else if(type instanceof FilterContract) return type.typecast(this);
			else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('typecast');
			else return null;
	}
});

invisibleField(Number, "__$asrtl_implicit", Number);

invisibleField(String.prototype, '__$asrtl_binary_plus', function(right){
	if(right === null || right === undefined) return this.valueOf();
	else if(isBoolean(right) || isReal(right) || isString(right)) return this + right;
	else if(right.__$asrtl_insert instanceof Function) return right.__$asrtl_insert(this, 0);
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "binaryPlus", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("binaryPlus", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryPlus');
	else return this.valueOf();
});

invisibleField(String.prototype, '__$asrtl_binary_minus', function(right){
	if(right === null || right === undefined) return this.valueOf();
	else if(isString(right)) return this.replace(right, '');
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "binaryMinus", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("binaryMinus", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryMinus');
	else return this.valueOf();
});

invisibleField(String.prototype, '__$asrtl_binary_mul', function(right){
	if(right === null || right === undefined || right.valueOf() === 0) return '';
	else if(isInteger(right)){ //repeat string the specified number of times
		var result = this.valueOf();
		while(--right) result += this;
		return result;
	}
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "multiplication", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("multiplication", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('multiplication');
	else return this.valueOf();
});

invisibleField(String.prototype, "__$asrtl_binary_and", function(right){
	if(right === null || right === undefined) return this.valueOf();
	else if(isString(right)){
		var result = '';
		for(var i = 0; i < Math.min(this.length, right.length); i++){
			var a = this[i];
			if(a === right[i]) result += a;		
		}
		return result;
	}
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "and", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("and", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('XOR');
	else return this.valueOf();
});

invisibleField(String.prototype, '__$asrtl_binary_div', function(right){
	if(right === null || right === undefined || right.valueOf() === 0) return NaN;
	else if(right === 1) return this.valueOf();
	else if(isInteger(right)){
		var result = new Array();
		result.__$contract$__ = new ArrayContract(String);
		var subresult = '';
		for(var i = 0; i < this.length; i++){
			subresult += this[i];
			if((i + 1) % right === 0) {result.push(subresult); subresult = ''; }
		}
		return result;
	}
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "division", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("division", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('division');
	else return this.valueOf();
});

invisibleField(String.prototype, '__$asrtl_binary_shr', function(right){
	if(right === null || right === undefined || right.valueOf() === 0) return this.valueOf();
	else if(isString(right)) return this + right;	//insert the left string into the beginning of the right string
	else if(isInteger(right)) return this.substr(0, this.length - right);
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "shiftRight", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("shiftRight", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('shiftRight');
	else return this.valueOf();
});

invisibleField(String.prototype, '__$asrtl_binary_shl', function(right){
	if(right === null || right === undefined || right.valueOf() === 0) return this.valueOf();
	else if(isString(right)) return right + this;
	else if(isInteger(right)) return this.substr(right, this.length - right);
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "shiftLeft", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("shiftLeft", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('shiftLeft');
	else return this.valueOf();
});

invisibleField(String.prototype, '__$asrtl_binary_equ', function(right){
	if(right === null || right === undefined || right.valueOf() == 0) return this.length === 0;
	else if(isString(right)) return this.valueOf() === right;
	else return false;
});

invisibleField(String.prototype, '__$asrtl_binary_lte', function(right){
	if(right === null || right === undefined || right.valueOf() === 0) return this.length === 0;
	else if(isString(right)) return this <= right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "lessThanOrEqual", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("lessThanOrEqual", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('lessThanOrEqual');
	else return false;
});

invisibleField(String.prototype, '__$asrtl_binary_gte', function(right){
	if(right === null || right === undefined || right.valueOf() === 0) return true;
	else if(isString(right)) return this >= right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "greaterThanOrEqual", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("greaterThanOrEqual", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('greaterThanOrEqual');
	else return false;
});

invisibleField(String.prototype, '__$asrtl_binary_gt', function(right){
	if(right === null || right === undefined || right.valueOf() === 0) return this.length > 0;
	else if(isString(right)) return this > right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "greaterThan", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("greaterThan", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('greaterThan');
	else return false;
});

invisibleField(String.prototype, '__$asrtl_binary_lt', function(right){
	if(right === null || right === undefined || right === 0) return false;
	else if(isString(right)) return this < right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "lessThan", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("lessThan", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('lessThan');
	else return false;
});

invisibleField(String.prototype, '__$asrtl_contains', function(value){
	return isString(value) && this.indexOf(value) >= 0;
});

invisibleField(String.prototype, '__$asrtl_typecast', function(type){
	if(right === null || right === undefined) return '';
	switch(right){
		case String: return this.valueOf();
		case Boolean: return this.length > 0;
		case undefined:
		case null: return null;
		case RegExp: return new RegExp(this);
		case $asyncscript.integer: return parseInt(this, 10);
		case Function: return thisProvider.bind(this).toLambda(false);
		default:
			if(type instanceof Signature) return thisProvider.bind(this).toLambda(type);
			else if(type instanceof FilterContract) return type.typecast(this);
			else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("typecast");
			else return null;
	}
});

invisibleField(String, "__$asrtl_implicit", toScriptString);

invisibleField(RegExp.prototype, '__$asrtl_binary_plus', function(right){
	if(right === null || right === undefined) return this.valueOf();
	if(isBoolean(right) || isReal(right) || isString(right)) return new RegExp(this.source + right);
	else if(right instanceof RegExp) return new RegExp(this.source + right.source);
	else if(right.__$asrtl_insert instanceof Function) return right.__$asrtl_insert(this, 0);
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "binaryPlus", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("binaryPlus", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryPlus');
	else return this.valueOf();
});

invisibleField(RegExp.prototype, '__$asrtl_binary_minus', function(right){
	if(right === null || right === undefined) return this.valueOf();
	else if(isString(right)) return new RegExp(this.source.replace(right, ''));
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "binaryMinus", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("binaryMinus", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryMinus');
	else return this.valueOf();
});

invisibleField(RegExp.prototype, '__$asrtl_binary_equ', function(right){
	if(right === null || right === undefined) return this.source.length === 0;
	else if(right instanceof RegExp) return this.source === right.source;
	else return false;
});

invisibleField(RegExp.prototype, '__$asrtl_binary_lte', function(right){
	if(right === null || right === undefined) return this.source.length === 0;
	else if(right instanceof RegExp) return this.source <= right.source;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "lessThanOrEqual", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("lessThanOrEqual", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('lessThanOrEqual');
	else return false;
});

invisibleField(RegExp.prototype, '__$asrtl_binary_gte', function(right){
	if(right === null || right === undefined) return true;
	else if(right instanceof RegExp) return this.source >= right.source;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "greaterThanOrEqual", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("greaterThanOrEqual", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('greaterThanOrEqual');
	else return false;
});

invisibleField(RegExp.prototype, '__$asrtl_binary_gt', function(right){
	if(right === null || right === undefined) return this.source.length > 0;
	else if(right instanceof RegExp) return this.source > right.source;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "greaterThan", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("greaterThan", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('greaterThan');
	else return false;
});

invisibleField(RegExp.prototype, '__$asrtl_binary_lt', function(right){
	if(right === null || right === undefined) return false;
	else if(right instanceof RegExp) return this.source < right.source;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "lessThan", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("lessThan", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('lessThan');
	else return false;
});

invisibleField(RegExp.prototype, '__$asrtl_typecast', function(right){
	switch(right){
		case String: return this.source;
		case undefined:
		case null: return null;
		case RegExp: return this;
		case Number:
		case $asyncscript.integer: return hashCode(this.source);
		case Function: return thisProvider.bind(this).toLambda(false);
		default:
			if(right instanceof Signature) return thisProvider.bind(this).toLambda(false);
			else if(type instanceof FilterContract) return type.typecast(this);
			else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('typecast');
			else return null;
	}
});

invisibleField(RegExp, '__$asrtl_implicit', function(value){
	if(value === null || value === undefined) return value;
	else if(value instanceof RegExp) return value;
	else return new RegExp(toScriptString(value)); 
});

invisibleField(Array.prototype, "__$asrtl_typecast", function(right){
	switch(right){
		case String: return toScriptString(this);
		case undefined:
		case null: return null;
		case Number:
		case $asyncscript.integer: return this.length;
		case Function: return thisProvider.bind(this).toLambda(false);
		default:
			if(right instanceof ArrayContract)
				switch($asyncscript.relationship(this.__$contract$__, right)){
					case "equal": return this;
					case "subset": 
						var result = this.slice(0);
						Object.defineProperty(result, "__$contract$__", {value: new Vector(result.length, right.contract)});
						return result;
					default:
						result = this.map(function(v){
							return $asyncscript.typecast(v, this.contract);
						}, right);
						Object.defineProperty(result, "__$contract$__", {value: new Vector(result.length, right.contract)});
						return result;
				}
			else if(right instanceof Vector)
				switch($asyncscript.relationship(this.__$contract$__.contract, right.contract)){
					case "subset":					
					case "equal":
						var result;
						if(this.length === right.__$size$__) {
							result = result.slice(0);
							Object.defineProperty(result, "__$contract$__", {value: right});
							return result;
						}
						else if(this.length > right.__$size$__){
							result = result.slice(0, right.__$size$__ - 1);
							Object.defineProperty(result, "__$contract$__", {value: right});
							return result;
						}
						else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('typecast');
						else return null;
					default:
						if(this.length >= right.__$size$__){
							result = new Array(right.__$size$__);
							for(var i = 0; i < result.length; i++) result[i] = {value: this[i], contract: right.contract};	
							return container.create.apply(container, result);		
						}
				}
			else if(right.__$cc$__ && this.length >= right.__$size$__){
				result = new Array(right.__$size$__);
				for(var i = 0, entry; i < result.length; i++){
					entry = result[i] = {value: this[i], contract: right.__$contracts$__[i]};
					if(i in right) entry.name = right[i];
				}
				return container.create.apply(container, result);
			}
			else if(type instanceof Signature) return thisProvider.bind(this).toLambda(false);
			else if(type instanceof FilterContract) return type.typecast(this);
			if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('typecast');
			else return null;
	}
});

container.base.__$asrtl_typecast =  function(right){
	var operator = this['to'];
	if(operator) return $asyncscript.invoke(this, operator, [right]);
	else switch(right){
		case String: return toScriptString(this);
		case undefined:
		case null: return null;
		case Number:
		case $asyncscript.integer: return this.__$size$__;
		case Function: return thisProvider.bind(this).toLambda(false);
		default:
			var result;
			if(right instanceof ArrayContract){
				result = new Array(this.__$size$__);
				for(var i = 0, entry; i < result.length; i++){
					entry = result[i] = {value: this[i], contract: right.contract};
					if(i in this.__$contract$__) entry.name = this.__$contract$__[i];
				}
				return container.create.apply(container, result);
			}
			else if(right instanceof Vector && this.__$size$__ >= right.__$size$__){
				result = new Array(right.__$size$__);
				for(var i = 0, entry; i < result.length; i++){
					entry = result[i] = {value: this[i], contract: right.contract};
					if(i in this.__$contract$__) entry.name = this.__$contract$__[i];
				}	
				return container.create.apply(container, result);
			}
			else if(right.__$cc$__ && this.__$size$__ >= right.__$size$__){
				result = new Array(right.__$size$__);
				for(var i = 0, entry; i < result.length; i++){
					entry = result[i] = {value: this[i], contract: right.contract};
					if(i in right) entry.name = right[i];
					else if(i in this.__$contract$__) entry.name = this.__$contract$__[i];
				}	
				return container.create.apply(container, result);
			}
			else if(type instanceof Signature) return thisProvider.bind(this).toLambda(false);
			else if(type instanceof FilterContract) return type.typecast(this);
			if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('typecast');
			else return null;
	}
};

$asyncscript.integer.__$asrtl_implicit = function(value){ return Math.floor(new Number(value)); };

invisibleField(Object, Function, "__$asrtl_implicit",
$asyncscript.typedef.__$asrtl_implicit =
Complementation.prototype.__$asrtl_implicit = 
Union.prototype.__$asrtl_implicit =
Vector.prototype.__$asrtl_implicit =
ArrayContract.prototype.__$asrtl_implicit =
Expression.__$asrtl_implicit =
Signature.prototype.__$asrtl_implicit =
containerContract.base.__$asrtl_implicit = function(value){ return value; },
false);

invisibleField(Number, Object, String, Boolean, Function, RegExp, "__$asrtl_binary_xor", 
Signature.prototype.__$asrtl_binary_xor =
Vector.prototype.__$asrtl_binary_xor =
Union.prototype.__$asrtl_binary_xor = 
Complementation.prototype.__$asrtl_binary_xor =
FilterContract.prototype.__$asrtl_binary_xor =
ArrayContract.prototype.__$asrtl_binary_xor =
$asyncscript.integer.__$asrtl_binary_xor =
$asyncscript.typedef.__$asrtl_binary_xor =
containerContract.base.__$asrtl_binary_xor = function(right){
	if(right === null || right === undefined) return Object;
	else if(right.__$asrtl_relationship instanceof Function)
		return $asyncscript.or(
			$asyncscript.and(this, $asyncscript.negation(right)),
			$asyncscript.and($asyncscript.negation(this), right)
		);
	else if(isInteger(right))
		switch(right){
			case 0: return null;
			case 1: return this;
			default: return new Vector(this, right);
		}
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "xor", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("xor", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('XOR');
	else return this;
},
false);


invisibleField(Number, Object, String, Boolean, Function, RegExp, "__$asrtl_binary_mul", 
Signature.prototype.__$asrtl_binary_mul =
Vector.prototype.__$asrtl_binary_mul =
Union.prototype.__$asrtl_binary_mul = 
Complementation.prototype.__$asrtl_binary_mul =
ArrayContract.prototype.__$asrtl_binary_mul =
$asyncscript.integer.__$asrtl_binary_mul =
FilterContract.prototype.__$asrtl_binary_mul =
$asyncscript.typedef.__$asrtl_binary_mul =
containerContract.base.__$asrtl_binary_mul = function(right){
	if(right === null || right === undefined) return right;
	else if(right.__$asrtl_relationship) return containerContract.create({contract: this}, {contract: right});
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "multiplication", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("multiplication", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('multiplication');
	else return this;
},
false);

invisibleField(Number, String, Boolean, Function, RegExp, "__$asrtl_binary_or", 
Signature.prototype.__$asrtl_binary_or =
Vector.prototype.__$asrtl_binary_or =
Union.prototype.__$asrtl_binary_or = 
Complementation.prototype.__$asrtl_binary_or =
ArrayContract.prototype.__$asrtl_binary_or =
$asyncscript.integer.__$asrtl_binary_or =
FilterContract.prototype.__$asrtl_binary_or =
$asyncscript.typedef.__$asrtl_binary_or =
containerContract.base.__$asrtl_binary_or = function(right){
	if(right === null || right === undefined) return this;
	else if(right.__$asrtl_relationship) return Union.create(this, right);
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "or", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("or", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('OR');
	else return this;
},
false);

invisibleField(Object, '__$asrtl_binary_or', function(right){
	if(right === null || right === undefined) return null;
	else if(right.__$asrtl_relationship instanceof Function) return this;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "or", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("or", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('OR');
	else return this;
});

invisibleField(Number, String, Boolean, Function, RegExp, "__$asrtl_binary_and", 
Signature.prototype.__$asrtl_binary_and =
Vector.prototype.__$asrtl_binary_and =
Union.prototype.__$asrtl_binary_and = 
Complementation.prototype.__$asrtl_binary_and =
ArrayContract.prototype.__$asrtl_binary_and =
$asyncscript.integer.__$asrtl_binary_and =
FilterContract.prototype.__$asrtl_binary_and =
$asyncscript.typedef.__$asrtl_binary_and =
containerContract.base.__$asrtl_binary_and = function(right){
	if(right === null || right === undefined) return null;
	else if(right.__$asrtl_relationship)
		switch(this.__$asrtl_relationship(right)){
			case "subset":
			case "equal": return this;
			case "superset": return right;
			default: return null;
		}
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "and", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("and", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('AND');
	else return this;
},
false);
invisibleField(Object, '__$asrtl_binary_and', function(right){
	if(right === null || right === undefined) return null;
	else if(right.__$asrtl_relationship instanceof Function) return right;
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "and", this);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('AND');
	else return this;
});

invisibleField(Number, Object, String, Boolean, Function, RegExp, "__$asrtl_typecast", 
Signature.prototype.__$asrtl_typecast =
Vector.prototype.__$asrtl_typecast =
Union.prototype.__$asrtl_typecast = 
Complementation.prototype.__$asrtl_typecast =
ArrayContract.prototype.__$asrtl_typecast =
$asyncscript.integer.__$asrtl_typecast =
$asyncscript.typedef.__$asrtl_typecast =
FilterContract.prototype.__$asrtl_typecast =
containerContract.base.__$asrtl_typecast = function(right){
	switch(right){
		case Object:
		case $asyncscript.typedef: return this;
		case String: return toScriptString(this);
		default: 
			if(type instanceof FilterContract) return type.typecast(this);
			if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('typecast');
			else return this;
	}
},
false);

Vector.prototype.__$asrtl_binary_plus = function(right){
	if(right === null || right === undefined || right.valueOf() == 0) return this;
	else if(isInteger(right)) return new Vector(this.contract, this.__$size$__ + right);
	else if(right.__$asrtl_insert instanceof Function) return right.__$asrtl_insert(this, 0);
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "binaryPlus", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("binaryPlus", this);
	else if(type instanceof FilterContract) return type.typecast(this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryPlus');
	else return this;
};

Vector.prototype.__$asrtl_binary_minus = function(right){
	if(right === null || right === undefined || right.valueOf() == 0) return this;
	else if(isInteger(right))
		if(this.__$size$__ <= right) return null;
		else if(this.__$size$__ === right + 1) return this.contract;
		else if(this.__$size$__ > right) return new Vector(this.contract, this.__$size$__ - right);
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "binaryMinus", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("binaryMinus", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryMinus');
	else return this;
};

Signature.prototype.__$asrtl_binary_plus = function(right){
	if(right === null || right === undefined) return this;
	else if(right instanceof Signature) return this.composition(right);
	else if(right.__$asrtl_relationship instanceof Array) return this.__$asrtl_insert(right, this.__$size$__);
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "binaryPlus", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("binaryPlus", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryPlus');
	else return this;
};

Signature.prototype.__$asrtl_binary_shl = function(right){
	if(right === null || right === undefined || right.valueOf() === 0 || right.valueOf() === false) return this;
	else if(isInteger(right))
		if(right >= this.__$size$__) return Signature.empty;
		else{
			var result = new Signature();
			result.__$size$__ = this.__$size$__ - right;
			for(var i = right; i < this.__$size$__; i++) result[i - right] = this[i];
			return result;
		}
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "shiftLeft", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("shiftLeft", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('shiftLeft');
	else return this;
};

Signature.prototype.__$asrtl_binary_shr = function(right){
	if(right === null || right === undefined || right.valueOf() === 0 || right.valueOf() === false) return this;
	else if(isInteger(right))
		if(right >= this.__$size$__) return Signature.empty;
		else {
			var result = new Signature();
			result.__$size$__ = this.__$size$__ - right;
			for(var i = 0; i < result.__$size$__; i++) result[i] = this[i];
			return result;
		}
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "shiftRight", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("shiftRight", this);	
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('shiftRight');
	else return this;
};

invisibleField(Function.prototype, "__$asrtl_binary_plus", function(right){
	if(right === null || right === undefined) return this;
	if(right.__$contract$__ instanceof Signature) return new OverloadList(this, right);
	else if(right.__$asrtl_insert instanceof Function) return right.__$asrtl_insert(this, 0);
	else return createDelayedFunction(this, "binaryPlus", right);
});

invisibleField(Function.prototype, "__$asrtl_binary_minus", function(right){
	return delayRightOperand(this, "binaryMinus", right);
});

invisibleField(Function.prototype, "__$asrtl_binary_shr", function(right){
	return delayRightOperand(this, "shiftRight", right);
});


invisibleField(Function.prototype, "__$asrtl_binary_shl", function(right){
	return delayRightOperand(this, "shiftLeft", right);
});

invisibleField(Function.prototype, "__$asrtl_binary_div", function(right){
	return delayRightOperand(this, "division", right);
});

invisibleField(Function.prototype, "__$asrtl_binary_mod", function(right){
	return delayRightOperand(this, "modulo", right);
});

invisibleField(Function.prototype, "__$asrtl_binary_xor", function(right){
	return delayRightOperand(this, "xor", right);
});

invisibleField(Function.prototype, "__$asrtl_binary_or", function(right){
	return delayRightOperand(this, "or", right);
});

invisibleField(Function.prototype, "__$asrtl_binary_and", function(right){
	return delayRightOperand(this, "and", right);
});

invisibleField(Function.prototype, "__$asrtl_binary_lte", function(right){
	return delayRightOperand(this, "lessThanOrEqual", right);
});

invisibleField(Function.prototype, "__$asrtl_binary_lt", function(right){
	return delayRightOperand(this, "lessThan", right);
});

invisibleField(Function.prototype, "__$asrtl_binary_gt", function(right){
	return delayRightOperand(this, "greaterThan", right);
});

invisibleField(Function.prototype, "__$asrtl_binary_gte", function(right){
	return delayRightOperand(this, "greaterThanOrEqual", right);
});

invisibleField(Function.prototype, "__$asrtl_binary_mul", function(right){
	if(right instanceof Function && right.__$contract$__ instanceof Signature){	//composition of the two functions
		var left = this;		
		return function(){
			var destination = $asyncscript.enterLambdaBody();
			//call the first lambda
			var result = $asyncscript.invoke(null, left, arguments);
			//call the second lambda
			var flen = left.__$contract$__["__$size$__"],
			slen = right.__$contract$__["__$size$__"], 
			args = new Array(slen);	//arguments for the second lambda
			args[0] = result;	//the result from the first lambda passed as first argument for the second lambda
			for(var i = flen + 1; i < flen + slen; i++) args[i - flen] = arguments[i];
			$asyncscript.prepareLambdaInvocation(destination);
			return $asyncscript.invoke(null, right, args);
		}.toLambda(this.__$contract$__.composition(right.__$contract$__));
	}
	else return delayRightOperand(this, "multiplication", right);
});

invisibleField(Function.prototype, "__$asrtl_binary_equ", function(right){ return this === right; });

invisibleField(Function.prototype, "__$asrtl_typecast", function(right){ 
	switch(right){
		case Object:
		case Function: return this;
		case String: return this.toString();
		default:
			if(this.__$contract$__ instanceof Signature && right instanceof Signature)
				switch($asyncscript.relationship(this.__$contract$__, right)){
					case "equal": return this;
					case "subset": return this.toLambda(right);
				}
			else if(type instanceof FilterContract) return type.typecast(this);
			if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('typecast');
			else return false;
	}
});

OverloadList.prototype.__$asrtl_binary_plus = function(right){
	if(right.__$contract$__ instanceof Signature || right instanceof OverloadList) return new OverloadList(this, right);
	else if(right.__$asrtl_insert instanceof Function) return right.__$asrtl_insert(this, 0);
	else return this.delayRightOperand("binaryPlus", right);
};

OverloadList.prototype.__$asrtl_binary_minus = function(right){
	return this.delayRightOperand("binaryMinus", right);
};

OverloadList.prototype.__$asrtl_binary_mul = function(right){
	return this.delayRightOperand("multiplication", right);
};

OverloadList.prototype.__$asrtl_binary_div = function(right){
	return this.delayRightOperand("division", right);
};

OverloadList.prototype.__$asrtl_binary_and = function(right){
	return this.delayRightOperand("and", right);
};

OverloadList.prototype.__$asrtl_binary_or = function(right){
	return this.delayRightOperand("or", right);
};

OverloadList.prototype.__$asrtl_binary_xor = function(right){
	return this.delayRightOperand("xor", right);
};

OverloadList.prototype.__$asrtl_binary_shr = function(right){
	return this.delayRightOperand("shiftRight", right);
};

OverloadList.prototype.__$asrtl_binary_shl = function(right){
	return this.delayRightOperand("shiftLeft", right);
};

OverloadList.prototype.__$asrtl_binary_gte = function(right){
	return this.delayRightOperand("greaterThanOrEqual", right);
};

OverloadList.prototype.__$asrtl_binary_lte = function(right){
	return this.delayRightOperand("lessThanOrEqual", right);
};

OverloadList.prototype.__$asrtl_binary_lt = function(right){
	return this.delayRightOperand("lessThan", right);
};

OverloadList.prototype.__$asrtl_binary_gt = function(right){
	return this.delayRightOperand("greaterThan", right);
};

OverloadList.prototype.__$asrtl_binary_equ = function(right){
	return (right instanceof OverloadList && right.__$size$__ === this.__$size$__) ? 
	Object.keys(this).every(function(len){
			var fn = this[len];
			return Object.some(function(len){ return fn === this[len]; }, right);
		}, this):
	false;
};

OverloadList.prototype.__$asrtl_binary_typecast = function(right){
	switch(right){
		case null:
		case undefined: return null;
		case Function:
		case Object: return this;
		default:
			if(right instanceof Signature){
				var result, lengths = Object.keys(this);
				for(var i = 0; i < lengths; i++)
					switch($asyncscript.relationship(this[lengths[i]].__$contract$__, right)){
						case "equal": return this[lengths[i]];
						case "subset":
							if(!result) result = this[lengths[i]];
						default: continue;
					}
				if(result) return result;
			}
			else if(type instanceof FilterContract) return type.typecast(this);
			if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('typecast');
			else return false;	
	}
};

containerContract.base.__$asrtl_binary_shl = function(right){
	if(right === null || right === undefined || right.valueOf() == 0) return this;
	else if(isInteger(right))
		return right >= this.__$size$__ ? containerContract.empty:
			containerContract.clone(this, function(i){ return i >= right; });
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "shiftLeft", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("shiftLeft", this);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('shiftLeft');
	else return false;
};

containerContract.base.__$asrtl_binary_shr = function(right){
	if(right === null || right === undefined || right.valueOf() == 0) return this;
	else if(isInteger(right)) 
		return right >= this.__$size$__ ? containerContract.empty:
			containerContract.clone(this, function(i){ return i < this; }.bind(this.__$size$__ - right));
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "shiftRight", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("shiftRight", this);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('shiftLeft');
	else return false;
};

containerContract.base.__$asrtl_binary_plus = function(right){
	if(right === null || right === undefined) return null;
	else if(right.__$asrtl_relationship instanceof Function) return this.__$asrtl_insert(right, this.__$size$__);
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "binaryPlus", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("binaryPlus", this);
	else if(right.__$asrtl_insert instanceof Function) return right.__$asrtl_insert(this, 0);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryPlus');
	else return false;
};

containerContract.base.__$asrtl_binary_minus = function(right){
	if(right === null || right === undefined) return this;
	else if(isInteger(right)) return containerContract.clone(this, function(i){ return i !== this.valueOf(); }.bind(right));
	else if(isString(right)) return containerContract.clone(this, function(i, name){ return name !== this.valueOf(); }.bind(right));
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "binaryMinus", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("binaryMinus", this);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryMinus');
	else return false;
};

invisibleField(Array.prototype, "__$asrtl_binary_shl", function(right){
	if(right === null || right === undefined || right.valueOf() == 0) return this;
	else if(isInteger(right)){
		var result = this.slice(0);
		while(right--) result.shift();
		return result.length === 0 ? container.empty : result;
	}
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "shiftLeft", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("shiftLeft", this);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('shiftLeft');
	else return false;
});

container.base.__$asrtl_binary_shl = function(right){
	var operator = this["<<"];
	if(operator) return $asyncscript.invoke(this, operator, [right]);
	else if(right === null || right === undefined || right.valueOf() == 0) return this;
	else if(isInteger(right)) return container.clone(this, function(i){ return i > right; });
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('shiftLeft');
	else return false;
};

container.base.__$asrtl_binary_plus = function(right){
	var operator = this["binary+"];
	return operator ? $asyncscript.invoke(this, operator, [right]) : this.__$asrtl_insert(right, this.__$size$__);	
};

invisibleField(Array.prototype, "__$asrtl_binary_shr", function(right){
	if(right === null || right === undefined || right.valueOf() == 0 ) return this;
	else if(isInteger(right)){
		var array = this.slice(0);
		while(right--) array.pop();
		return array.length === 0 ? container.empty : array;
	}
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('shiftLeft');
	else return false;
});

container.base.__$asrtl_binary_shr = function(right){
	var operator = this[">>"];
	if(operator) return $asyncscript.invoke(this, operator, [right]);
	else if(right === null || right === undefined || right.valueOf() == 0) return this;
	else if(isInteger(right)) return container.clone(this, function(i){ return i < this}.bind(this.__$size$__ - right));
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "shiftRight", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("shiftRight", this);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('shiftLeft');
	else return false;
};

invisibleField(Array.prototype, "__$asrtl_binary_minus", function(right){
	if(right === null || right === undefined) return this;
	else if(isInteger(right)){
		var array = this.slice(0);
		array.splice(right, 1);
		return array.length === 0 ? container.empty : array;
	}
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryMinus');
	else return false; 
});

container.base.__$asrtl_binary_minus = function(right){
	var operator = this["binary-"];
	if(operator) return $asyncscript.invoke(this, operator, [right]);
	else if(right === null || right === undefined) return this;	
	else if(isInteger(right)) return container.clone(this, function(i){ return i !== this.valueOf(); }.bind(right));
	else if(isString(right)) return container.clone(this, function(i, name){ return name !== name.valueOf(); }.bind(right));
	else if(right instanceof Function && right.__$contract$__ instanceof Signature) return delayLeftOperand(right, "binaryMinus", this);
	else if(right instanceof OverloadList) return right.delayLeftOperand("binaryMinus", this);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('binaryMinus');
	else return false;
};

container.base.__$asrtl_binary_mod = function(right){
	var operator = this["%"];
	if(operator) return $asyncscript.invoke(this, operator, [right]);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('modulo');
	else return false;
};

container.base.__$asrtl_binary_div = function(right){
	var operator = this["/"];
	if(operator) return $asyncscript.invoke(this, operator, [right]);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp('division');
	else return false;
};

//unsupported binary functions
invisibleField(RegExp, Number, String, Boolean, Function, Object, "__$asrtl_binary_plus",
$asyncscript.integer.__$asrtl_binary_plus = 
$asyncscript.typedef.__$asrtl_binary_plus =
ArrayContract.prototype.__$asrtl_binary_plus =
Complementation.prototype.__$asrtl_binary_plus =
Union.prototype.__$asrtl_binary_plus = function(right){
	if(right.__$asrtl_insert) return right.__$asrtl_insert(this, 0);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("binaryPlus");
	else return this;
},
false);

invisibleField(RegExp, Number, String, Boolean, Function, Object, "__$asrtl_binary_minus", 
Signature.prototype.__$asrtl_binary_minus = 
$asyncscript.integer.__$asrtl_binary_minus = 
$asyncscript.typedef.__$asrtl_binary_minus =
ArrayContract.prototype.__$asrtl_binary_minus =
Complementation.prototype.__$asrtl_binary_minus =
Union.prototype.__$asrtl_binary_minus = function(){
	if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("binaryMinus");
	else return this;
},
false);

invisibleField(Array.prototype, RegExp.prototype, RegExp, Number, String, Boolean, Function, Object, "__$asrtl_binary_div",
containerContract.base.__$asrtl_binary_div =
Signature.prototype.__$asrtl_binary_div = 
$asyncscript.integer.__$asrtl_binary_div = 
$asyncscript.typedef.__$asrtl_binary_div =
ArrayContract.prototype.__$asrtl_binary_div =
Complementation.prototype.__$asrtl_binary_div =
Union.prototype.__$asrtl_binary_div = function(){
	if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("binaryMinus");
	else return this;
},
false);

invisibleField(RegExp.prototype, RegExp, Number, String, Boolean, Function, Object, "__$asrtl_binary_shl",
$asyncscript.integer.__$asrtl_binary_shl = 
$asyncscript.typedef.__$asrtl_binary_shl =
ArrayContract.prototype.__$asrtl_binary_shl =
Complementation.prototype.__$asrtl_binary_shl =
Union.prototype.__$asrtl_binary_shl =
Vector.prototype.__$asrtl_binary_shl = function(){
	if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("shiftLeft");
	else return this;
},
false);

invisibleField(RegExp.prototype, RegExp, Number, String, Boolean, Function, Object, "__$asrtl_binary_shr",
$asyncscript.integer.__$asrtl_binary_shr = 
$asyncscript.typedef.__$asrtl_binary_shr =
ArrayContract.prototype.__$asrtl_binary_shr =
Complementation.prototype.__$asrtl_binary_shr =
Union.prototype.__$asrtl_binary_shr =
Vector.prototype.__$asrtl_binary_shr = function(){
	if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("shiftRight");
	else return this;
},
false);
 
invisibleField(Array.prototype, String.prototype, RegExp.prototype, RegExp, Number, String, Boolean, Function, Object, "__$asrtl_binary_mod",
containerContract.base.__$asrtl_binary_mod = 
Signature.prototype.__$asrtl_binary_mod = 
$asyncscript.integer.__$asrtl_binary_mod = 
$asyncscript.typedef.__$asrtl_binary_mod =
ArrayContract.prototype.__$asrtl_binary_mod =
Complementation.prototype.__$asrtl_binary_mod =
Union.prototype.__$asrtl_binary_mod =
Vector.prototype.__$asrtl_binary_mod =
OverloadList.prototype.__$asrtl_binary_mod = function(){
	if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("modulo");
	else return this;
},
false);

container.base.__$asrtl_binary_or = function(right){
	var operator = this["|"];
	if(operator) return $asyncscript.invoke(this, operator, [right]);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("OR");
	else return this;
};

invisibleField(String.prototype, RegExp.prototype, Array.prototype, "__$asrtl_binary_or",
function(){
	if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("or");
	else return this;
},
false);

container.base.__$asrtl_binary_and = function(right){
	var operator = this["&"];
	if(operator) return $asyncscript.invoke(this, operator, [right]);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("AND");
	else return this;
};

invisibleField(RegExp.prototype, Array.prototype, "__$asrtl_binary_and",
function(){
	if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("and");
	else return this;
},
false);

container.base.__$asrtl_binary_xor = function(right){
	var operator = this["^"];
	if(operator) return $asyncscript.invoke(this, operator, [right]);
	else if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("XOR");
	else return this;
};

invisibleProperty(String.prototype, RegExp.prototype, Array.prototype, "__$asrtl_binary_xor",
function(){
	if($asyncscript.state.checked) throw runtimeErrors.unsupportedOp("xor");
	else return this;
}, false);

//MEMBER ACCESS

/**
 * Returns value of the object member.
 * @param {Object} obj An object that encapsulates the specified member.
 * @param {String} member The name of the member.
 * @return {Object} A value of the member.
 */
$asyncscript.getMember = function(obj, member){
	if(member === "set") console.log("HERE %s", typeof obj);
	if(obj === undefined || obj === null)
		if(this.state.checked) throw runtimeErrors.voidref;
		else return null;
	else if(obj.__$asrtl_getmember) return obj.__$asrtl_getmember(member);
	else if(obj instanceof Promise)
		if(obj.isError) throw obj.result;
		else if(obj.isCompleted) return this.getMember(obj.result, member);
		else return this.fork(function(obj){ return $asyncscript.getMember(obj, member); }, obj);
	else if(obj[member]) return obj[member]; 
	else if(this.state.checked) throw runtimeErrors.missingMember; 
	else return null;
};

FilterContract.prototype.__$asrtl_getmember = function(member){
	switch(member){
		case "predicate": return this.predicate;
		case "base": return this.contract;
	}
	if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return null;
};

var exprMethods = {
	isQuoted: newLambda(function(expr){ return (expr && expr.tree.quoted) ? true : false }, false, Expression),
	//gets position of the node	
	positionOf: newLambda(function(expr){
		return expr ? container.create({
			name: "column", 
			value: expr.tree.position.column, 
			contract: $asyncscript.integer},
			{name: "line",
			value: expr.tree.position.line,
			contract: $asyncscript.integer
			}) : null;
	}, false, Expression),
	//visit each node in-to-deep	
	visit: newLambda(function(expr, visitor){
		var ast = require('./ast.js');
		return expr ? (ast.visit(expr.tree, function(tree){ tree = new Expression(tree, true); $asyncscript.invoke(undefined, visitor, [tree]); }), true) : false;
	}, false, Expression, new Signature(Expression)),
	//compiles the node
	compile: newLambda(function(expr){
		var compiler = require('./compiler.js'), ParserError = require('./ParserError.js'), result = new $asyncscript.Promise();
		if(expr) compiler.translate(expr.tree, false, true, function(err, compiled){
				if(err) result.fault(err);
				else try{
					compiled = new Function("return " + compiled);
					result.success(compiled());
				}catch(e){
					result.fault(e);
				}
			});
		else result.success(null);
		return result;
	}, false, Expression),
	//returns node at the specified placeholder
	nodeAt: newLambda(function(expr, index){
		var ast = require('./ast.js'), CodePlaceholderExpression = ast.CodePlaceholderExpression;
		expr = expr ? ast.findOne(expr.tree, function(expr){
			return expr instanceof CodePlaceholderExpression && expr.index === index; 
		}) : null;
		return expr && new Expression(expr, true);
	}, false, Expression, $asyncscript.integer)
};

$asyncscript.expand = function(expr, indicies){
	if(expr === null || expr === undefined) return null;
	else if(expr instanceof Property) return this.expand(expr.value, indicies);
	else if(expr instanceof Promise)
		if(expr.isError) throw expr.result;
		else if(expr.isCompleted) return this.expand(expr.result, indicies);
		else return this.fork(function(expr){ return $asyncscript.expand(expr, indicies, 0); }, expr);
	for(var i = arguments[2] || 0, idx; i < indicies; i++){
		idx = indicies[i];
		if(idx instanceof Property) {indicies[i] = idx.value; return this.expand(expr, indicies, i); }
		else if(idx instanceof Promise)
			if(idx.isError) throw idx.result;
			else if(idx.isCompleted) {indicies[i] = idx.result; return this.expand(expr, indicies, i); }
			else return this.fork(function(a){ this.indicies[this.index] = a; return $asyncscript.expand(this.expr, this.indicies, this.index); }.bind({index: i, indicies: indicies, expr: expr}), idx);
		else indicies[i] = Expression.convert(idx); 
	}
	//add expressions to placeholders
	var ast = require('./ast.js');
	var result = ast.visit(expr.tree, function(expr){
		if(expr.nodeType === "CodePlaceholderExpression" && expr.index < this.length){ 
			var result = new ast.CodePlaceholderExpression(expr.index, expr.position.column, expr.position.line);
			result['default'] = this[expr.index];
			return result;
		}
	}.bind(indicies));
};

//load expression contract methods
[
	{name: "isBinary", nodeType: "CodeBinaryExpression"},
	{name: "isUnary", nodeType: "CodeUnaryExpression"},
	{name: "isBoolean", nodeType: "CodeBooleanExpression"},
	{name: "isBuiltInContract", nodeType: "CodeBuiltInContractExpression"},	
	{name: "isIdentifier", nodeType: "CodeBinaryExpression"},
	{name: "isInteger", nodeType: "CodeIntegerExpression"},
	{name: "isDecl", nodeType: "CodeLetExpression"},
	{name: "isReal", nodeType: "CodeRealExpression"},
	{name: "isString", nodeType: "CodeStringExpression"},
	{name: "isRegExp", nodeType: "CodeRegulaExpression"},
	{name: "isInvocation", nodeType: "CodeInvocationExpression"},
	{name: "isFork", nodeType: "CodeForkExpression"},
	{name: "isAwait", nodeType: "CodeStringExpression"},
	{name: "isArray", nodeType: "CodeArrayExpression"},
	{name: "isArrayContract", nodeType: "CodeArrayContractExpression"},
	{name: "isIndexer", nodeType: "CodeIndexerExpression"},
	{name: "isConditional", nodeType: "CodeConditionalExpression"},
	{name: "isSeh", nodeType: "CodeSehExpression"},
	{name: "isSelector", nodeType: "CodeSwitcherExpression"},
	{name: "isContext", nodeType: "CodeContextExpression"},
	{name: "isContainerContract", nodeType: "CodeContainerContractExpression"},
	{name: "isContinue", nodeType: "CodeContinueExpression"},
	{name: "isBreak", nodeType: "CodeBreakExpression"},
	{name: "isReturn", nodeType: "CodeReturnExpression"},
	{name: "isFault", nodeType: "CodeFaultExpression"},
	{name: "isPlaceholder", nodeType: "CodePlaceholderExpression"},
	{name: "isRecursiveRef", nodeType: "CodeCurrentFunctionExpression"},
	{name: "isFunction", nodeType: "CodeFunctionExpression"},
	{name: "isRepeat", nodeType: "CodeRepeatExpression"},
	{name: "isFor", nodeType: "CodeForExpression"},
	{name: "isExpansion", nodeType: "CodeExpansionExpression"},
	{name: "isAsync", nodeType: "CodeAsyncExpression"},
	{name: "isBreak", nodeType: "CodeBreakExpression"},
	{name: "isScope", nodeType: "CodeScopeExpression"},
	{name: "isBreakpoint", nodeType: "CodeBreakpointExpression"},
	{name: "isAny", nodeType: "CodeAnyValueExpression"},
].forEach(function(e){
	this[e.name] = newLambda(new Function("expr", "return expr && expr.tree.nodeType === " + JSON.stringify(e.nodeType)), false, Expression);
}, exprMethods);

Expression.__$asrtl_getmember = function(member){
	if(member = exprMethods[member]) return member;
	else if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return null;
};

Expression.prototype.__$asrtl_getmember = function(member){
	member = Expression.convert(member, this.tree.position);
	var ast = require('./ast.js');
	return new Expression(new ast.CodeBinaryExpression(this.tree, ast.parseOperator('.', true), member, this.tree.position.column, this.tree.position.line), true);
};

var typedefMethods = {
	isFilter: newLambda(function(contract){ return contract instanceof FilterContract; }, false, Object),
	filter: newLambda(function(filter){
		return filter && filter.__$contract$__ instanceof Signature && filter.__$contract$__["__$size$__"] > 0 ?
			new FilterContract(filter.__$contract$__[0], filter.toLambda(false, Object)) : null;
	}, false, Function)
};

$asyncscript.typedef.__$asrtl_getmember = function(member){
	if(member = typedefMethods[member]) return member;
	else if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return null;
};

invisibleField(Boolean.prototype, Boolean, Function, String, RegExp, Object, "__$asrtl_getmember",
ArrayContract.prototype.__$asrtl_getmember =
Complementation.prototype.__$asrtl_getmember =
OverloadList.prototype.__$asrtl_getmember =
Vector.prototype.__$asrtl_getmember = 
$asyncscript.integer.__$asrtl_getmember = function(member){
	if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return null;
},
false);

invisibleField(Number.prototype, "__$asrtl_getmember", function(member){
	if(member === undefined || member === null) return this.__$asrtl_getmember("");
	//real properties
	if(isReal(this))
		switch(member.valueOf()){
			case "int": return Math.round(this);
			case "ceil": return Math.ceil(this);
			case "floor": return Math.floor(this);
			case "notnum": return isNaN(this);
		}
	//common properties
	switch(member){
		case "abs": return Math.abs(this);
		case "sqrt": return Math.sqrt(this);
		case "exp": return Math.exp(this);
		case "square": return this * this;
	}
	if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return null;
}, false);

function createIterator(state){
	var result = {
		__proto__: container.base,
		next: newLambda(function(){
			return state.moveNext() ? createIterator(state): null;
		}, false),
		current: state.current,
	};
	Object.defineProperty(result, "__$contract$__", {value: {
			__proto__: containerContract.base,
			0: "next",
			next: 0,
			1: "current",
			current: 1,
			__$contracts$__: {0: Signature.empty, 1: Object},
			__$size$__: 2
	}, enumerable: false, configurable: false});
	Object.defineProperty(result, "__$size$__", {get: container.sizeAccessor, enumerable: false, configurable: false});
	Object.defineProperty(result, 0, {get: container.byNameAccessor("next"), configurable: false, enumerable: false});
	Object.defineProperty(result, 1, {get: container.byNameAccessor("current"), configurable: false, enumerable: false});
	return result;
}

var arrayMethods = {
	iterator: newLambda(function(){
		return this.length === 0 ? null : createIterator({
			index: 0,
			array: this,
			get current(){ return this.array[this.index]; },
			moveNext: function(){ 
				if(this.index < this.array.length - 1) { this.index += 1; return true; }
				else return false;
			}
		});
	}, false),
	reverse: newLambda(function(){
		var result = this.reverse();
		result.__$contract$__ = this.__$contract$__;
		return result;
	}, false),
};

invisibleField(Array.prototype, "__$asrtl_getmember", function(member){
	if(member = arrayMethods[member]) return member;
	else switch(member){
		case "length": return this.length;
		case "last": return this.length ? this[this.length - 1] : null;
		case "first": return this.length ? this[0] : null;
	}
	if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return null;
});

var stringMethods = {
	indexOf: newLambda(String.prototype.indexOf, false, String),
	substr: newLambda(String.prototype.substr, false, $asyncscript.integer, $asyncscript.integer),
	cmp: newLambda(String.prototype.localeCompare, false, String),
	toUpper: newLambda(function(locale){
		return this[locale ? "toLocaleUpperCase" : "toUpperCase"]();
	}, false, Boolean),
	toLower: newLambda(function(locale){
		return this[locale ? "toLocaleLowerCase" : "toLowerCase"]();
	}, false, Boolean),
	charCode: newLambda(String.prototype.charCodeAt, false, $asyncscript.integer)
};

invisibleField(String.prototype, "__$asrtl_getmember", function(member){
	switch(member){
		case "length": return this.length;
		case "empty": return this.length === 0;
		default: if(member = stringMethods[member]) return member;
	}
	if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return null;
}, false);

var lambdaMethods = {
	call: newLambda(function(_this, args){
		if(args === null || args === undefined) return this.call(_this);
		else if(args instanceof Array) return this.apply(_this, args);
		var a = new Array(args.__$size$__);
		for(var i = 0; i < a.length; i++) a[i] = args[i];
		return this.apply(_this, a);
	}, false, Object, containerContract.empty, Object),
	bind: newLambda(function(target){
		if(target instanceof Property) return arguments.callee.call(this, target);
		else if(this.wrapped) return arguments.callee.call(this.wrapped, target);
		else return wrapLambda(this, this.__$contract$__, target);
	}, false, Object)
};

invisibleField(Function.prototype, "__$asrtl_getmember", function(member){
	if(member = lambdaMethods[member]) return member;	
	else if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return null;
}, false);

var regexpMethods = {
	test: newLambda(RegExp.prototype.test, false, String)
};

invisibleField(RegExp.prototype, "__$asrtl_getmember", function(member){
	switch(member){
		case "length": return this.source.length;
		case "i": return this.ignoreCase;
		case "g": return this.global;
		case "m": return this.multiline;
		default: if(member = regexpMethods[member]) return member;
	}	
	if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return null;
}, false);

var signatureMethods = {
	iterator: newLambda(function(){
		return this.__$size$__ ? createIterator({
			signature: this,
			position: 0,
			get current(){ return this.signature[this.position]; },
			moveNext: function(){
				if(this.position < this.signature.__$size$__ - 1){
					this.position += 1;
					return true;
				}
				else return false;
			}
		}) : null;
	}, false)
};

Signature.prototype.__$asrtl_getmember = function(member){
	if(member in signatureMethods) return signatureMethods[member];
	else switch(member){
		case "length": return this.__$size$__;
		case "empty": return this.__$size$__ === 0;
	}
	if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return null;
};

/**
 * Returns a contract binding of the container element.
 * @param {Object} An object that represents member name.
 * @return {Object} A contract binding of the member.
 */
containerContract.base.__$asrtl_getmember = function(member){
	if(member = this.__$contracts$__[this[member]]) return member;
	else if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return null;
};

container.base.__$asrtl_getmember = function(member){
	var value = this[member], operator;
	if(value !== undefined)
		return value instanceof Property && value.draft ? value.bind(this) : value;
	else if(operator = this["get."]) return $asyncscript.invoke(this, operator, [member]);
	else if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return null;
};

OverloadList.prototype.__$asrtl_getmember = 
Signature.prototype.__$asrtl_getmember = function(member){
	switch(member){
		case "length": return this.__$size$__; 	
	}
	if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return null;
};

/**
 * Overwrites member.
 * @param {Object} obj An object that contains the specified member.
 * @param {Object} member An object that represents a member.
 * @param {Object} value A new member value.
 */
$asyncscript.overwrite = function(obj, member, value){
	if(obj === null || obj === undefined) throw runtimeErrors.voidref;
	else if(obj instanceof Promise)
		if(obj.isError) throw obj.result;
		else if(obj.isCompleted) return this.overwrite(obj.result, member, value);
		else return this.fork(function(obj){ return $asyncscript.overwrite(obj, member, value); }, obj);
	else if(value instanceof Promise)
		if(value.isError) throw value.result;
		else if(value.isCompleted) return this.overwrite(obj, member, value.result);
		else return this.fork(function(value){ return $asyncscript.overwrite(obj, member, value); }, obj);
	else if(obj.__$asrtl_overwrite instanceof Function) return obj.__$asrtl_overwrite(member, value);
	else return obj;
};

/**
 * Writes value to the member.
 * @param {Object} obj An object that contains the specified member.
 * @param {Object} member An object that represents a member.
 * @param {Object} value A new member value.
 */
$asyncscript.setMember = function(obj, member, value){
	if(obj === null || obj === undefined) throw runtimeErrors.voidref;
	else if(obj instanceof Promise)
		if(obj.isError) throw obj.result;
		else if(obj.isCompleted) return this.setMember(obj.result, member, value);
		else return this.fork(function(obj){ return $asyncscript.setMember(obj, member, value); }, obj);
	else if(value instanceof Promise)
		if(value.isError) throw value.result;
		else if(value.isCompleted) return this.setMember(obj, member, value.result);
		else return this.fork(function(value){ return $asyncscript.setMember(obj, member, value); }, obj);
	else if(obj.__$asrtl_setmember instanceof Function) return obj.__$asrtl_setmember(member, value);
	else return obj[member] = value;
};

Expression.prototype.__$asrtl_setmember = function(member, value){
	member = Expression.convert(member, this.tree.position);
	value = Expression.convert(value, this.tree.position);	
	var ast = require('./ast.js');
	return new Expression(new ast.CodeBinaryExpression(
		new ast.CodeBinaryExpression(this.tree, ast.parseOperator('.', true), member, this.tree.position.column, this.tree.position.line),
		ast.parseOperator('=', true),
		value,
		this.tree.position.column, this.tree.position.line), true);
};

Property.prototype.__$asrtl_setmember = function(member, value){ $asyncscript.setMember(this.value, member, value); };

containerContract.base.__$asrtl_overwrite = function(member, value){
	if(value instanceof Property) return this.__$asrtl_overwrite(member, value.value);
	//can only set the contract
	else if(value && value.__$asrtl_relationship)
		if(member in value){
			var result = {__proto__: this.__proto__, __$size$__: this.__$size$__, __$contracts$__: {}};
			for(var i = 0; i < this.__$size$__; i++){
				containerContract.putNameIndexMap(i, this[i], result);
				result.__$contracts$__[i] = member === name || member === i ? value : this.__$contracts$__[i]; 
			}
			return result;
		}
		else return this;
	else throw runtimeErrors.contractExpected;
};

container.base.__$asrtl_overwrite = function(member, value){
	if(member in this){
		//binds value
		value = $asyncscript.binding(value, this.__$contract$__["__$contracts$__"][member]);
		//clone container
		var properties = {
			__$size$__: {configurable: false, enumerable: false, get: container.sizeAccessor},
			__$contract$__: {configurable: false, enumerable: false, writable: false, value: this.__$contract$__}	
		};
		for(var i = 0; i < this.__$size$__; i++){
			var name = this.__$contract$__[i];
			if(name){
				properties[i] = {configurable: false, enumerable: false, get: container.byNameAccessor(name)};
				properties[name] = {configurable: false, enumerable: false, writable: false, value: name === member ? value : this[i]};
			}
			else properties[i] = {configurable: false, enumerable: false, writable: false, value: this[i]};		
		}
		return Object.create(container.base, properties); 
	}
	else return this;
};

container.base.__$asrtl_setmember = function(member, value){
	var operator;
	if(value instanceof Property) return this.__$asrtl_setmember(member, value.value);
	//field exists
	else if(member in this){
		operator = this[member];
		value = $asyncscript.binding(value, this.__$contract$__["__$contracts$__"][member]);
		if(member instanceof Property){
			if(member.draft) (member = member.bind(this)).value = value;
			else member.value = value;
			return member;
		}
		else return value; 
	}
	//field doesn't exist
	else if(operator = this["set."]) return $asyncscript.invoke(this, operator, [member, value]);
	else if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return value;
};

invisibleField(Function.prototype, "__$asrtl_setmember", function(member, value){
	if($asyncscript.state.checked) throw runtimeErrors.missingMember;
	else return null;
}, false);

$asyncscript.getItem = function(obj, indicies, destination){
	if(destination === undefined) destination = new this.Promise();
	else if(obj === null || obj === undefined) throw $asyncscript.errors.voidref;
	else if(obj instanceof Promise)
		if(obj.isError){ this.fault(obj.result, destination); throw obj.result; }
		else if(obj.isCompleted) obj = obj.result;
		else return (this.fork(function(obj){ return $asyncscript.getItem(obj, indicies, destination); }, obj), destination);
	var result;
	//returns an item
	if(obj.__$asrtl_getitem instanceof Function){
		//synchronize arguments
		for(var i = arguments[3] || 0, idx; i < indicies.length; i++)
			if((idx = indicies[i]) instanceof Property) indicies[i] = idx.value;
			else if(idx instanceof Promise)
				if(idx.isError) { this.fault(idx.result, destination); throw idx.result; }
				else if(idx.isCompleted) indicies[i] = idx.result;
				else return this.fork(function(idx){
					this.indicies[this.index] = idx;
					return $asyncscript.getItem(obj, this.indicies, destination, this.index);
				}.bind({"indicies": indicies, "index": i}), idx), destination;
		this.destination = destination;
		result = obj.__$asrtl_getitem.apply(obj, indicies);
		delete this.destination;
	}	
	else if($asyncscript.state.checked) {
		$asyncscript.fault(runtimeErrors.unsupportedOp("[]"), destination);
		throw runtimeErrors.unsupportedOp("[]");
	}
	else result = null;
	$asyncscript[result instanceof Error ? "fault" : "ret"](result, destination);
	//finalize the indexer
	if(destination instanceof Promise)
		if(destination.isError) throw destination.result;
		else if(destination.isCompleted) return destination.result;
	return destination;
};

Expression.prototype.__$asrtl_getitem = function(){
	var ast = require('./ast.js'), result = new ast.CodeIndexerExpression(this.tree, [], this.tree.position.column, this.tree.position.line);
	for(var i = 0, element; i < arguments.length; i++)
		result.indicies.push(Expression.convert(arguments[i]));
	return new Expression(result, true);
};

Property.prototype.__$asrtl_getitem = function(){ return $asyncscript.getItem(this.value, arguments); };

/**
 * Provides an assigment logic.
 */
$asyncscript.assignment = function(left, right){
	if(left instanceof Property)
		return right instanceof Property ? this.assignment(left, right.value) : (left.value = right, left);
	else return right;
};

$asyncscript.setItem = function(obj, value, indicies, destination){
	if(obj === null || obj === undefined) throw $asyncscript.errors.voidref;
	var result;
	if(obj instanceof Promise)
		if(obj.isError){ this.fault(obj.result, destination); throw obj.result; }
		else if(obj.isCompleted) obj = obj.result;
		else return (this.fork(function(obj){ return $asyncscript.setItem(obj, value, indicies, destination); }, obj), destination);
	if(value instanceof Promise)
		if(value.isError) { this.fault(value.result, destination); throw value.result; }
		else if(value.isCompleted) value = value.result;
		else return (this.fork(function(value){ return $asyncscript.setItem(obj, value, indicies, destination); }, value), destination);
	//returns an item
	if(obj.__$asrtl_getitem instanceof Function) {
		//synchronize arguments
		for(var i = arguments[4] || 0, idx; i < indicies.length; i++)
			if((idx = indicies[i]) instanceof Property) indicies[i] = idx.value;
			else if(idx instanceof Promise)
				if(idx.isError) { this.fault(idx.result, destination); throw idx.result; }
				else if(idx.isCompleted) indicies[i] = idx.result;
				else return this.fork(function(idx){
					this.indicies[this.index] = idx;
					return $asyncscript.setItem(obj, value, this.indicies, destination, this.index);
				}.bind({"indicies": indicies, "index": i}), idx), destination;
		//save destination for the nested lambda call
		this.destination = destination;
		result = obj.__$asrtl_setitem.call(obj, value, indicies);
		delete this.destination;
	}
	else if($asyncscript.state.checked) {
		$asyncscript.fault(runtimeErrors.unsupportedOp("[]"), destination);
		throw runtimeErrors.unsupportedOp("[]");
	}
	else result = null;
	$asyncscript[result instanceof Error ? "fault" : "ret"](result, destination);
	//finalize the indexer
	if(destination instanceof Promise)
		if(destination.isError) throw destination.result;
		else if(destination.isCompleted) return destination.result;
	return destination;
};

Expression.prototype.__$asrtl_setitem = function(value, indicies){
	value = Expression.convert(value);
	var ast = require('./ast.js'), result = new ast.CodeIndexerExpression(this.tree, [], this.tree.position.column, this.tree.position.line);
	for(var i = 0, element; i < indicies.length; i++)
		result.indicies.push(Expression.convert(indicies[i]));
	result = new ast.CodeBinaryExpression(result, ast.parseOperator("=", true), value, this.tree.position.column, this.tree.position.line);
	return new Expression(result, true);
};

Property.prototype.__$asrtl_setitem  = function(value, indicies){ $asyncscript.setItem(this.value, value, indicies, $asyncscript.destination); };

invisibleField(Array.prototype, "__$asrtl_getitem", 
Signature.prototype.__$asrtl_getitem = 
OverloadList.prototype.__$asrtl_getitem =
function(idx){
	if(isInteger(idx)){
		var lengths = Object.keys(this);
		for(var i = 0; i < this.__$size$__; i++)
			if(i == idx) return this[lengths[i]];
		return null;
	}
	else if($asyncscript.state.checked) return $asyncscript.errors.unsupportedOp("[]");
	else return null;
});

invisibleField(Array.prototype, "__$asrtl_setitem", function(value, index){
	if(isInteger(index = index[0])){
		var result = this.slice(0);
		result[index] = $asyncscript.binding(value, (result.__$contract$__ = this.__$contract$__).contract);
		return result;
	}
	else if($asyncscript.state.checked) return $asyncscript.errors.unsupportedOp("[]");
	else return this;
});

Signature.prototype.__$asrtl_setitem = function(value, index){
	index = index[0];
	if(value === null || (value && value.__$asrtl_relationship instanceof Function)){
		var result = new Signature();
		for(var i = 0; i < (result.__$size$__ = this.__$size$__); i++)
			result[i] = i === index ? value : this[i];
		return result;
	}
	else if($asyncscript.state.checked) return $asyncscript.errors.unsupportedOp("[]");
	else return this;
};

Signature.prototype.__$asrtl_getitem = function(index){
	if(isInteger(index)) return this[index];
	else if($asyncscript.state.checked) return $asyncscript.errors.unsupportedOp("[]");
	else return this;
};

invisibleField(Function.prototype, "__$asrtl_getitem", function(){
	var fn = this, indicies = arguments, result = function(){
		return $asyncscript.getItem(fn.apply(this, arguments), indicies, $asyncscript.enterLambdaBody());
	};
	return this.__$contract$__ instanceof Signature ? result.toLambda(this.__$contract$__) : result;
});

invisibleField(Function.prototype, "__$asrtl_setitem", function(value, indicies){
	var fn = this,
	result = function(){
		return $asyncscript.setItem(fn.apply(this, arguments), value, indicies, $asyncscript.enterLambdaBody());
	};
	return this.__$contract$__ instanceof Signature ? result.toLambda(this.__$contract$__) : result;
});

containerContract.base.__$asrtl_getitem = function(index){ return this.__$contracts$__[index]; };

containerContract.base.__$asrtl_setitem = function(value, index){
	index = index[0];
	if(value === null || (value && value.__$asrtl_relationship instanceof Function)){
		var result = {
			__$size$__: this.__$size$__,
			__$contracts$__: {},
			__proto__: this.__proto__		
		};
		for(var i = 0; i < this.__$size$__; i++){
			containerContract.putNameIndexMap(i, this[i], result);
			result.__$contracts$__[i] = i === index ? value : this.__$contracts$__[i];
		}
		return result;
	}
	else if($asyncscript.state.checked) return $asyncscript.errors.unsupportedOp("[]");
	else return this;
};

container.base.__$asrtl_getitem = function(index){
	var operator;
	if(operator = this["get[]"]) return $asyncscript.invoke(this, operator, arguments, $asyncscript.enterLambdaBody());
	else if(arguments.length === 1 && (index in this)) return this[index];
	else if($asyncscript.state.checked) return $asyncscript.errors.unsupportedOp("[]");
	else return null;
};

container.base.__$asrtl_setitem = function(value, index){
	var operator;
	if(operator = this["set[]"]) return $asyncscript.invoke(this, value, index, $asyncscript.enterLambdaBody());
	else if(index.length === 1 && (index[0] in this)){
		index = index[0];
		var properties = {
			__$size$__: {get: container.sizeAccessor, enumerable: false, configurable: false},
			__$contract$__: {value: this.__$contract$__, writable: false, configurable: false, enumerable: false},
		};
		for(var i = 0; i < this.__$size$__; i++){
			var contract = this.__$contract__$["__$contracts$__"][i],
			name = this.__$contract$__[i];
			if(name){
				properties[i] = {get: container.byNameAccessor(name), enumerable: false, configurable: false};
				properties[name] = {value: i === index ? $asyncscript.binding(value, contract) : this[i], configurable: false, writable: false};
			}
			else properties[i] = {value: i === index ? $asyncscript.binding(value, contract) : this[i], writable: false, configurable: false, enumerable: false};	
		}
		return Object.create(container.base, properties);
	}
	else if($asyncscript.state.checked) return $asyncscript.errors.unsupportedOp("[]");
	else return this;
};

//CONTRACT BINDING

$asyncscript.contractOf = function(value){
	if(value === null || value == undefined) return null;
	else if(value instanceof Function) return value.__$contract$__ || Function;
	else if(value = value.__$contract$__) return value;
	else return Object;
};

$asyncscript.binding = function(value, contract){
	if(contract === undefined || contract === Object) return value;
	else if(contract instanceof Promise)
		if(contract.isError) throw contract.result;
		else if(contract.isCompleted) return this.binding(value, contract.result);
		else return this.fork(function(contract){ return $asyncscript.binding(value, contract); }, contract);
	else if(value instanceof Promise)
		if(value.isError) throw value.result;
		else if(value.isCompleted) return this.binding(value.result, contract);
		else return value.setContract(contract);
	else if(value === null) return this.invoke(undefined, contract, []);	//default value
	else if(contract.__$asrtl_relationship instanceof Function)
		switch(this.relationship(contract, this.contractOf(value))){
			case "different":
			case "subset": value = undefined; break;
			case "superset": value = value instanceof Property ? value : contract.__$asrtl_implicit(value); break;
	}
	else throw runtimeErrors.contractExpected; 
	
	if(value === undefined) throw runtimeErrors.failedContractBinding;
	else return value;
};

/**
 * Computes complementation of the contract.
 * @param {Object} The target contract.
 * @return {Object} The complementation of the contract.
 */
$asyncscript.complementation = function(contract){
	if(contract === null || contract === undefined) return Object;
	else if(contract === Object) return null;
	else if(contract instanceof Complementation) return contract.contract;
	else if(contract instanceof Promise)
		if(contract.isError) throw contract.result;
		else if(contract.isCompleted) return this.complementation(contract.result);
		else return this.fork(arguments.callee.bind(this), contract);
	else return new Complementation(contract);
};

//CONTAINER AND ARRAY

invisibleField(Array.prototype, "__$asrtl_insert", function(element, position){
	if(position > this.length) return this;
	var result = this.slice(0);
	result.splice(position, element);
	return result;
});

containerContract.base.__$asrtl_contains = function(obj){ 
	for(var i = 0; i < this.__$size$__; i++){
		var contract = this.__$contracts$__[i];
		if($asyncscript.areEqual(obj, contract)) continue; else return false;	
	}
	return true;
};

container.base.__$asrtl_insert = function(obj, position){
	if(position > this.__$size$__) return this;
	var result = {__proto__: this.__proto__};
	Object.defineProperty(result, "__$size$__", {get: container.sizeAccessor, enumerable: false, configurable: false});
	Object.defineProperty(result, "__$contract$__", {value: {__$size$__: this.__$size$__ + 1, __$contracts$__: {}, __proto__: containerContract.base}, enumerable: false, writable: false, configurable: false});
	if(position === undefined || position === this.__$size$__){
		for(var i = 0; i < this.__$size$__; i++){
			var name = this.__$contract$__[i];
			containerContract.putNameIndexMap(i, name, result.__$contract$__);
			result.__$contract$__["__$contracts$__"][i] = this.__$contract$__["__$contracts$__"][i];
			if(name){
				result[name] = this[i];
				Object.defineProperty(result, i, {get: container.byNameAccessor(name), configurable: false, enumerable: false});
			}
			else Object.defineProperty(result, i, {value: this[i], configurable: false, enumerable: false, writable: false});
		}
		result.__$contract$__["__$contracts$__"][position] = Object;
		Object.defineProperty(result, position, {value: obj, enumerable: false, configurable: false, writable: false});
	}
	else for(var i = 0; i < this.__$size$__; i++){
		var name = this.__$contract$__[i];
		if(i < position){
			containerContract.putNameIndexMap(i, name, result.__$contract$__);
			result.__$contract$__["__$contracts$__"][i] = this.__$contract$__["__$contracts$__"][i];
			if(name){
				result[name] = this[i];
				Object.defineProperty(result, i, {get: container.byNameAccessor(name), configurable: false, enumerable: false});
			}
			else Object.defineProperty(result, i, {value: this[i], configurable: false, enumerable: false, writable: false});
		}
		else if(i === position){
			result.__$contract$__["__$contracts$__"][position] = Object;
			Object.defineProperty(result, position, {value: obj, enumerable: false, configurable: false, writable: false});
		}
		else {
			containerContract.putNameIndexMap(i + 1, name, result.__$contract$__);
			result.__$contract$__["__$contracts$__"][i + 1] = this.__$contract$__["__$contracts$__"][i];
			if(name){
				cprops[name] = this[i];
				Object.defineProperty(result, i + 1, {get: container.byNameAccessor(name), configurable: false, enumerable: false});
			}
			else Object.defineProperty(result, i + 1, {value: this[i], configurable: false, enumerable: false, writable: false});
		}
	}
	return result;
};

Signature.prototype.__$asrtl_contains =
container.base.__$asrtl_contains = function(value){
	for(var i = 0; i < this.__$size$__; i++)
		if($asyncscript.areEqual(value, this[i])) return true;
	return false;
};

invisibleField(Array.prototype, '__$asrtl_contains', function(value){
	for(var i = 0; i < this.length; i++)
		if($asyncscript.areEqual(value, this[i])) return true;
	return false;
});

//INVOCATION

Expression.prototype.__$asrtl_invoke = function(args){
	var ast = require('./ast.js'), result = new ast.CodeInvocationExpression(this.tree, [], this.tree.position.column, this.tree.position.line);
	for(var i = 0, element; i < args.length; i++)
		result.arguments.push(Expression.convert(args[i]));
	return new Expression(result, true);
};

FilterContract.prototype.__$asrtl_invoke = function(args){
	var value = args[0];
	return arguments.length >= 1 ? this.__$asrtl_contractfor(value) : runtimeErrors.invalidArgCount;
};

Expression.__$asrtl_invoke = function(args){
	var source = args[0];
	if(args.length < 1) return runtimeErrors.invalidArgCount;
	else if(!isString(source)) return new Error("Source code should be supplied as string");
	var SyntaxAnalyzer = require('./SyntaxAnalyzer.js'), ParserError = require('./ParserError.js');
	var result = new $asyncscript.Promise();
	SyntaxAnalyzer.parse(source, result.complete.bind(result));
	return result;
};

Signature.fromArguments = function(){
	if(arguments.length === 0) return this.empty;
	var result = new this();
	result.__$size$__ = arguments.length;
	for(var i = 0; i < arguments.length; i++) result[i] = $asyncscript.contractOf(arguments[i]);
	return result;
};

$asyncscript.typedef.__$asrtl_invoke = function(args){
	switch(args.length){
		case 0: return this;
		default: 
			var contract = args[0];
			return contract && contract.__$asrtl_relationship ? contract : null;
	}
};

//Function
OverloadList.prototype.__$asrtl_invoke = function(args){
	//find matching function
	var fn = this[args.length];
	return fn instanceof Function ? 
		fn.apply(null, args) : 
		new Error("Overloaded lambda function cannot be called with the specified arguments.");
};

/** Executes some portions of AsyncScript code.
 * @param {String} code A code to execute.
 * @return {Object} Execution result.
 */
$asyncscript.run = function(code, _this, callback){
	var compiler = require('./compiler.js');
	return compiler.run(_this, code, function(err, result){
		if(err !== undefined) return callback(err);
		else if(result instanceof Promise)
			if(result.isError) return callback(result.result);
			else if(result.isCompleted) return callback(undefined, result.result);
			else return result.on('success', function(v){ callback(undefined, v); }).on('error', callback);
		else return callback(undefined, result);
	});
};

Signature.prototype.__$asrtl_invoke = function(args){
	var body = args[0];
	if(args.length < 1) return runtimeErrors.invalidArgCount;
	else if(!isString(body)) return new Error("Lambda body should be passed as string");
	
	var lambda = "return @";
	for(var i = 0; i < this.__$size$__; i++)
		lambda += "_" + i + (i < this.__$size$__ - 1 ? ", " : "");
	lambda += " -> " + body + ";";
	var result = new $asyncscript.Promise();
	$asyncscript.run(lambda, null, function(err, lambda){
		if(err) return result.fault(err);
		lambda.__$contract$__ = this;
		return result.success(lambda);
	}.bind(this));
	return result;
};

invisibleField(Function, "__$asrtl_invoke", function(args){
	var body = args[0];
	if(args.length < 1) return runtimeErrors.invalidArgCount;
	var sig = new Signature();
	sig.__$size$__ = args.length - 1;
	for(var i = 1, contract; i < args.length; i++)
		if((contract = args[i]).__$asrtl_relationship) sig[i - 1] = contract;
		else return runtimeErrors.contractExpected;
	return sig.__$asrtl_invoke(body);
});

//String
invisibleField(String, "__$asrtl_invoke", function(args){ return toScriptString(args[0]); });
invisibleField(String.prototype, "__$asrtl_invoke", String.prototype.valueOf);
//Real and integer
$asyncscript.integer.__$asrtl_invoke = function(args){
	var value = args[0];
	if(value === null || value === undefined) return 0;
	else if(isReal(value)) return Math.round(value);
	else if(isBoolean(value)) return Number(value);
	else if(isString(value)) return hashCode(value);
	else if(value instanceof Function) return hashCode(value.toString());
	else return hashCode(toScriptString(value)); 
};
invisibleField(Number, "__$asrtl_invoke", function(args, target){
	return isReal(args[0]) ? args[0] : $asyncscript.integer.__$asrtl_invoke(args, target); 
});
invisibleField(Number.prototype, "__$asrtl_invoke", Number.prototype.valueOf);
//Boolean
invisibleField(Boolean, "__$asrtl_invoke", function(args){ return args[0] ? true : false; });
invisibleField(Boolean.prototype, "__$asrtl_invoke", Boolean.prototype.valueOf);
//Vector
Vector.prototype.__$asrtl_invoke = function(args){
	if(args.length < this.__$size$__) return new Error(utils.format("Expected %s arguments", this.__$size$__));
	for(var i = 0; i < this.__$size$__; i++) args[i] = {value: args[i], contract: this.contract};
	return container.create.apply(container, args);
};
//Array contract
ArrayContract.prototype.__$asrtl_invoke = function(args){
	if(args.length === 0) return container.empty;
	for(var i = 0; i < args.length; i++) args[i] = {value: args[i], contract: this.contract};
	return container.create.apply(container, args);
};
//RegExp
invisibleField(RegExp, "__$asrtl_invoke", function(args, t){ return new RegExp(String.__$asrtl_invoke(args, t)); });
invisibleField(RegExp.prototype, "__$asrtl_invoke", function(){ return new RegExp(this.source); });
//container
container.base.__$asrtl_invoke = function(args){
	var operator = this["()"];
	return operator ? $asyncscript.invoke(this, operator, args, $asyncscript.enterLambdaBody()) : this;
};
//container contract
containerContract.base.__$asrtl_invoke = function(args){
	if(args.length < this.__$size$__) return runtimeErrors.invalidArgCount;
	else if(args.length === 0) return container.empty;
	var result = {__proto__: container.base};
	Object.defineProperty(result, "__$size$__", {get: container.sizeAccessor, enumerable: false, configurable: false});
	Object.defineProperty(result, "__$contract$__", {value: this, enumerable: false, configurable: false, writable: false});
	for(var i = 0; i < this.__$size$__; i++){
		var name = this[i];
		if(name){
			cprops[name] = $asyncscript.binding(args[i], this.__$contracts$__[i]);
			Object.defineProperty(result, i, {get: container.byNameAccessor(name), configurable: false, enumerable: false});
		}
		else Object.defineProperty(result, i, {value: $asyncscript.binding(args[i], this.__$contracts$__[i]), configurable: false, enumerable: false, writable: false});
	}
	return result;
};

/**
 * Invokes the function.
 * @param {Object} self This-reference for the function.
 * @param {Function|Object} f A function or object to invoke.
 * @param {Array} args An arguments of the function.
 * @param {Object} destination Predefined result of the function.
 * @return {Object} Invocation result.
 */
$asyncscript.invoke = function(_this, f, args, destination){
	if(f === null || f === undefined)
		if(this.state.checked) throw runtimeErrors.voidref;
		else return null;
	//unwraps owner
	else if(_this instanceof Property) return this.invoke(_this.value, f, args, destination);
	else if(_this instanceof Promise)
		if(_this.isError) throw _this.result;
		else if(_this.isCompleted) return this.invoke(_this.result, f, args, destination);
		else return this.fork(function(_this){ return $asyncscript.invoke(_this, f, args, destination); }, _this);
	//unwraps function
	else if(f instanceof Property) return this.invoke(_this, f.value, args, destination);
	else if(f instanceof Promise)
			if(f.isError) throw f.result;
			else if(f.isCompleted) return invokeSpecial(_this, f.result, args, destination);
			else return this.fork(function(f){ return $asyncscript.invoke(_this, f, args, destination); }, f);
	//invokes object, not function
	else if(f.__$asrtl_invoke){
		//synchronize arguments
		for(var i = arguments[4] || 0, a; i < args.length; i++)
			if((a = args[i]) instanceof Property)
				a = args[i] = a.value;
			if(a instanceof Promise)
				if(a.isError) {this.fault(a.result, destination); throw a.result; }
				else if(a.isCompleted) args[i] = a.result;
			else return (this.fork(function(a){
				this.args[this.index] = a;
				return $asyncscript.invoke(_this, f, this.args, destination);
			}.bind({"args": args, index: i}), a), destination);
		var result = f.__$asrtl_invoke(args, _this);
		this[result instanceof Error ? "fault" : "ret"](result, destination);
		return result;
	}
	//native js function
	else if(f instanceof Function){
		if(f.isStandaloneLambda) this.prepareLambdaInvocation(destination); 
		return f.apply(_this, args);
	} 
	else if(this.state.checked) throw runtimeErrors.unsupportedOp('()');
	else return null;
};

$asyncscript.invokeMethod = function(_this, method, args, destination){
	if(_this === null || _this === undefined)
		if(this.state.checked) throw runtimeErrors.voidref;
		else return null;
	else if(_this instanceof Property)
		switch(method){
			case "get": this.prepareLambdaInvocation(destination); return _this.__$asrtl_getvalue();
			case "set": this.prepareLambdaInvocation(destination); return _this.__$asrtl_setvalue(args[0]);
			default: return this.invokeMethod(_this.value, method, args, destination);
		}
	else if(_this instanceof Promise)
		if(_this.isError) throw _this.result;
		else if(_this.isCompleted) return this.invokeMethod(_this.result, method, args, destination);
		else return this.fork(function(_this){ return $asyncscript.invokeMethod(_this, method, args, destination); }, _this);
	else if(method instanceof Property) return this.invokeMethod(_this, method.value, args, destination);
	else if(_this instanceof Promise)
		if(_this.isError) throw _this.result;
		else if(_this.isCompleted) return this.invokeMethod(_this.result, method, args, destination);
		else return this.fork(function(_this){ return $asyncscript.invokeMethod(_this, method, args, destination); }, _this);
	else return this.invoke(_this, this.getMember(_this, method), args, destination);
};

$asyncscript.createCallback = function(promise){
	if(promise instanceof Promise){
		promise = Promise.prototype.complete.bind(promise);
		promise.__$contract$__ = new Signature(Object, promise.__$contract$__);
		return promise;
	}
	else promise = null;
};

//Repeat

function RepeatState(iteration, aggregator){
	this.iteration = iteration;
	if(aggregator instanceof Function) {
		this.aggregator = aggregator;
		this.emitValue = function(value){
			if(!("result" in this)) this.result = value;
			else if((value = this.aggregator(this.result, value)) instanceof Promise)
				if(value.isError){ this.promise.fault(value.result); this.terminated = true; }
				else if(value.isCompleted) return this.emitValue(value.result);
				else $asyncscript.fork(this.emitValue.bind(this), value);
			else this.result = value;
		}
	}
	else this.emitValue = Array.prototype.push.bind(this.result = new Array());
	this.promise = new Promise();
	this.terminated = false;
}

RepeatState.prototype['continue'] = function(args, idx){
	if(this.terminated) return false;
	for(var i = idx || 0, value; i < args.length; i++)
		if((value = args[i]) instanceof Promise)
			if(value.isError){ this.terminated = true; this.promise.fault(value.result); return false; }
			else if(value.isCompleted) {args[i] = value.result; return this['continue'](args, i); }
			else return $asyncscript.fork(function(value){
				this.args[this.index] = value;
				return this.state['continue'](this.args, this.index);
			}.bind({state: this, index: i, args: args}), value), true;
		else this.emitValue(value);
	$asyncscript.fork(this.iteration, this);
	return true;
};

RepeatState.prototype['break'] = function(args, idx){
	if(this.terminated) return false;
	for(var i = idx || 0, value; i < args.length; i++)
		if((value = args[i]) instanceof Promise)
			if(value.isError){ this.terminated = true; this.promise.fault(value.result); return false; }
			else if(value.isCompleted) {args[i] = value.result; return this['break'](args, i); }
			else return $asyncscript.fork(function(value){
				this.args[this.index] = value;
				return this.state['break'](this.args, this.index);
			}.bind({state: this, index: i, args: args}), value), true;
		else this.emitValue(value);
	this.terminated = true;
	if(this.result instanceof Promise)
		if(this.result.isError) this.promise.fault(this.result.result);
		else if(this.result.isCompleted) this.promise.success(this.result.result);
		else this.result.route(this.promise);
	else this.promise.success(this.result);
	return true;
};

/**
 * Executes repeatable block of code.
 * @param {Function} iteration A function with single parameter that should be executed in loop.
 * @param {Function} aggregator Aggregation function used to create loop result. Optional.
 * @return Loop execution result.
 */
$asyncscript.repeat = function(iteration, aggregator){
	if(aggregator instanceof Promise)
		if(aggregator.isError) throw aggregator.result;
		else if(aggregator.isCompleted) return this.repeat(iteration, aggregator.result);
		else return this.fork(function(aggregator){ return $asyncscript.repeat(iteration, aggregator); }, aggregator);
	else if(aggregator instanceof Property) return this.repeat(iteration, aggregator.value);
	var state = new RepeatState(iteration, aggregator);
	$asyncscript.fork(iteration, state);
	return state.promise;
};

/**
 * Provides logic for 'continue' operator.
 * @param {Object} state Scope object.
 * @param {Array} args An array of values to be passed into the continuation procedure.
 * @return {Object} A value returned from continuation procedure.
 */
$asyncscript.continueWith = function(state, args){
	//synchronize state object	
	if(state === null || state == undefined) throw this.errors.voidref;
	else if(state instanceof Property) return this.continueWith(state.value);
	else if(state instanceof Promise) 
		if(state.isError) throw state.result;
		else if(state.isCompleted) return this.continueWith(state.result);
		else return $asyncscript.fork(function(state){
			return $asyncscript.continueWith(state, args);
		}, state);
	else if(state instanceof RepeatState || state instanceof ForEachState) return state['continue'](args, 0);
	else return $asyncscript.invoke(state, "continue", args);
};

/**
 * Provides logic for 'break' operator.
 * @param {Object} state Scope object.
 * @param {Array} args An array of values to be passed into the continuation procedure.
 * @return {Object} A value returned from continuation procedure.
 */
$asyncscript.breakWith = function(state, args){
	//synchronize state object	
	if(state === null || state == undefined) throw this.errors.voidref;
	else if(state instanceof Property) return this.continueWith(state.value);
	else if(state instanceof Promise) 
		if(state.isError) throw state.result;
		else if(state.isCompleted) return this.continueWith(state.result);
		else return $asyncscript.fork(function(state){
			return $asyncscript.continueWith(state, args);
		}, state);
	else if(state instanceof RepeatState || state instanceof ForEachState) return state['break'](args, 0);
	else return $asyncscript.invoke(state, "break", args);
};

/**
 * Implements logic for !! operator with finally block.
 * @param {Function} t Dangerous code.
 * @param {Function} c Catch code that accepts 'error' parameter.
 * @param {Function} f Finally code.
 * @return Block execution result.
 */
$asyncscript.tryCatchFinally = function(t, c, f){
	var result, caught = false;
	try{
		result = t();
		while(result instanceof Promise)
			if(result.isError) throw result.result;
			else if(result.isCompleted) result = result.result;
			else break;
		return result;
	}
	catch(error){
		caught = true;
		return c(error); 
	}
	finally{
		if(!caught && result instanceof Promise)
			$asyncscript.fork(function(result){
				f();
				return result;
			}, 
			result, 
			function(error){
				try{ return c(error); } finally{ f(); }
			});
		else f();
	}
};

/**
 * Implements logic for !! operator without finally block.
 * @param {Function} t Dangerous code.
 * @param {Function} c Catch code that accepts 'error' parameter.
 * @return Block execution result.
 */
$asyncscript.tryCatch = function(t, c){
	var result, caught = false;
	try{
		result = t();
		while(result instanceof Promise)
			if(result.isError) throw result.result;
			else if(result.isCompleted) result = result.result;
			else break;
	}
	catch(error){
		caught = true;
		result = c(error); 
	}
	finally{
		if(!caught && result instanceof Promise) result = $asyncscript.fork(function(result){ return result; }, result, c);
	}
	return result;
};

/**
 * Provides logic for select-case operator.
 * @param {Object} value A value to check.
 * @param {Object} comparer Value comparer.
 * @param {Array} values An array of {value: 0, handler: Function} values.
 * @param {Integer} state Internal state. Must be 0.
 * @param {Function} def Default handler.
 */
$asyncscript.select = function(value, comparer, values, state, def){
	for(var i = state; i < values.length; i++){
		var pair = values[i], caseval = pair.value, handler = pair.handler;
		if(caseval instanceof Promise)
			if(caseval.isError) throw caseval.result;
			else if(caseval.isCompleted) return pair.value = caseval.result, this.select(value, comparer, values, state, def);
			else return this.fork(function(caseval){
				this.pair.value = caseval;
				return $asyncscript.select(value, comparer, values, this.state, def);
			}.bind({state: i, pair: pair}), caseval);
		//compares value
		var equals = comparer === undefined ? this.areEqual(value, caseval) : this.invoke(null, comparer, [value, caseval]);
		//handle equality
		while(equals instanceof Promise)
			if(equals.isError) throw equals.result;
			else if(equals.isCompleted) equals = equals.result;
			else return this.fork(function(equals){
				return equals ? this.handler() : $asyncscript.select(value, comparer, values, this.state, def);
			}.bind({handler: handler, state: i + 1}), equals);
		if(equals) return handler();
	}
	//invokes default
	return def ? def() : null;
};

function ForEachState(iteration, aggregator){
	RepeatState.call(this, iteration, aggregator);
}

ForEachState.prototype.moveNext = function(source){
	if(this.terminated) return false;
	else if(source === null || source === undefined) {this.terminated = true; this.promise.success(this.result); return false; }
	else if(source instanceof Promise)
		if(source.isError) {this.terminated = true; this.promise.fault(source.result); return false; }
		else if(source.isCompleted) return this.moveNext(source);
		else return $asyncscript.fork(this.moveNext.bind(this), source);
	else if(source instanceof Property) return this.moveNext(source.value);
	//executes the next iteration
	$asyncscript.fork(function(value){
		this.iteration(this, value);
	}.bind(this), 
	$asyncscript.getMember(this.source = source, "current"),
	this.promise.fault.bind(this.promise));
	return true;
};

ForEachState.prototype['continue'] = function(args, idx){
	if(this.terminated) return false;
	for(var i = idx || 0, value; i < args.length; i++)
		if((value = args[i]) instanceof Promise)
			if(value.isError){ this.terminated = true; this.promise.fault(value.result); return false; }
			else if(value.isCompleted) {args[i] = value.result; return this['continue'](args, i); }
			else return $asyncscript.fork(function(value){
				this.args[this.index] = value;
				return this.state['continue'](this.args, this.index);
			}.bind({state: this, index: i, args: args}), value);
		else this.emitValue(value);
	//moves to the next value	
	return this.moveNext($asyncscript.invokeMethod(this.source, "next", []));
};

ForEachState.prototype['break'] = function(args, idx){
	if(this.terminated) return false;
	for(var i = idx || 0, value; i < args.length; i++)
		if((value = args[i]) instanceof Promise)
			if(value.isError){ this.terminated = true; this.promise.fault(value.result); return false; }
			else if(value.isCompleted) {args[i] = value.result; return this['break'](args, i); }
			else return $asyncscript.fork(function(value){
				this.args[this.index] = value;
				return this.state['break'](this.args, this.index);
			}.bind({state: this, index: i, args: args}), value);
		else this.emitValue(value);
	this.terminated = true;
	if(this.result instanceof Promise)
		if(this.result.isError) this.promise.fault(this.result.result);
		else if(this.result.isCompleted) this.promise.success(this.result.result);
		else this.result.route(this.promise);
	else this.promise.success(this.result);	
	return true;
};

$asyncscript.foreach = function(source, iteration, aggregator){
	if(source === null || source === undefined) return null;
	else if(source instanceof Promise)
		if(source.isError) throw source.result;
		else if(source.isCompleted) return this.foreach(source.result, iteration, aggregator);
		else return this.fork(function(source){
			return $asyncscript.foreach(source, iteration, aggregator);
		}, source);
	else if(source instanceof Property) return this.foreach(source.value, iteration, aggregator);
	else if(aggregator instanceof Promise)
		if(aggregator.isError) throw aggregator.result;
		else if(aggregator.isCompleted) return this.foreach(source, iteration, aggregator.result);
		else return this.fork(function(aggregator){
			return $asyncscript.foreach(source, iteration, aggregator);
		}, aggregator);
	else if(aggregator instanceof Property) return this.foreach(source, iteration, aggregator.value);
	var state = new ForEachState(iteration, aggregator);
	state.moveNext(source);
	return state.promise;
};

//Debugger

/**
 * Transfers control to the debugger.
 * @param {String} name The name of the breakpoint.
 * @param {Object} context Lexical scope context that contains this-reference and all named slots.
 * @param {Object} position Column and line.
 * @return {Object} A value returned from the debugger.
 */
$asyncscript.breakpoint = function(context, name, position){
	var result;
	if(this.debugger) {		
		result = new this.Promise();
		this.debugger.breakpoint(context, name, position, result.complete.bind(result));
	}
	else {
		console.error("AsyncScript debugger is not attached");
		result = null;
	}
	return result;
};

$asyncscript.loadScript = function(name, loader){
	if(!(loader instanceof Function)) loader = require;	
	try{
		return loader(name);	//attempts to load through NodeJS
	}
	catch(e){	//attempts to load AsyncScript module
		var path = loader('path'), fs = loader('fs');
		//attempts to find in modules path
		var filename = path.join(__dirname, "../modules", name);
		if(fs.existsSync(filename)) return require(filename);
		//attempts to find in the root directory of the executed script
		filename = path.dirname(process.argv[1]);	//extracts filename of the executed script
		filename = path.join(filename, name);
		if(fs.existsSync(filename)) return require(filename);
	}
	throw new Error("Module " + name + " not found");
};
