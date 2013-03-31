//AsyncScript Common Extensions

var ParserError = require("../lib/ParserError.js"), 
	js = require("../lib/jsnodes.js"),
	ast = require("../lib/ast.js"),
	Lexeme = require("../lib/Lexeme.js"),
	compiler = require("../lib/compiler.js"),
	ScriptTranslator = compiler.ScriptTranslator,
	mangle = compiler.mangle,
	GenericScope = compiler.GenericScope,
	CatchScope = compiler.CatchScope;

//Loading script at runtime
ASYNCSCRIPT_SYNTAX_EXTENSIONS.require = function(column, line, terminators, callback){
	var lex = this.nextLexeme(function(lex){ return lex && lex.kind === "string"; },
								function(column, line, lex){ return ParserError.expected("Module name", lex, column, line); });
	if(lex instanceof ParserError) return callback(lex);
	//$asyncscript.loadScript("name", require);
	var result = new ast.JavaScriptCode(column, line, new js.JSCall("$asyncscript", "loadScript", new js.JSStringLiteral(lex.value), "require"));
	//moves to the next lexeme
	lex = this.shouldNextLexeme();
	return lex instanceof ParserError ? callback(lex) : callback(undefined, result);
};
//========================================================================================================
//Awaits the specified result
/**
 * Creates a new AWAIT expression.
 * @class Represents synchronization block.
 * @param {Integer} column The number of the column in the source code.
 * @param {Integer} line The number of the line in the source code.
 */
function CodeAwaitExpression(column, line){
	if(arguments.length === 0){
		$asyncscript.awaitArgument = this.awaitArgument;	
	}
	else {
		this.nodeType = "CodeAwaitExpression";
		this.synchronizedValues = new Array();
		this.$extension = "std";
		this.position = {'column': column, 'line': line};
	}
}

CodeAwaitExpression.prototype.awaitArgument = function(args, index, _this){
	return $asyncscript.fork(function(a){
		args[index] = a;
		return args.callee.apply(_this, args);
	}, 
	args[index], 
	function(a){
		(args[index] = new $asyncscript.Promise()).fault(a);
		return args.callee.apply(_this, args);
	}); 
};

CodeAwaitExpression.prototype.$prerequisite = "$asyncscript.awaitArgument = " + CodeAwaitExpression.prototype.awaitArgument;

CodeAwaitExpression.prototype.toString = function(){
	var result = 'await(';
	if(this.synchronizedValues.length > 0)
		this.synchronizedValues.forEach(function(l, idx, array){ 
			result += l + (idx < array.length - 1 ? ', ' : ')')
		});
	else result += ') -> ';
	result += this.body;
	if(this['else'])
		result += ' : ' + this['else'];
	return result;
};

function expressionListToString(){
	var result = '{';
	this.forEach(function(v){
		result += ' ' + v + ';';
	});
	result += ' }';
	return result;
}

function AwaitScope(names){ 
	GenericScope.call(this, true);
	names.forEach(function(name){
		if(name!= null) this[name] = {};
	}, this.input = {});
}

AwaitScope.prototype = {
	get checked(){ return this.parent.checked; },
	get identifiers(){ return Object.keys(this.locals).concat(Object.keys(this.input)); },
};

AwaitScope.prototype.declareLocal = function(name){
	return name in this.input ? false : GenericScope.prototype.declareLocal.call(this, name);
};

AwaitScope.prototype.identifierOptions = function(name){
	return this.input[name] || this.locals[name];
};

AwaitScope.prototype.translate = function(values, body, fault){
	function binding(value, contract){
		return new js.JSCall("$asyncscript", "binding", value, contract)	
	}
	if(body instanceof Array) body.unshift(new js.JSCode("value = new $asyncscript.Promise()")); 
	else body = [new js.JSReturn(body)];
	//declare locals	
	Object.keys(this.locals).forEach(function(name){
		this.unshift(new js.JSVariableDecl(mangle(name)));
	}, body);
	var args = [new js.JSThis()], names = new Array();
	//insert synchronization for each parameter
	body = values.map(function(v, i, values){
		var name, value, contract;	
		if("name" in v && "contract" in v && "value" in v){
			names.push(name = mangle(v.name));
			args.push(value = v.value);
			contract = v.contract;
		}
		else {
			names.push(name = "$$" + i);
			args.push(value = v);
		}
		var result = "if(" + name + " instanceof $asyncscript.Promise)" +
		"if(" + name + ".isError) " + (fault ? "return $asyncscript.fork(__$err$__.bind(this), " + name + ".result)" : "throw " + name + ".result") + ";" +
		"else if(" + name + ".isCompleted) {arguments[" + i + "] = " + name + ".result; return arguments.callee.apply(this, arguments); }" +
		"else return $asyncscript.awaitArgument(arguments, " + i + ", this);" +
		(contract ? (name + "=" + binding(name, contract)) : "");
		return new js.JSCode(result);
	}).concat(body);
	if(fault) body.unshift(new js.JSCode("var __$err$__ = " + fault));
	return new js.JSCall(new js.JSFunction(names, body), "call", args);
};


CodeAwaitExpression.prototype.translate = function(context, emitDebug){
	context.pushScope(new AwaitScope(this.synchronizedValues.map(function(v){ 
		return v.name; 
	})));
	var values = ScriptTranslator.expressions(this.synchronizedValues, function(v){
		var value;
		if(v instanceof ast.CodeLetExpression){
			value = ScriptTranslator.translate(v.value, context, emitDebug);
			if(value instanceof ParserError) return value;
			var contract = v.contract === undefined || v.contract instanceof ast.CodeBuiltInExpression && v.value === "object" ? undefined : ScriptTranslator.translate(v.contract, context, emitDebug);
			if(contract instanceof ParserError) return contract;
			return {name: v.name, contract: contract, value: value};
		}
		else return ScriptTranslator.translate(v, context, emitDebug);
	});
	if(values instanceof ParserError) return values;
	//parse else branch
	var body, handler;
	if(this.body instanceof Array){
		context.scope.declareLocal("value");
		body = ScriptTranslator.expressions(this.body, context, emitDebug);
		if(body instanceof ParserError) return body;
		body.push(new js.JSReturn("value"));
	}
	else {
		body = ScriptTranslator.translate(this.body, context, emitDebug);
		if(body instanceof ParserError) return body;
	}
	//analyze error handler
	var scope = context.popScope();
	if(handler = this['else']){
		context.pushScope(new CatchScope());
		handler = ScriptTranslator.block(handler, context, emitDebug, true);
		if(handler instanceof ParserError) return handler;
		else handler = context.popScope().translate(handler);
	}
	return scope.translate(values, body, handler);
};

ASYNCSCRIPT_SYNTAX_EXTENSIONS.await = function(column, line, terminators, callback){
	var result = new CodeAwaitExpression(column, line), async = require('../lib/async_helpers.js');
	//passes through await keyword
	var expr = this.nextLexeme(
		function(lex){ return lex === Lexeme.punctuation['(']; },
		function(column, line, lex){ return ParserError.expected('(', lex, column, line); });
	if(expr instanceof ParserError) return callback(expr);
	//parse set of let statements
	async.asyncWhile(function(callback){ return callback(this.lookup !== Lexeme.punctuation[')']); }.bind(this),
		function(next){
			var expr = this.shouldNextLexeme();
			return expr instanceof ParserError ? callback(expr) : this.next([Lexeme.punctuation[','], Lexeme.punctuation[')']], function(err, expr){
				if(err) return callback(err);
				result.synchronizedValues.push(expr);
				return next();
			});
		}.bind(this),
		function(){
			//pass through ) operator
			var expr = this.shouldNextLexeme();
			if(expr instanceof ParserError) return expr;
			else if(expr !== Lexeme.punctuation['->']) return callback(ParserError.expected('->', expr, this.column, this.line));
			expr = this.shouldNextLexeme();
			if(expr instanceof ParserError) return callback(expr);
			terminators.push(Lexeme.punctuation[':']);
			//parse await body
			this.next(terminators, function(err, body){
				if(err) return callback();
				terminators.pop();
				result.body = body;
				//synchronization exception handler
				switch(this.lookup){
					case Lexeme.punctuation[':']:
						body = this.shouldNextLexeme();	//pass through else keyword
						if(body instanceof ParserError) return callback(body);
						return this.next(terminators, function(err, _else){
							if(err) return callback(err);
							this['else'] = _else;
							return callback(undefined, result);
						}.bind(this));
					default: return callback(undefined, result); 
				}
			}.bind(this));
	}.bind(this));
};

/**
 * Restores the AWAIT expression.
 */
ASYNCSCRIPT_SYNTAX_EXTENSIONS.await.restore = function(tree, loader){
	var result = new CodeAwaitExpression(tree.position.column, tree.position.line);
	result.synchronizedValues = this.restore(tree.synchronizedValues, loader);
	result.body = this.restore(tree.body, loader);
	if("else" in tree) result['else'] = this.restore(tree['else'], loader);
	return result;
};

//========================================================================================================
//executes a new asynchronous operation
function CodeForkExpression(column, line, operand){
	if(arguments.length === 0){	//deserialization
		$asyncscript.asyncInvoke = this.implementation;
	}
	else {
		//normal creation
		this.position = {"column": column, "line": line};
		this.nodeType = "CodeForkExpression";
		this.$extension = "std";
		this.operand = operand;
	}

}

CodeForkExpression.prototype.toString = function(){
	return '(fork ' + this.operand + ')';
};

CodeForkExpression.prototype.implementation = function(_this, f, args, destination){
	if(destination === undefined) args.push($asyncscript.createCallback(destination = new $asyncscript.Promise()));
	else if(destination instanceof $asyncscript.Promise) args.push($asyncscript.createCallback(destination));
	return arguments.callee(_this, f, args, destination), destination;
};

CodeForkExpression.prototype.$prerequisite = "$asyncscript.asyncInvoke = " + CodeForkExpression.prototype.implementation;

CodeForkExpression.prototype.translate = function(context, emitDebug){
	function invokeAsync(_this, method, args, destination){
		return destination ? new js.JSCall("$asyncscript", "invokeAsync", _this || new js.JSUndefined(), method, args, destination) :
			new js.JSCall("$asyncscript", "invokeAsync", _this || new js.JSUndefined(), method, args);	
	}
	function fork(code, target, fault){
		if(target)
			return fault ? new js.JSCall("$asyncscript", "fork", code, target, fault): new js.JSCall("$asyncscript", "fork", code, target);
		else if(fault)
			return new js.JSCall("$asyncscript", "fork", code, new js.JSNull(), fault);
		else return new js.JSCall("$asyncscript", "fork", code);
	}
	var expression;	
	if(this.operand instanceof ast.CodeInvocationExpression){	//invoke async
		expression = this.operand;
		var self = expression.self ? ScriptTranslator.translate(expression.self, context, emitDebug) : null;
		if(self instanceof ParserError) return self;
		var method = self && expression.method.name ? new js.JSStringLiteral(mangle(expression.method.name)) : ScriptTranslator.translate(expression.method, context, emitDebug);
		if(method instanceof ParserError) return method;
		var destination = expression.destination ? ScriptTranslator.translate(expression.destination, context, emitDebug) : undefined;
		if(destination instanceof ParserError) return destination;
		var arguments = ScriptTranslator.expressions(expression.arguments, context, emitDebug);
		if(arguments instanceof ParserError) return arguments;
		//compiling invocation
		return invookeAsync(self, method, new js.JSNewArray(arguments), destination);	
	}
	else{	//compiles into the block (ignores destinaiton)
		expression = this.operand;
		context.pushScope(new GenericScope(true));
		expression = ScriptTranslator.block(expression instanceof Array ? expression : [expression] , context, emitDebug, true);
		context.popScope();
		if(expression instanceof ParserError) return expression;
		expression = new js.JSCall(new js.JSScope(expression), "bind", new js.JSThis());
		return fork(expression);
	}
};

ASYNCSCRIPT_SYNTAX_EXTENSIONS.fork = function(column, line, terminators, callback){
	var lex = this.shouldNextLexeme(), result = new CodeForkExpression(column, line);
	if(lex instanceof ParserError) return callback(lex);
	//parse operand
	this.next(terminators, function(err, operand){
		if(err) return callback(err);
		result.operand = operand;
		return callback(undefined, result);
	}.bind(this));
};

/**
 * Restores the FORK expression.
 */
ASYNCSCRIPT_SYNTAX_EXTENSIONS.fork.restore = function(tree, loader){
	return new CodeForkExpression(tree.position.column, tree.position.line, this.restore(tree.operand, loader));
};

//========================================================================================================
//Computes the size of the specified object

function CodeSizeOfExpression(column, line, operand){
	if(arguments.length === 0){	//deserialization
		$asyncscript.sizeof = this.implementation;
	}
	else {
		//normal creation
		this.position = {"column": column, "line": line};
		this.nodeType = "CodeSizeOfExpression";
		this.$extension = "std";
		this.operand = operand;
	}
}

CodeSizeOfExpression.prototype.toString = function(){
	return '(sizeof ' + this.operand + ')';
};

CodeSizeOfExpression.prototype.implementation = function(obj){
	if(obj instanceof $asyncscript.Promise)
		if(obj.isError) throw obj.result;
		else if(obj.isCompleted) return arguments.callee(obj.result);
		else return $asyncscript.fork(arguments.callee, obj);
	else if(obj instanceof $asyncscript.Property)
		return arguments.callee(obj.value);
	else if(obj === null || obj === undefined) return 0;
	else if(obj.__$c$__ || 
		obj.__$cc$__ || 
		obj instanceof $asyncscript.Signature || 
		obj instanceof $asyncscript.Vector || 
		obj instanceof $asyncscript.OverloadList) return obj.__$size$__;
	else return 0;
};

CodeSizeOfExpression.prototype.$prerequisite = "$asyncscript.sizeof = " + CodeSizeOfExpression.prototype.implementation;

CodeSizeOfExpression.prototype.translate = function(context, emitDebug){
		return new js.JSCall("$asyncscript", "sizeof", ScriptTranslator.translate(this.operand, context, emitDebug));
};

ASYNCSCRIPT_SYNTAX_EXTENSIONS.sizeof = function(column, line, terminators, callback){
	var lex = this.shouldNextLexeme(), result = new CodeSizeOfExpression(column, line);
	if(lex instanceof ParserError) return callback(lex);
	//parse operand
	this.next(terminators, function(err, operand){
		if(err) return callback(err);
		result.operand = operand;
		return callback(undefined, result);
	}.bind(this));
};

/**
 * Restores the SIZEOF expression.
 */
ASYNCSCRIPT_SYNTAX_EXTENSIONS.sizeof.restore = function(tree, loader){
	return new CodeSizeOfExpression(tree.position.column, tree.position.line, this.restore(tree.operand, loader));
};

//========================================================================================================
//read reactive value
function CodeReactiveReadExpression(column, line, operand){
	if(arguments.length === 0){	//deserialization
		$asyncscript.getv = this.implementation;
	}
	else {
		//normal creation
		this.position = {"column": column, "line": line};
		this.nodeType = "CodeReactiveReadExpression";
		this.$extension = "std";
		this.operand = operand;
	}

}

CodeReactiveReadExpression.prototype.toString = function(){
	return '(getv ' + this.operand + ')';
};

CodeReactiveReadExpression.prototype.implementation = function(obj){
	if(obj instanceof $asyncscript.Promise)
		if(obj.isError) throw obj.result;
		else if(obj.isCompleted) return arguments.callee(obj.result);
		else return $asyncscript.fork(arguments.callee, obj);
	else if(obj instanceof $asyncscript.Property)
		return arguments.callee(obj.value);
	else return obj;
};

CodeReactiveReadExpression.prototype.$prerequisite = "$asyncscript.getv = " + CodeReactiveReadExpression.prototype.implementation;

CodeReactiveReadExpression.prototype.translate = function(context, emitDebug){
		return new js.JSCall("$asyncscript", "getv", ScriptTranslator.translate(this.operand, context, emitDebug));
};

ASYNCSCRIPT_SYNTAX_EXTENSIONS.getv = function(column, line, terminators, callback){
	var lex = this.shouldNextLexeme(), result = new CodeReactiveReadExpression(column, line);
	if(lex instanceof ParserError) return callback(lex);
	//parse operand
	this.next(terminators, function(err, operand){
		if(err) return callback(err);
		result.operand = operand;
		return callback(undefined, result);
	}.bind(this));
};

/**
 * Restores the GETV expression.
 */
ASYNCSCRIPT_SYNTAX_EXTENSIONS.getv.restore = function(tree, loader){
	return new CodeReactiveReadExpression(tree.position.column, tree.position.line, this.restore(tree.operand, loader));
};

//========================================================================================================
//sets reactive value

function CodeReactiveWriteExpression(column, line, left, right){
	if(arguments.length === 0){	//deserialization
		$asyncscript.setv = this.implementation;
	}
	else {
		//normal creation
		this.position = {"column": column, "line": line};
		this.nodeType = "CodeReactiveWriteExpression";
		this.$extension = "std";
		this.left = left;
		this.right = right;
	}

}

CodeReactiveWriteExpression.prototype.toString = function(){
	return '(setv (' + this.left + ') = (' + this.right + '))';
};

CodeReactiveWriteExpression.prototype.implementation = function(left, right){
	if(left instanceof $asyncscript.Property && left.canWrite)
		//synchronize right argument		
		if(right instanceof $asyncscript.Promise)
			if(right.isError) throw left.result;
			else if(right.isCompleted) return arguments.callee(left, right.result);
			else return $asyncscript.fork(function(right){
				return this(left, right);
			}.bind(arguments.callee), right);		
		//unwraps right value
		else if(right instanceof $asyncscript.Property)
			return arguments.callee(left, right.value);
		//writes value
		else return left.value = right, true;
	//synchronize left argument
	else if(left instanceof $asyncscript.Promise)
		if(left.isError) throw left.result;
		else if(left.isCompleted) return arguments.callee(left.result, right);
		else return $asyncscript.fork(function(left){
			return this(left, right);
		}.bind(arguments.callee), left);	
	else return false;
};

CodeReactiveWriteExpression.prototype.$prerequisite = "$asyncscript.setv = " + CodeReactiveWriteExpression.prototype.implementation;

CodeReactiveWriteExpression.prototype.translate = function(context, emitDebug){
		return new js.JSCall("$asyncscript", "setv", ScriptTranslator.translate(this.left, context, emitDebug), ScriptTranslator.translate(this.right, context, emitDebug));
};

ASYNCSCRIPT_SYNTAX_EXTENSIONS.setv = function(column, line, terminators, callback){
	var lex = this.shouldNextLexeme(), result = new CodeReactiveWriteExpression(column, line);
	if(lex instanceof ParserError) return callback(lex);
	//parse left operand
	this.next([Lexeme.operators['=']], function(err, left){
		if(err) return callback(err);
		result.left = left;
		//parse right operand		
		var lex = this.shouldNextLexeme();
		return lex instanceof ParserError ? callback(lex) : this.next(terminators, function(err, right){
			if(err) return callback(err);
			result.right = right;
			return callback(undefined, result);
		}.bind(this));
	}.bind(this));
};

/**
 * Restores the SETV expression.
 */
ASYNCSCRIPT_SYNTAX_EXTENSIONS.setv.restore = function(tree, loader){
	return new CodeReactiveWriteExpression(tree.position.column, tree.position.line, this.restore(tree.left, loader), this.restore(tree.right, loader));
};

//========================================================================================================
//creates a new future object with the specified contract
function CodeFutureExpression(column, line, contract){
	if(arguments.length === 0){	//deserialization
		$asyncscript.newPromise = this.implementation;
	}
	else {
		//normal creation
		this.position = {"column": column, "line": line};
		this.nodeType = "CodeFutureExpression";
		this.$extension = "std";
		this.contract = contract;
	}

}

CodeFutureExpression.prototype.toString = function(){
	return '(future ' + this.contract + ')';
};

CodeFutureExpression.prototype.implementation = function(contract){
	if(contract instanceof $asyncscript.Property) return arguments.callee(contract.value);
	else if(contract instanceof $asyncscript.Promise)
		if(contract.isError) throw contract.result;
		else if(contract.isCompleted) return arguments.callee(contract.result);
		else return $asyncscript.fork(arguments.callee, contract);
	else if(contract.__$asrtl_relationship) return new $asyncscript.Promise(contract);
	else if($asyncscript.state.checked) throw runtimeErrors.contractExpected;
	else return null;
};

CodeFutureExpression.prototype.$prerequisite = "$asyncscript.newPromise = " + CodeReactiveReadExpression.prototype.implementation;

CodeFutureExpression.prototype.translate = function(context, emitDebug){
		return new js.JSCall("$asyncscript", "newPromise", ScriptTranslator.translate(this.contract, context, emitDebug));
};

ASYNCSCRIPT_SYNTAX_EXTENSIONS.future = function(column, line, terminators, callback){
	var lex = this.shouldNextLexeme(), result = new CodeFutureExpression(column, line);
	if(lex instanceof ParserError) return callback(lex);
	//parse operand
	this.next(terminators, function(err, contract){
		if(err) return callback(err);
		result.contract = contract;
		return callback(undefined, result);
	}.bind(this));
};

/**
 * Restores the FUTURE expression.
 */
ASYNCSCRIPT_SYNTAX_EXTENSIONS.future.restore = function(tree, loader){
	return new CodeFutureExpression(tree.position.column, tree.position.line, this.restore(tree.contract, loader));
};
