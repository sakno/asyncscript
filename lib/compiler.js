var ParserError = require('./ParserError.js'), 
	ast = require('./ast.js'), 
	SyntaxAnalyzer = require('./SyntaxAnalyzer.js'), 
	Lexeme = require('./Lexeme.js'),
	js = require('./jsnodes.js'),
	async = require('./async_helpers.js');

//================================================================================================

function mangle(name){
	var result = '';
	for(var i = 0, ch; i < name.length; i++)
		result += Lexeme.isAlpha(ch = name[i]) || Lexeme.isDigit(ch) || ch === '$' ? ch : '$' + i;
	return result;
}

/**
 * Initializes a new generic scope.
 * @class Represents generic scope.
 */
function GenericScope(transient){
	this.locals = {};
	this.transient = transient;
}

GenericScope.prototype = {
	get checked(){ return this.parent ? this.parent.checked : true; },
	get identifiers(){ return Object.keys(this.locals); }
};

/**
 * Declares a new local variable.
 * @param {String} name The name of the local variable.
 * @return {Boolean} true
 */
GenericScope.prototype.declareLocal = function(name){	
	if(this.locals[name]) return;
	else return this.locals[name] = {};
};

GenericScope.prototype.identifierOptions = function(name){ return this.locals[name]; };

/**
 * Initializes a new module scope.
 * @class Represents root scope of the AsyncScript module.
 * @param {Boolean} dynamically The code located in this scope generated dynamically.
 */
function GlobalScope(dynamically){
	GenericScope.call(this, false);
	this.checked = true;
	if(this.dynamically = dynamically) this.locals["result"] = {};
}

GlobalScope.prototype = {
	get identifiers(){ return Object.keys(this.locals); },
	declareLocal: GenericScope.prototype.declareLocal,
	identifierOptions: GenericScope.prototype.identifierOptions
};

GlobalScope.prototype.translate = function(body, emitDebug){
	if(this.dynamically) body.unshift(new js.JSCode("result = new $asyncscript.Promise()"));
	Object.keys(this.locals).forEach(function(name){
		this.unshift(new js.JSVariableDecl(mangle(name)));	
	}, body);
	//begin bootstrapping
	if(this.dynamically) {
		body.push(new js.JSCode("return result"));
		body = new js.JSCall(new js.JSFunction([], body), 'call', new js.JSThis());
	}
	else {
		body.unshift(new js.JSCode("(function(){\n" +
			"var fs = require('fs'), path = require('path');\n" +
			"return require((fs.existsSync || path.existsSync)('./lib/main.js') ? fs.realpathSync('./lib/main.js') : 'asyncscript');\n"+
			"}).call(this); var DEBUG = " + (emitDebug ? true : false) + ", FILENAME = __filename"));
		body = new js.JSBlock(body);
	}
	//end bootstrapping
	return body;
};

function AccessorScope(accessor){
	GenericScope.call(this, true);
	switch(this.accessor = accessor){
		case "get": this.locals["result"] = {}; break;
		case "set": this.valueOptions = {name: 'value'}; break;
	}
}

AccessorScope.prototype = {
	get checked(){ return this.parent.checked; },
	get identifiers(){ return Object.keys(this.locals).concat(this.valueOptions ? [this.valueOptions.name] : []); }
};

AccessorScope.prototype.declareLocal = function(name){
	return this.valueOptions && name === this.valueOptions.name ? false : GenericScope.prototype.declareLocal.call(this, name);
};

AccessorScope.prototype.identifierOptions = function(name){
	return this.valueOptions && name === this.valueOptions.name ? this.valueOptions : GenericScope.prototype.identifierOptions.call(this, name);
};

AccessorScope.prototype.translate = function(body){
	//local variables
	Object.keys(this.locals).forEach(function(name){
		body.unshift(new js.JSVariableDecl(mangle(name)));
	});
	switch(this.accessor){
		case "get":
			//return result
			body.push(new js.JSReturn(new js.JSVariableRef("result")));
			return new js.JSFunction([], body);
		break;
		case "set":
			return new js.JSFunction(["value"], body);
		break;
	}
};

function RepeatScope(name){
	GenericScope.call(this, true);
	this.loopState = {'name': name};
	this.loopState.loopState = true;
}

RepeatScope.prototype = {
	get identifiers(){ return Object.keys(this.locals).concat([this.loopState.name]); },
	get checked(){ return this.parent.checked; },
	get loopStateName(){ return this.loopState.name; },
	loopScope: true
};

RepeatScope.prototype.declareLocal = function(name){
	return name === this.loopState.name ? false : GenericScope.prototype.declareLocal.call(this, name);
};

RepeatScope.prototype.identifierOptions = function(name){
	return name === this.loopState.name ? this.loopState : GenericScope.prototype.identifierOptions.call(this, name);
};

RepeatScope.prototype.translate = function(body, aggregator){
	Object.keys(this.locals).forEach(function(name){
		this.unshift(new js.JSVariableDecl(name));
	}, body);
	body = new js.JSFunction([this.loopState.name], body);
	body = new js.JSCall(body, "bind", new js.JSThis());
	return aggregator ? new js.JSCall(runtimeRef.value, "repeat", body, aggregator) : new js.JSCall(runtimeRef.value, "repeat", body);
};

function ForEachScope(loopVar){
	GenericScope.call(this, true);
	this.loopVar = {'name': loopVar.name};
	this.loopVar.contract = loopVar.contract;
}

ForEachScope.prototype = {
	get identifiers(){ return Object.keys(this).concat([this.loopVar.name]); },
	get checked(){ return this.parent.checked; },
	loopStateName: "__$state$__",
	loopScope: true
};

ForEachScope.prototype.declareLocal = function(name){
	return name === this.loopVar.name ? false : GenericScope.prototype.declareLocal.call(this, name);
};

ForEachScope.prototype.identifierOptions = function(name){
	return name === this.loopVar.name ? this.loopVar : GenericScope.prototype.identifierOptions.call(this, name);
};

ForEachScope.prototype.translate = function(source, body, aggregator){
	Object.keys(this.locals).forEach(function(name){
		this.unshift(new js.JSVariableDecl(name));
	}, body);
	body = new js.JSFunction([this.loopStateName, this.loopVar.name], body);
	body = new js.JSCall(body, "bind", new js.JSThis());
	return aggregator ? new js.JSCall(runtimeRef.value, "foreach", source, body, aggregator) : new js.JSCall(runtimeRef.value, "foreach", source, body);
};

function CatchScope(){
	GenericScope.call(this, true);
	this.error = {name: 'error'};
}

CatchScope.prototype = {
	get identifiers(){ return Object.keys(this.locals).concat([this.error.name]); },
	get checked(){ return this.parent.checked; }
};

CatchScope.prototype.identifierOptions = function(name){
	return this.error.name === name ? this.error : GenericScope.prototype.identifierOptions.call(this, name);
};

CatchScope.prototype.declareLocal = function(name){
	return this.error.name === name ? false : GenericScope.prototype.declareLocal.call(this, name);
};

CatchScope.prototype.translate = function(body){
	if(body instanceof Array && body.length > 1) body.unshift(new js.JSCode("value = new $asyncscript.Promise()"));
	//declare locals	
	Object.keys(this.locals).forEach(function(name){
		this.unshift(new js.JSVariableDecl(mangle(name)));
	}, body);
	return new js.JSCall(new js.JSFunction(["error"], body), "bind", new js.JSThis());
};

function ObjectScope(fields){
	GenericScope.call(this, true);
	(this.fields = fields).forEach(function(name){
		this[name] = {};
	}, this.locals);
}

ObjectScope.prototype = {
	get checked(){ return this.parent.checked; },
	get identifiers(){ return Object.keys(this.locals); }
};

ObjectScope.prototype.identifierOptions = GenericScope.prototype.identifierOptions;

ObjectScope.prototype.declareLocal = GenericScope.prototype.declareLocal;

ObjectScope.prototype.translate = function(source, body){
	//insert initialization for each field
	this.fields.forEach(function(f){
		this.unshift(new js.JSAssignment(mangle(f), "$asyncscript.getMember(__$src$__," + JSON.stringify(f) + ")"));
	}, body);	
	//declare locals	
	Object.keys(this.locals).forEach(function(name){
		this.unshift(new js.JSVariableDecl(mangle(name)));
	}, body);
	body = new js.JSFunction(["__$src$__"], body);
	return new js.JSCall(body, "call", new js.JSThis(), source);
};

/**
 * Creates a new checked/unchecked execution scope semantics.
 * @class Represents checked/unchecked execution semantics.
 * @param {Boolean} checked Specifies execution semantics.
 */
function ContextScope(checked){
	GenericScope.call(this, true);
	this.checked = checked;
}

ContextScope.prototype = {
	get identifiers(){ return Object.keys(this.locals); },
	identifierOptions: GenericScope.prototype.identifierOptions,
	declareLocal: GenericScope.prototype.declareLocal
};

ContextScope.prototype.translate = function(body){
	//local variables
	Object.keys(this.locals).forEach(function(name){
		body.unshift(new js.JSVariableDecl(mangle(name)));
	});
	body.unshift(new js.JSCode("$asyncscript.enterState($asyncscript.state.setChecked(" + this.checked + "))")); 
	body = new js.JSTryCatchFinally(body, null, new js.JSCode("$asyncscript.exitState()"));
	body = new js.JSScope(body);
	return new js.JSCall(body, "call", new js.JSThis());
};

var currentFunction = "callee", runtimeRef = {
	value: new js.JSVariableRef("$asyncscript"),
	getMember: function(obj, name){ return new js.JSCall(this.value, "getMember", obj, name); },
	setMember: function(obj, name, value){ return new js.JSCall(this.value, "setMember", obj, name, value); },
	overwrite: function(obj, name, value){ return new js.JSCall(this.value, "overwrite", obj, name, value); },
	typecast: function(op1, op2){ return new js.JSCall(this.value, "typecast", op1, op2); },
	binaryPlus: function(op1, op2){ return new js.JSCall(this.value, "binaryPlus", op1, op2); },
	binaryMinus: function(op1, op2){ return new js.JSCall(this.value, "binaryMinus", op1, op2); },
	multiplication: function(op1, op2){ return new js.JSCall(this.value, "multiplication", op1, op2); },
	xor: function(op1, op2){ return new js.JSCall(this.value, "xor", op1, op2); },
	division: function(op1, op2){ return new js.JSCall(this.value, "division", op1, op2); },
	and: function(op1, op2){ return new js.JSCall(this.value, "and", op1, op2); },
	or: function(op1, op2){ return new js.JSCall(this.value, "or", op1, op2); },
	greaterThan: function(op1, op2){ return new js.JSCall(this.value, "greaterThan", op1, op2); },
	lessThan: function(op1, op2){ return new js.JSCall(this.value, "lessThan", op1, op2); },
	greaterThanOrEqual: function(op1, op2){ return new js.JSCall(this.value, "greaterThanOrEqual", op1, op2); },
	lessThanOrEqual: function(op1, op2){ return new js.JSCall(this.value, "lessThanOrEqual", op1, op2); },
	shiftRight: function(op1, op2){ return new js.JSCall(this.value, "shiftRight", op1, op2); },
	shiftLeft: function(op1, op2){ return new js.JSCall(this.value, "shiftLeft", op1, op2); },
	modulo: function(op1, op2){ return new js.JSCall(this.value, "modulo", op1, op2); },
	instanceOf: function(op1, op2){ return new js.JSCall(this.value, "instanceOf", op1, op2); },
	contains: function(op1, op2){ return new js.JSCall(this.value, "contains", op1, op2); },
	areEqual: function(op1, op2){ return new js.JSCall(this.value, "areEqual", op1, op2); },
	areNotEqual: function(op1, op2){ return new js.JSCall(this.value, "areNotEqual", op1, op2); },
	assignment: function(op1, op2){ return new js.JSCall(this.value, "assignment", op1, op2); },
	redirectTo: function(op1, op2){ return new js.JSCall(this.value, "redirectTo", op1, op2); },
	invoke: function(method, args, destination){
		return destination ? new js.JSCall(this.value, "invoke", new js.JSUndefined(), method, args, destination) :
			new js.JSCall(this.value, "invoke", new js.JSUndefined(), method, args);	
	},
	invokeMethod: function(_this, method, args, destination){
		return destination ? new js.JSCall(this.value, "invokeMethod", _this, method, args, destination) :
			new js.JSCall(this.value, "invokeMethod", _this, method, args);	
	},
	getItem: function(target, indicies, destination){
		return destination ? new js.JSCall(this.value, "getItem", target, indicies, destination):
		new js.JSCall(this.value, "getItem", target, indicies);
	},
	setItem: function(target, value, indicies, destination){
		return destination ? new js.JSCall(this.value, "setItem", target, value, indicies, destination):
			new js.JSCall(this.value, "setItem", target, value, indicies);
	},
	ret: function(result, destination){
		return new js.JSCall(this.value, "ret", result, destination);
	},
	fault: function(result, destination){
		return new js.JSCall(this.value, "fault", result, destination);	
	},
	newContainerContract: function(properties){
		properties.fields["$PROTOTYPE"] = "$asyncscript.containerContract.base";
		return properties;
	},
	newContainer: function(properties){
		return new js.JSCall(
			new js.JSVariableRef("Object"), 
			"create", 
			new js.JSCode("$asyncscript.container.base"),
			properties
		);	
	},
	binding: function(value, contract){
		return new js.JSCall(this.value, "binding", value, contract)
	},
	newProperty: function(getter, setter, cached, contract, _this){
		return _this ? new js.JSNew(new js.JSMemberAccess(this.value, "Property"), getter, setter, cached ? "true" : "false", contract, _this) :
			new js.JSNew(new js.JSMemberAccess(this.value, "Property"), getter, setter, cached ? "true" : "false", contract);
	},
	breakpoint: function(context, name, position){
		return new js.JSCall(this.value, "breakpoint", context, name, position);
	},
	resolveName: function(name, position){
		return position ? new js.JSCall(this.value, "resolveName", name, "__filename", position) : new js.JSCall(this.value, "resolveName", name);	
	},
	'continue': function(state, args){
		return new js.JSCall(this.value, "continueWith", state, args);
	},
	'break': function(state, args){
		return new js.JSCall(this.value, "breakWith", state, args);
	},
	tryCatchFinally: function(t, c, f){
		return f ? new js.JSCall(this.value, "tryCatchFinally", t, c, f) : new js.JSCall(this.value, "tryCatch", t, c);
	},
	select: function(value, comparer, values, def){
		return def ? new js.JSCall(this.value, "select", value, comparer || new js.JSUndefined(), values, new js.JSNumberLiteral(0), def) : new js.JSCall(this.value, "select", value, comparer || new js.JSUndefined(), values, new js.JSNumberLiteral(0));
	}
};

function FunctionScope(oneWay, parameters){
	GenericScope.call(this, true);
	parameters.forEach(function(name){
		name = this[name] = {};
		name.synchronized = name.parameter = true;
	}, this.parameters = new Object());
	if(!(this.oneWay = oneWay)) this.locals["result"] = {};
	this.locals["callee"] = {};
}

FunctionScope.prototype = {
	get checked(){ return this.parent.checked; },
	get identifiers(){ return Object.keys(this.locals).concat(Object.keys(this.parameters)); }
};

FunctionScope.prototype.declareLocal = function(name){
	if(this.locals[name]) return;
	else if(this.parameters[name]) return;
	else return this.locals[name] = {};
};

FunctionScope.prototype.identifierOptions = function(name, checker){ 
	return this.locals[name] || this.parameters[name];
};

FunctionScope.prototype.translate = function(body, signature){
	if(!this.oneWay) delete this.locals.result;
	delete this.locals.callee;
	//local variables
	Object.keys(this.locals).forEach(function(name){
		//put variable declaration
		this.unshift(new js.JSVariableDecl(mangle(name)));
	}, body);
	//synchronize each parameter and supplies binding
	Object.keys(this.parameters).reverse().forEach(this.oneWay ?
	function(name, idx){
		name = mangle(name);
		body.unshift(new js.JSCode("if(" + name + " instanceof $asyncscript.Promise)" +
			"if(" + name + ".isError) throw " + name + ".result;"+
			"else if(" + name + ".isCompleted)" + name + " = " + name + ".result;" +
			"else return ($asyncscript.fork(function(arg){" +
			"  this.args[" + idx + "] = arg;" +
			"  return this.method.apply(this['this'], this.args);" +
			"}.bind({'this': this, args: arguments, method: callee}), " + name + "), result);" +
			name + " = " + "$asyncscript.binding(" + name + ", callee['__$contract$__'][" + idx + "]);"
		));
	} :
	function(name, idx){
		name = mangle(name);
		body.unshift(new js.JSCode("if(" + name + " instanceof $asyncscript.Promise)" +
			"if(" + name + ".isError) {$asyncscript.fault(" + name + ", result); throw " + name + ".result;}"+
			"else if(" + name + ".isCompleted)" + name + " = " + name + ".result;" +
			"else return ($asyncscript.fork(function(arg){" +
			"  this.args[" + idx + "] = arg;" +
			"  $asyncscript.prepareLambdaInvocation(result);" +
			"  return this.method.apply(this['this'], this.args);" +
			"}.bind({'this': this, args: arguments, method: callee}), " + name + "), result);" +
			name + " = " + "$asyncscript.binding(" + name + ", callee['__$contract$__'][" + idx + "]);"
		));
	}, this.parameters);
	//initialization of the current function and save the destination
	if(this.oneWay){
		body.unshift(new js.JSCode("var callee = arguments.callee; $asyncscript.enterLambdaBody();"));	
		body.push(new js.JSCode("return null;"));
	}
	else{
		body.unshift(new js.JSCode("var callee = arguments.callee, result = $asyncscript.enterLambdaBody();"));
		//return value
		body.push(new js.JSCode("if(result instanceof $asyncscript.Promise)" +
								"if(result.isError) throw result.result;" + 
								"else if(result.isCompleted) result = result.result;" +
								"return result;"));
	}
	body = new js.JSFunction(Object.keys(this.parameters).map(mangle), body);	
	signature.unshift(this.oneWay ? "true" : "false");
	return new js.JSCall(body, "toLambda", signature);
};

/**
 * Initializes a new translation context.
 */
function TranslationContext(root){
	this.scope = root;
	//set of bootstrapper code
	this.bootstrapper = {};
}

TranslationContext.prototype = {
	get checked(){ return this.scope.checked; },
	get root(){ return this.scope.parent ? false : true; }
};

TranslationContext.prototype.registerBootstrapper = function(name, code){
	if(name in this.bootstrapper) return false;
	this.bootstrapper[name] = code;
	return true; 
};

TranslationContext.prototype.getVisibleIdentifiers = function(temporary){
	var lookup = this.scope, result = {};
	while(lookup){
		var ids = lookup.identifiers;
		ids.forEach(function(name){
			if(name in this) return;
			var options = lookup.identifierOptions(name);
			if(options.temporary && !temporary) return;
			this[name] = options;
		}, result);
		if(lookup.transient) lookup = lookup.parent; else break;
	}
	return result;
};

TranslationContext.prototype.identifierOptions = function(name, checker){
	var lookup = this.scope, options;
	while(lookup && !options){
		options = lookup.identifierOptions(name);
		if(lookup.transient) lookup = lookup.parent; else break;
	}
	return options && checker instanceof Function ? checker(options) : options;
};

TranslationContext.prototype.findScope = function(scope, condition){
	var lookup = this.scope;
	while(lookup)
		if(lookup instanceof scope && (condition ? condition(lookup) : true)) return lookup;
		else if(lookup.transient) lookup = lookup.parent;
		else break;
}

TranslationContext.prototype.declareLocal = function(name, setter){
	var options = this.scope.declareLocal(name);
	if(options)
		if(setter instanceof Function) setter(options);
		else if(setter instanceof Object) Object.keys(setter).forEach(function(key){
			options[key] = this[key];		
		}, setter);
	return options;
};

TranslationContext.prototype.pushScope = function(scope){
	scope.parent = this.scope;
	this.scope = scope;
	return scope;
};

TranslationContext.prototype.popScope = function(){
	var currentScope = this.scope;
	this.scope = currentScope.parent;
	return currentScope;
};

//================================================================================================

/**
 * Initializes a new preprocessor.
 * @param {SyntaxAnalyzer} parser An instance of syntax analyzer.
 * @param {TranslationContext} context Translation context.
 * @param {Boolean} emitDebug true for compilation in debug mode; otherwise, false.
 */
function Preprocessor(parser, context, emitDebug){
	this.parser = parser;
	this.context = context;
	this.emitDebug = emitDebug;
}

Preprocessor.prototype.loadExtension = function(filename, parsed){
	if(parsed){	//attempts to load extension module
		var path = require('path'), fs = require('fs');
		filename = [filename, path.join(__dirname, '../extensions', filename, 'main'), path.join(__dirname, '../extensions', filename), path.join(process.cwd(),  filename), path.join(process.cwd(),  filename, 'main')];
		return filename.some(function(filename){
			if((fs.existsSync || path.existsSync)(filename)) return (require(filename), true);
			else if((fs.existsSync || path.existsSync)(filename += '.js')) return (require(filename), true);
			else return false;
		});
	}	
	else {		//parse directive
		filename = filename.split(" ");
		return filename.length === 2 ? this.loadExtension(filename[1], true) : false;
	}
};

Preprocessor.prototype.execute = function(directive){
	if(directive.indexOf("extension") === 0) return this.loadExtension(directive, false);
	else return false;
};

//================================================================================================

/**
 * Initializes a new AST-to-JavaScript translator.
 * @param {String|SyntaxAnalyzer} source Source code parser.
 * @param {Object} root A root lexical scope. Optional.
 */
function ScriptTranslator(source, root){
	//translates single AST node
	if(source && source.nodeType) this.parser = {
		get column(){ return source.position.column; },
		get line(){ return source.position.line; },
		aborted: false,
		next: function(callback){ 
			if(this.aborted) return;
			this.aborted = true;
			return callback(source);
		}
	};
	else if(source instanceof SyntaxAnalyzer) this.parser = source;
	else{
		source = new SyntaxAnalyzer(source);
		source.constructor = SyntaxAnalyzer;
		this.parser = source;		
	};
	this.context = new TranslationContext(root || new GlobalScope());
}

//compile nodes

ScriptTranslator.expressions = function(set, translator){
	if(arguments.length === 3){
		var emitDebug = arguments[2];
		return this.expressions(set, function(expr){
			return this.translate(expr, translator/*means context*/, emitDebug);
		});
	}
	var result = new Array(set.length);
	for(var i = 0; i < set.length; i++){
		var expr = translator.call(this, set[i], i);
		if(expr instanceof ParserError) return expr;
		else result[i] = expr;
	}
	return result;
};

//unary expression
ScriptTranslator.unaryExpression = function(expression, context, emitDebug){
	var methodName;
	switch(expression.operator.value){
		case '$': methodName = "contractOf"; break;
		case '+': methodName = "unaryPlus"; break;
		case '-': methodName = "unaryMinus"; break;
		case '**': methodName = "square"; break;
		case '!': methodName = "negation"; break;
		case "++": methodName = "increment"; break;
		case "--": methodName = "decrement"; break;
		default: return;
	}
	var operand = this.translate(expression.operand, context, emitDebug); //operand
	if(operand instanceof ParserError) return operand;
	return new js.JSCall(runtimeRef.value, 
		methodName, 
		operand, //operand
		new js.JSBooleanLiteral(expression.operator.style === "postfix")	//is postfixed operator ?
	);
};

//variable reference
ScriptTranslator.identifier = function(expression, context, emitDebug){
	var options = context.identifierOptions(expression.name), mangledName = mangle(expression.name);
	if(options === undefined)
		return mangledName in global? new js.JSVariableRef(mangledName):
		ParserError.undeclaredIdentifier(expression.name, expression.position.column, expression.position.line);
	else if(options.value === null) return new js.JSNull();
	else if(options.value instanceof ast.CodeBuiltInContractExpression && !options.contract)
		return this.contractRef(options.value);	
	else switch(options.value && options.value.constructor){
		case Number: return new js.JSNumberLiteral(options.value); 
		case String: return new js.JSStringLiteral(options.value); 
		case Boolean: return new js.JSBooleanLiteral(options.value);
		default: return new js.JSVariableRef(mangle(expression.name));
	}
};

//binary operator
ScriptTranslator.binaryExpression = function(expression, context, emitDebug){
	var target, fieldName, value, left, right;
	//assignment through indexer
	if(expression.left instanceof ast.CodeIndexerExpression && expression.operator.value === '='){
		target = this.translate(expression.left.target, context, emitDebug);
		if(target instanceof ParserError) return target;
		var indicies = this.expressions(expression.left.indicies, context, emitDebug);
		if(indicies instanceof ParserError) return indicies;
		var destination = expression.left.destination ? this.translate(expression.left.destination, context, emitDebug) : undefined;
		if(destination instanceof ParserError) return destination;
		value = this.translate(expression.right, context, emitDebug);
		if(value instanceof ParserError) return value;
		return runtimeRef.setItem(target, value, new js.JSNewArray(indicies), destination);
	}
	var method;
	switch(expression.operator.value){
		case '&&':
		case '||':
			target = new Array();
			//parse each operand
			expression.operands.every(function(op, idx){
				op = this.translate(op, context, emitDebug);
				if(op instanceof ParserError) return target = op, false;
				var tempname = "$" + idx;
				this.push(new js.JSVariableDecl(tempname));
				//is operand synchronized ?
				var src = "if(arguments.length > " + idx + ") " + tempname + " = " + "arguments[" + idx + "];"+
				"else{" +
				tempname + " = " + op + ";" +
				"if(" + tempname + " instanceof $asyncscript.Property)" + tempname + " = " + tempname + ".value;" +
				"if(" + tempname + " instanceof $asyncscript.Promise)"+
					"if(" + tempname + ".isError) throw " + tempname + ".result;" +
					"else if(" + tempname + ".isCompleted) " + tempname + " = " + tempname + ".result;" +
					"else return $asyncscript.fork(function(op){ return this.fn.call(this['this'], ";
				//pass already synchronized arguments
				for(var i = 0; i <= idx; i++) src += i === idx ? "op":  "$" + i + ", ";
				src += "); }.bind({fn: arguments.callee, 'this': this}), " + tempname + ");";
				src += "}";	//closes first if
				this.push(new js.JSCode(src));
				//check on true
				if(expression.operator.value === '||')
					this.push(new js.JSCode('if(' + tempname + ') return ' + tempname + ';'));
				else //check on false
					this.push(new js.JSCode('if(!' + tempname + ') return null;')); 
				return true;
			}, this);
			return target instanceof ParserError ? target : new js.JSCall(new js.JSFunction([], target), "call", new js.JSThis());
		case '===':
		case '!==':
			left = this.translate(expression.left, context, emitDebug);
			if(left instanceof ParserError) return left;
			right = this.translate(expression.right, context, emitDebug);
			if(right instanceof ParserError) return right;
			return new js.JSBinaryOperator(left, expression.operator.value, right);
		case ".":
			if(expression.right instanceof ast.CodeIdentifierExpression) 
				right = new js.JSStringLiteral(expression.right.name);
			else if(expression.right instanceof ast.CodeStringExpression)
				right = new js.JSStringLiteral(expression.right.value);
			else {
				right = this.translate(expression.right, context, emitDebug);
				if(right instanceof ParserError) return right;
			}
			left = this.translate(expression.left, context, emitDebug);
			return left instanceof ParserError ? left : runtimeRef.getMember(left, right);
		case '+': method = "binaryPlus"; break;
		case '-': method = "binaryMinus"; break;
		case "*": method = "multiplication"; break;
		case '^': method = "xor"; break;
		case '/': method = "division"; break;
		case "&": method = "and"; break;
		case '|': method = "or"; break;
		case ">": method = "greaterThan"; break;
		case "<": method = "lessThan"; break;
		case ">=": method = "greaterThanOrEqual"; break;
		case "<=": method = "lessThanOrEqual"; break;
		case ">>": method = "shiftRight"; break;
		case "<<": method = "shiftLeft"; break;
		case "%": method = "modulo"; break;
		case "is": method = "instanceOf"; break;
		case "to": method = "typecast"; break;
		case "in": method = "contains"; break;
		case "==": method = "areEqual"; break;
		case "!=": method = "areNotEqual"; break;
		case "=>": method = "redirectTo"; break;
		case "with": 
			left = this.translate(expression.left, context, emitDebug);
			if(left instanceof ParserError) return left;
			right = this.translate(expression.right, context, emitDebug);
			if(right instanceof ParserError) return right;
			return runtimeRef.invokeMethod(left, new js.JSStringLiteral("with"), new js.JSNewArray(right));
		case ':=': 
			//overwrite member
			if(expression.left instanceof ast.CodeBinaryExpression && expression.left.operator.value === '.'){
				target = this.translate(expression.left.left, context, emitDebug);
				if(target instanceof ParserError) return target;
				//cast field name to string, if it is necessary.
				if(expression.left.right instanceof ast.CodeIdentifierExpression) 
					fieldName = new js.JSStringLiteral(expression.left.right.name);
				else if(expression.left.right instanceof ast.CodeStringExpression){
					fieldName = this.translate(expression.left.right, context, emitDebug);
					if(fieldName instanceof ParserError) return fieldName;
				}
				else {
					fieldName = this.translate(expression.left.right, context, emitDebug);
					if(fieldName instanceof ParserError) return fieldName;
					fieldName = runtimeRef.typecast(fieldName, new js.JSVariableRef("String"));
				}
				value = this.translate(expression.right, context, emitDebug);
				if(value instanceof ParserError) return value;
				return runtimeRef.overwrite(target, fieldName, value);
			}
		case "=":
			//member assignment
			if(expression.left instanceof ast.CodeBinaryExpression && expression.left.operator.value === '.'){
				target = this.translate(expression.left.left, context, emitDebug);
				if(target instanceof ParserError) return target;
				//cast field name to string, if it is necessary.
				if(expression.left.right instanceof ast.CodeIdentifierExpression) 
					fieldName = new js.JSStringLiteral(expression.left.right.name);
				else if(expression.left.right instanceof ast.CodeStringExpression){
					fieldName = this.translate(expression.left.right, context, emitDebug);
					if(fieldName instanceof ParserError) return fieldName;
				}
				else {
					fieldName = this.translate(expression.left.right, context, emitDebug);
					if(fieldName instanceof ParserError) return fieldName;
					fieldName = runtimeRef.typecast(fieldName, new js.JSVariableRef("String"));
				}
				value = this.translate(expression.right, context, emitDebug);
				if(value instanceof ParserError) return value;
				return runtimeRef.setMember(target, fieldName, value);
			}
			//identifier assignment
			else method = "assignment";
			break;
		default: return;
	}
	left = this.translate(expression.left, context, emitDebug);
	if(left instanceof ParserError) return left;
	right = this.translate(expression.right, context, emitDebug);
	if(right instanceof ParserError) return right;
	return runtimeRef[method](left, right);
};

//integer literal
ScriptTranslator.integerLiteral = function(expression){
	return new js.JSNumberLiteral(expression.value);
};

//boolean literal
ScriptTranslator.booleanLiteral = function(expression){
	return new js.JSBooleanLiteral(expression.value);
};

//integer literal
ScriptTranslator.realLiteral = ScriptTranslator.integerLiteral;

//contract reference
ScriptTranslator.contractRef = function(expression){
	switch(expression.value){
		case "void": return new js.JSNull();
		case "real": return new js.JSVariableRef("Number");
		case "boolean": return new js.JSVariableRef("Boolean");
		case "string": return new js.JSVariableRef("String");
		case "object": return new js.JSVariableRef("Object");
		case "integer": return new js.JSMemberAccess(runtimeRef.value, "integer");
		case "regexpr": return new js.JSVariableRef("RegExp");
		case "expression": return new js.JSMemberAccess(runtimeRef.value, "Expression");
		case "typedef": return new js.JSMemberAccess(runtimeRef.value, "typedef");
		case "function": return new js.JSVariableRef("Function");
	}
};

//invocation expression
ScriptTranslator.invocationExpression = function(expression, context, emitDebug){
	var self = expression.self ? this.translate(expression.self, context, emitDebug) : null;
	if(self instanceof ParserError) return self;
	var method = self && expression.method.name ? new js.JSStringLiteral(mangle(expression.method.name)) : this.translate(expression.method, context, emitDebug);
	if(method instanceof ParserError) return method;
	var destination = expression.destination ? this.translate(expression.destination, context, emitDebug) : undefined;
	if(destination instanceof ParserError) return destination;
	var arguments = this.expressions(expression.arguments, context, emitDebug);
	if(arguments instanceof ParserError) return arguments;
	//compiling invocation
	return self ? runtimeRef.invokeMethod(self, method, new js.JSNewArray(arguments), destination) : runtimeRef.invoke(method, new js.JSNewArray(arguments), destination);
};

//indexer
ScriptTranslator.indexerExpression = function(expression, context, emitDebug){
	var target = this.translate(expression.target, context, emitDebug);
	if(target instanceof ParserError) return target;
	var indicies = this.expressions(expression.indicies, context, emitDebug);
	if(indicies instanceof ParserError) return indicies;
	var destination = expression.destination ? this.translate(expression.destination, context, emitDebug) : undefined;
	if(destination instanceof ParserError) return destination;
	return runtimeRef.getItem(target, new js.JSNewArray(indicies), destination);
};

//translates getter
function translateGetter(getter, context, emitDebug){
	if(!getter) return new js.JSNull();
	context.pushScope(new AccessorScope("get"));
	//parse getter implementation
	if(getter instanceof Array) {
		getter = this.expressions(getter, context, emitDebug);
		if(getter instanceof ParserError) return getter;
	}
	else{
		getter = this.translate(getter, context, emitDebug);
		if(getter instanceof ParserError) return getter;
		getter = [new js.JSAssignment(new js.JSVariableRef("result"), getter)];
	}
	return context.popScope().translate(getter);
}

//translates setter
function translateSetter(setter, context, emitDebug){
	if(!setter) return new js.JSNull();
	context.pushScope(new AccessorScope("set"));
	//parse getter implementation
	if(setter instanceof Array) {
		setter = this.expressions(setter, context, emitDebug);
		if(setter instanceof ParserError) return setter;
	}
	else {
		setter = this.translate(setter, context, emitDebug);
		if(setter instanceof ParserError) return setter;
		setter = [setter];
	}
	return context.popScope().translate(setter);
}

//identifier declaration
ScriptTranslator.letExpression = function(expression, context, emitDebug){
	var options = {}, value = expression.value, result;
	var contract = expression.contract && !(expression.contract instanceof ast.CodeBuiltInContractExpression && expression.contract.value === "object") ? this.translate(expression.contract, context, emitDebug) : undefined;
	if(contract instanceof ParserError) return contract;
	options.contract = expression.contract;	//save the type of the variable
	if(expression.get || expression.set){	//reactive value
		//creates a new variable with getter or/and setter
		var getter = translateGetter.call(this, expression.get, context, emitDebug);
		if(getter instanceof ParserError) return getter;
		var setter = translateSetter.call(this, expression.set, context, emitDebug, true);
		if(setter instanceof ParserError) return setter;
		result = new js.JSAssignment(
			new js.JSVariableRef(mangle(expression.name)),
			contract ?
				runtimeRef.newProperty(getter, setter, expression.isCached, contract, new js.JSThis()):
				runtimeRef.newProperty(getter, setter, expression.isCached, new js.JSUndefined(), new js.JSThis())
		);
	}
	//generic compilation
	else {
		value = this.translate(value, context, emitDebug);
		result = value instanceof ParserError ? value : new js.JSAssignment(
			new js.JSVariableRef(mangle(expression.name)),
			contract ? runtimeRef.binding(value, contract) : value
		);
	}
	//declares a new identifier
	return context.declareLocal(expression.name, options) ? result : ParserError.duplicatedIdentifier(expression.name, expression.position.column, expression.position.line);
};

ScriptTranslator.signatureExpression = function(expression, context, emitDebug){
	var parameters = {};
	for(var i = 0; i < expression.parameters.length; i++){
		var p = expression.parameters[i];
		if(parameters[p.name]) return ParserError.duplicatedIdentifier(p.name, p.position.column, p.position.line);
		p = parameters[p.name] = p.contract && !(p.contract instanceof ast.CodeBuiltInContractExpression && p.contract.value === "object") ? this.translate(p.contract, context, emitDebug) : new js.JSUndefined();
		if(p instanceof ParserError) return p;
	}
	var signature = Object.keys(parameters).map(function(name){ 
		return this[name];  
	}, parameters);
	signature.unshift(expression.oneWay ? "true" : "false");
	return new js.JSNew(new js.JSMemberAccess(runtimeRef.value, 'Signature'), signature);
};

ScriptTranslator.functionExpression = function(expression, context, emitDebug){
	//compile function
	if(expression.implementation){
		var parameters = {};
		for(var i = 0; i < expression.parameters.length; i++){
			var p = expression.parameters[i];
			if(parameters[p.name]) return ParserError.duplicatedIdentifier(p.name, p.position.column, p.position.line);
			p = parameters[p.name] = p.contract === undefined ? new js.JSVariableRef("Object") : this.translate(p.contract, context, emitDebug);
			if(p instanceof ParserError) return p;
		}
		delete parameters.result;
		context.pushScope(new FunctionScope(expression.oneWay, Object.keys(parameters)));
		//traslates body
		var body; 
		if(expression.implementation instanceof Array){
			body = this.expressions(expression.implementation, context, emitDebug);
			if(body instanceof ParserError) return body;
		}
		else{
			body = this.translate(expression.implementation, context, emitDebug);
			if(body instanceof ParserError) return body;
			body = expression.oneWay ? [body] : [runtimeRef.ret(body, new js.JSVariableRef("result"))];
		}
		//translates binding for each parameter
		var signature = Object.keys(parameters).map(function(p){ 
			p = this[p]; 
			return p ? new js.JSVariableRef("Object") : p;
		}, parameters);
		return context.popScope().translate(body, signature);
	}
	//compile signature
	else return this.signatureExpression(expression, context, emitDebug);	
};

ScriptTranslator.quotedExpression = function(expression, context, emitDebug){
	return new js.JSNew(new js.JSMemberAccess(runtimeRef.value, "Expression"), new js.JSCode(JSON.stringify(expression.tree))); 
};

ScriptTranslator.block = function(expressions, context, emitDebug, declareLocals){
	if(!(expressions instanceof Array)) return this.block([expressions], context, emitDebug, declareLocals);
	else if(expressions.length === 1){
		expressions = this.translate(expressions[0], context, emitDebug);
		if(expressions instanceof ParserError) return expressions;
		expressions = [new js.JSReturn(expressions)];
	}
	else{
		context.scope.declareLocal("value");	//stores scope value
		expressions = this.expressions(expressions, context, emitDebug);
		if(expressions instanceof ParserError) return expressions;
		//assign scope value
		expressions.unshift(new js.JSCode("value = new $asyncscript.Promise()"));
		//return scope value
		expressions.push(new js.JSCode("if(value instanceof $asyncscript.Promise)" +
								"if(value.isError) throw result.result;" + 
								"else if(value.isCompleted) value = value.result;" +
								"return value;"));
	}
	//declare locals
	if(declareLocals) Object.keys(context.scope.locals).forEach(function(name){
		this.unshift(new js.JSVariableDecl(name));
	}, expressions);
	return expressions;
}

ScriptTranslator.scopeExpression = function(expression, context, emitDebug){
	context.pushScope(new GenericScope(true));
	expression = this.block(expression, context, emitDebug, true);
	context.popScope();
	return expression instanceof ParserError ? expression : new js.JSCall(new js.JSScope(expression), "call", new js.JSThis());
};

ScriptTranslator.directiveOrString = function(expression, context, emitDebug){
	var pre = new Preprocessor(this.parser, context, emitDebug);
	var result = pre.execute(expression.value);
	return pre.execute(expression.value) ? null : this.translate(expression, context, emitDebug);
};

ScriptTranslator.breakpointExpression = function(expression, context, emitDebug){
	if(!emitDebug) return null;
	//creates a context that references all declared identifiers
	var ctx = {}, identifiers = context.getVisibleIdentifiers();
	Object.keys(identifiers).forEach(function(name){
		var options = this[name];
		ctx[name] = new js.JSVariableRef(mangle(name));
	}, identifiers);
	ctx['this'] = new js.JSThis();
	return runtimeRef.breakpoint(new js.JSObject(ctx), new js.JSStringLiteral(expression.name), new js.JSObject(expression.position));
};

ScriptTranslator.stringExpression = function(expression){
	return new js.JSStringLiteral(expression.value);
};

ScriptTranslator.containerExpression = function(expression, context, emitDebug){
	//creates `with` transformation method
	expression = function(){
		var fields = [],
		//select only 
		transformations = this.fields.filter(function(f){
			if(f instanceof ast.CodeBinaryExpression && f.operator.value === 'with' || f.value instanceof ast.CodeBinaryExpression && f.value.operator.value === 'with')
				return true;
			else {
				this.push(f);
				return false;
			}
		}, fields)
		.map(function(f, i, array){
			var context;
			if(f instanceof ast.CodeBinaryExpression){
				context = f.right;
				f = f.left;
			}
			else {
				context = f.value.right;
				f.value = f.value.left;
			}
			var transformation = new ast.CodeLetExpression(this.column, this.line);
			transformation.name = "t" + i;
			//__ctx__ is <context> ? <_then> : <_else>
			var _then, _else, previous = new ast.CodeIdentifierExpression("t" + (i - 1), this.column, this.line);
			switch(i){
				//initial transform
				case 0: 
					_then = new ast.CodeContainerExpression(this.column, this.line, [f]);
					_else = new ast.CodeContainerExpression(this.column, this.line, []);
				break;
				case array.length - 1:
					transformation = new ast.CodeReturnExpression(this.column, this.line);
				default:
					_then = new ast.CodeBinaryExpression(
						previous,
						ast.parseOperator("+", true),
						new ast.CodeContainerExpression(this.column, this.line, [f]),
						this.column,
						this.line);
					_else = previous;
				break;
			}
			transformation.value = new ast.CodeConditionalExpression(
				//condition
				new ast.CodeBinaryExpression(
					new ast.CodeIdentifierExpression("_ctx_", this.column, this.line),
					ast.parseOperator('is', true),
					context,
					this.column,
					this.line
				),
				this.column,
				this.line);
			transformation.value['then'] = _then;
			transformation.value['else'] = _else;
			return array.length > 1 ? transformation : transformation.value;
		}, this);
		transformations.toString = function(){
			return "{" + this.join(";") + "}";
		};
		//creates `with` method if it is necessary
		if(transformations.length > 0){
			transformations = new ast.CodeFunctionExpression(this.column, this.line, [{name: "_ctx_"}], transformations.length === 1 ? transformations[0] : transformations);
			transformations = new ast.CodeLetExpression(this.column, this.line, transformations);
			transformations.name = "with";
			fields.push(transformations);
		}
		//set stable fields
		this.fields = fields;
		return this;
	}.call(expression);
	if(expression.fields.length === 0) return new js.JSMemberAccess(new js.JSMemberAccess(runtimeRef.value, "container"), "empty");
	//compiles container
	var result = {"__$contract$__": {
			"__$size$__": new js.JSNumberLiteral(expression.fields.length),
			"__$contracts$__": {}
		},
		"__$size$__": new js.JSObject({
			enumerable: false,
			configurable: false,
			get: new js.JSCode("$asyncscript.container.sizeAccessor")
		})
	};
	expression.fields.every(function(f, idx){
		//named slot
		var contract = new js.JSVariableRef("Object"), value;
		if(f instanceof ast.CodeLetExpression){
			contract = f.contract && !(f.contract instanceof ast.CodeBuiltInContractExpression && f.contract.value === "object") ?
				this.translate(f.contract, context, emitDebug):
				new js.JSVariableRef("Object");
			if(contract instanceof ParserError) return result = contract, false;
			//reactive			
			if(f.get || f.set) {
				var getter = translateGetter.call(this, f.get, context, emitDebug),
					setter = translateSetter.call(this, f.set, context, emitDebug);
				if(getter instanceof ParserError) return result = getter, false;
				else if(setter instanceof ParserError) return result = setter, false;
				else value = runtimeRef.newProperty(getter, setter, f.isCached, contract);
			} else {
				value = this.translate(f.value, context, emitDebug);
				if(value instanceof ParserError) return result = value, false;
			}
			//field
			result[f.name] = new js.JSObject({
				configurable: new js.JSBooleanLiteral(false), 
				writable: new js.JSBooleanLiteral(false),
				enumerable: new js.JSBooleanLiteral(true),
				value: value});
			result[idx] = new js.JSObject({
				configurable: new js.JSBooleanLiteral(false),
				enumerable: new js.JSBooleanLiteral(false), 
				get: new js.JSCode("function(){ return this['" + f.name + "']; }")
			});
			result.__$contract$__[f.name] = new js.JSNumberLiteral(idx);
			result.__$contract$__[idx] = new js.JSStringLiteral(f.name);
		} 
		else {
			//detects contract through TO operator
			if(f instanceof ast.CodeBinaryExpression && f.operator.value === "to"){
				value = this.translate(f.left, context, emitDebug);
				contract = this.translate(f.right, context, emitDebug);
			}
			else value = this.translate(f, context, emitDebug);
			if(value instanceof ParserError) return result = value, false;
			result[idx] = new js.JSObject({
				configurable: new js.JSBooleanLiteral(false), 
				writable: new js.JSBooleanLiteral(false),
				enumerable: new js.JSBooleanLiteral(false),
				value: value});
		}
		//assigns contract of the field to the container contract			
		result.__$contract$__["__$contracts$__"][idx] = contract;
		return true;
	}, this);
	if(result instanceof ParserError) return result;
	result.__$contract$__["__$contracts$__"] = new js.JSObject(result.__$contract$__["__$contracts$__"]);
	result.__$contract$__ = new js.JSObject({
		value: runtimeRef.newContainerContract(new js.JSObject(result.__$contract$__)),
		configurable: new js.JSBooleanLiteral(false), 
		writable: new js.JSBooleanLiteral(false),
		enumerable: false,
	});
	return runtimeRef.newContainer(new js.JSObject(result));
};

ScriptTranslator.arrayExpression = function(expression, context, emitDebug){
	expression = this.expressions(expression.elements, context, emitDebug);
	return expression instanceof ParserError ? expression : new js.JSNewArray(expression);
};

ScriptTranslator.containerContractExpression = function(expression, context, emitDebug){
	var fields = {
		"__$size$__":new js.JSNumberLiteral(expression.fields.length),
		"__$contracts$__": {}
	};
	expression = this.expressions(expression.fields, function(def, idx){
		var contract = def.contract && !(def.contract instanceof ast.CodeBuiltInContractExpression && def.contract.value === "object") ?
			this.translate(def.contract, context, emitDebug):
			new js.JSVariableRef("Object");
		if(contract instanceof ParserError) return contract;
		fields.__$contracts$__[idx] = contract;
		if(def.name){
			fields[def.name] = new js.JSNumberLiteral(idx);
			fields[idx] = new js.JSStringLiteral(def.name);
		}
	});
	if(expression instanceof ParserError) return expression;
	fields.__$contracts$__ = new js.JSObject(fields.__$contracts$__);
	return runtimeRef.newContainerContract(new js.JSObject(fields));
};

ScriptTranslator.regularExpression = function(expression){
	return new js.JSNew(new js.JSVariableRef("RegExp"), new js.JSStringLiteral(expression.value));
};

ScriptTranslator.arrayContract = function(contract, context, emitDebug){
	contract = this.translate(contract.element, context, emitDebug);
	return contract instanceof ParserError ? contract : new js.JSNew(new js.JSMemberAccess(runtimeRef.value, "ArrayContract"), contract);
};

ScriptTranslator.scopeRef = function(expression, context, emitDebug){
	switch(expression = expression.scope){
		case "this": return new js.JSThis();
		case "global": return "global";
		default: throw "Unexpected scope value";
	}
};

ScriptTranslator.contextExpression = function(expression, context, emitDebug){
	context.pushScope(new ContextScope(expression.checked));
	expression = expression.expr;
	expression = this.block(expression instanceof ParserError ? expression : [expression], context, emitDebug, true);
	if(expression instanceof ParserError) return expression;
	return context.popScope().translate(expression);
};

ScriptTranslator.withExpression = function(expression, context, emitDebug){
	var source = this.translate(expression.source, context, emitDebug);	
	if(source instanceof ParserError) return source;	
	context.pushScope(new ObjectScope(expression.fields));	
	expression = expression.body;
	expression = this.block(expression instanceof Array ? expression : [expression] , context, emitDebug);
	if(expression instanceof ParserError) return expression;
	return context.popScope().translate(source, expression);
};

ScriptTranslator.returnExpression = function(expression, context, emitDebug){
	var value = this.translate(expression.value, context, emitDebug);
	if(value instanceof ParserError) return value;
	var destination;	
	if(expression.destination){	//destination explicitly defined
		destination = this.translate(expression.destination, context, emitDebug);
		if(destination instanceof ParserError) return destination;
		return runtimeRef.ret(value, destination);
	}
	//explore result
	else if(destination = context.identifierOptions("result")) return runtimeRef.ret(value, new js.JSVariableRef("result"));
	else if(context.root)	//exports from the module
		return new js.JSCode("(module ? (module.exports = " + value + ", true) : false)");
	else return ParserError.undeclaredIdentifier("result", expression.position.column, expression.position.line);
	
};

ScriptTranslator.faultExpression = function(expression, context, emitDebug){
	var error = this.translate(expression.error, context, emitDebug);
	if(error instanceof ParserError) return error;
	var destination;	
	if(expression.destination){	//destination explicitly defined
		destination = this.translate(expression.destination, context, emitDebug);
		return destination instanceof ParserError ? destination : runtimeRef.fault(error, destination);
	}else {
		//explores 'result' identifier
		destination = context.identifierOptions("result");
		return destination ? runtimeRef.fault(error, new js.JSVariableRef("result")) :
			ParserError.undeclaredIdentifier("result", expression.position.column, expression.position.line);
	}
};

ScriptTranslator.aggregator = function(aggregator, context, emitDebug, column, line){
	switch(aggregator){
		case "+": return new js.JSCode("$asyncscript.binaryPlus.bind($asyncscript)");
		case "-": return new js.JSCode("$asyncscript.binaryMinus.bind($asyncscript)");
		case "*": return new js.JSCode("$asyncscript.multiplication.bind($asyncscript)");
		case "||":
		case "|": return new js.JSCode("$asyncscript.or.bind($asyncscript)");
		case "&&":
		case "&": return new js.JSCode("$asyncscript.and.bind($asyncscript)");
		case "%": return new js.JSCode("$asyncscript.modulo.bind($asyncscript)");
		case "^": return new js.JSCode("$asyncscript.xor.bind($asyncscript)");
		case ".": return new js.JSCode("$asyncscript.getMember.bind($asyncscript)");
		case ">>": return new js.JSCode("$asyncscript.shiftRight.bind($asyncscript)");
		case "<<": return new js.JSCode("$asyncscript.shiftLeft.bind($asyncscript)");
		case ">": return new js.JSCode("$asyncscript.greaterThan.bind($asyncscript)");
		case "<": return new js.JSCode("$asyncscript.lessThan.bind($asyncscript)"); 
		case ">=": return new js.JSCode("$asyncscript.greaterThanOrEqual.bind($asyncscript)");
		case "<=": return new js.JSCode("$asyncscript.lessThanOrEqual.bind($asyncscript)");
		case "==": return new js.JSCode("$asyncscript.areEqual.bind($asyncscript)");
		case "!=": return new js.JSCode("$asyncscript.areNotEqual.bind($asyncscript)");
		case "===": return new js.JSCode("function(a, b){ return a === b; }");
		case "!==": return new js.JSCode("function(a, b){ return a !== b; }"); 
		case "/": return new js.JSCode("$asyncscript.division.bind($asyncscript)");
		case "to": return new js.JSCode("$asyncscript.typecast.bind($asyncscript)");
		case "is": return new js.JSCode("$asyncscript.instanceOf.bind($asyncscript)");
		case "in": return new js.JSCode("$asyncscript.binaryMinus.bind($asyncscript)");
		case "+=":
		case "-=":
		case "=>":
		case "*=":
		case "/=":
		case "%=":
		case "&=":
		case "|=":
		case "^=": return ParserError.unsupportedAggregation(aggregator, column, line);
		case null:
		case undefined: return;
		default:
			aggregator = this.translate(aggregator, context, emitDebug);	
			return aggregator instanceof ParserError ? 
				aggregator :
				new js.JSFunction(["current", "value"], runtimeRef.invoke(new js.JSThis(), aggregator, new js.JSNewArray(["current", "value"])));
	}
};

ScriptTranslator.repeatExpression = function(expression, context, emitDebug){
	context.pushScope(new RepeatScope(expression.loopVar));
	if(expression.body instanceof Array){
		body = this.expressions(expression.body, context, emitDebug);
		if(body instanceof ParserError) return body;
	}
	else {
		body = this.translate(expression.body, context, emitDebug);
		if(body instanceof ParserError) return body;
		body = runtimeRef['continue'](context.scope.loopState.name, new js.JSNewArray([body]));
	}
	var aggregator = this.aggregator(expression.aggregator, context, emitDebug, expression.position.column, expression.position.line);
	return aggregator instanceof ParserError ? aggregator : context.popScope().translate(body, aggregator);
};

ScriptTranslator.continueExpression = function(expression, context, emitDebug){
	var values = this.expressions(expression.values, context, emitDebug);
	if(values instanceof ParserError) return values;
	var innerLoop;
	//analyze destination
	if(expression.destination){
		//if destination is a loop name reference then emit loop continuation code
		if(expression.destination instanceof ast.CodeIdentifierExpression && context.findScope(RepeatScope, function(scope){ 
				return scope.loopState.name === expression.destination.name; 
		})) return new js.JSCall(expression.destination.name, "continue", new js.JSNewArray(values));
		//continue is not located inside of the 'repeat' expression
		expression = this.translate(expression.destination, context, emitDebug);
		if(expression instanceof ParserError) return expression;
		return runtimeRef['continue'](expression, new js.JSNewArray(values));
	}
	else if(innerLoop = context.findScope(Object, function(scope){ return scope.loopScope; }))	//is in repeat scope, use state var
		return new js.JSCall(innerLoop.loopStateName, "continue", new js.JSNewArray(values));
	else //no repeat scope and destination, return error
		return ParserError.invalidContinue(expression.position.column, expression.position.line);
};

ScriptTranslator.breakExpression = function(expression, context, emitDebug){
	var values = this.expressions(expression.values, context, emitDebug);
	if(values instanceof ParserError) return values;
	var innerLoop;
	//analyze destination
	if(expression.destination){
		//if destination is a loop name reference then emit loop continuation code
		if(expression.destination instanceof ast.CodeIdentifierExpression && context.findScope(RepeatScope, function(scope){ 
				return scope.loopState.name === expression.destination.name; 
		})) return new js.JSCall(expression.destination.name, "break", new js.JSNewArray(values));
		//continue is not located inside of the 'repeat' expression
		expression = this.translate(expression.destination, context, emitDebug);
		if(expression instanceof ParserError) return expression;
		return runtimeRef['break'](expression, new js.JSNewArray(values));
	}
	else if(innerLoop = context.findScope(Object, function(scope){ return scope.loopScope; }))	//is in repeat scope, use state var
		return new js.JSCall(innerLoop.loopStateName, "break", new js.JSNewArray(values));
	else //no repeat scope and destination, return error
		return ParserError.invalidBreak(expression.position.column, expression.position.line);
};

ScriptTranslator.conditional = function(expression, context, emitDebug){
	var condition = this.translate(expression.condition, context, emitDebug);
	if(condition instanceof ParserError) return condition;
	context.pushScope(new GenericScope(true));
	var _then = this.block(expression['then'], context, emitDebug, true);
	context.popScope();
	if(_then instanceof ParserError) return _then;
	else _then = new js.JSBlock(_then);
	context.pushScope(new GenericScope(true));
	var _else = this.block(expression['else'], context, emitDebug);
	context.popScope();
	if(_else instanceof ParserError) return _else;
	else _else = new js.JSBlock(_else);
	var result = new js.JSCode("if(__$cond$__ instanceof $asyncscript.Promise)" +
		"if(__$cond$__.isError) throw __$cond$__.result;" +
		"else if(__$cond$__.isCompleted) return arguments.callee.call(this, __$cond$__.result);" +
		"else return $asyncscript.fork(arguments.callee.bind(this), __$cond$__);" +
	"else if(__$cond$__ instanceof $asyncscript.Property) return arguments.callee.call(this, __$cond$__.value);" +
	"else if(__$cond$__)" + _then +
	"else " + _else);
	return new js.JSCall(new js.JSFunction(["__$cond$__"], result), "call", new js.JSThis(), condition);
};

ScriptTranslator.sehExpression = function(seh, context, emitDebug){
	context.pushScope(new GenericScope(true));
	var dangerous = this.block(seh['try'], context, emitDebug, true);
	if(dangerous instanceof ParserError) return dangerous;
	else dangerous = new js.JSCall(new js.JSFunction([], dangerous), "bind", new js.JSThis());
	context.popScope();
	//translates catch scope
	var _catch;
	context.pushScope(new CatchScope());
	_catch = this.block(seh['catch'], context, emitDebug, false);
	if(_catch instanceof ParserError) return _catch;
	else _catch = context.popScope().translate(_catch);
	var _finally;
	if(seh['finally']){
		context.pushScope(new GenericScope(true));
		_finally = this.block(seh['finally'], context, emitDebug, true);
		if(_finally instanceof ParserError) return _finally;
		else _finally = new js.JSCall(new js.JSFunction([], _finally), "bind", new js.JSThis());
		context.popScope();
	}
	//final translation
	return runtimeRef.tryCatchFinally(dangerous, _catch, _finally);
};

ScriptTranslator.anyvalue = function(){ return new js.JSMemberAccess(runtimeRef.value, "anyvalue"); };

ScriptTranslator.switcher = function(expression, context, emitDebug){
	var value = this.translate(expression.target, context, emitDebug);
	if(value instanceof ParserError) return value;
	//translates equality operation
	var comparer;
	if(expression.comparer){
		comparer = this.translate(expression.comparer, context, emitDebug);
		if(comparer instanceof ParserError) return comparer;
	}
	var values = {length: 0};
	//enumerates through values
	for(var i = 0; i < expression.cases.length; i++){
		var c = expression.cases[i];
		//emits handler
		context.pushScope(new GenericScope(true));
		var handler = this.block(c.handler, context, emitDebug, true);
		if(handler instanceof ParserError) return handler;
		context.popScope();
		handler = new js.JSFunction([], handler);
		//enumerates through values
		for(var j = 0; j < c.values.length; j++){
			var caseval = this.translate(c.values[j], context, emitDebug);
			if(caseval instanceof ParserError) return caseval;
			//synchronizes case value
			values[values.length] = new js.JSCode("{value: " + caseval + ", " + (j === 0 ? "handler: " + handler : "get handler(){return this[" + (values.length - j) + "].handler; }") + "}");
			values.length += 1;
		}
	}
	values = new js.JSObject(values);
	//compiles default expr
	var def;
	if(def = expression['else']){
		context.pushScope(new GenericScope(true));
		def = this.block(def, context, emitDebug, true);
		if(def instanceof ParserError) return def;
		else def = new js.JSFunction([], def);
		context.popScope();
	}
	return runtimeRef.select(value, comparer, values, def);
};

ScriptTranslator.forExpression = function(expression, context, emitDebug){
	//translates source
	var source = this.translate(expression.source, context, emitDebug);
	if(source instanceof ParserError) return source;
	else source = runtimeRef.invokeMethod(source, new js.JSStringLiteral("iterator"), new js.JSNewArray([]));
	//translates loop body
	context.pushScope(new ForEachScope(expression.loopVar));
	if(expression.body instanceof Array){
		body = this.expressions(expression.body, context, emitDebug);
		if(body instanceof ParserError) return body;
	}
	else {
		body = this.translate(expression.body, context, emitDebug);
		if(body instanceof ParserError) return body;
		body = runtimeRef['continue'](context.scope.loopStateName, new js.JSNewArray([body]));
	}
	if(expression.loopVar.contract !== undefined && !(expression.loopVar.contract instanceof ast.CodeBuiltInContractExpression && expression.loopVar.contract.value === "object")){
		var loopVarBinding = this.translate(expression.loopVar.contract, context, emitDebug);
		if(loopVarBinding instanceof ParserError) return ParserError;
		else body.unshift(new js.JSAssignment(context.scope.loopVar.name, runtimeRef.binding(context.scope.loopVar.name, loopVarBinding)));
	}
	var aggregator = this.aggregator(expression.aggregator, context, emitDebug, expression.position.column, expression.position.line);	
	return aggregator instanceof ParserError ? aggregator : context.popScope().translate(source, body, aggregator);
};

ScriptTranslator.callee = function(expr, context, emitDebug){
	var scope;
	if(expr.quoted)
		if(scope = context.findScope(FunctionScope)) return new js.JSNew(new js.JSMemberAccess(runtimeRef.value, "Expression"), new js.JSCode(JSON.stringify(scope.expression)));
		else return new js.JSNull();
	//if located in the function scope the use callee storage
	else if(context.findScope(FunctionScope)) return new js.JSVariableRef(currentFunction);
	else return new js.JSNull();
};

ScriptTranslator.placeholder = function(expr, context, emitDebug){
	return expr['default'] ? this.translate(expr['default'], context, emitDebug): new js.JSNew(new js.JSMemberAccess(runtimeRef.value, "Expression"), JSON.stringify(expr));
};

ScriptTranslator.expansion = function(expr, context, emitDebug){
	var target = this.translate(expr.target, context, emitDebug);
	if(target instanceof ParserError) return target;
	var args = this.expressions(expr.arguments, context, emitDebug);
	if(args instanceof ParserError) return args;
	return new js.JSCall(runtimeRef.value, "expand", target, new js.JSNewArray(args));
};

ScriptTranslator.jscode = function(expr){
	return "function(){ " + expr.code + "}.call(this)";
};

//main translator
ScriptTranslator.translate = function(node, context, emitDebug){
	//reduce node if it is possible
	if(node && node.reduce instanceof Function) node = node.reduce(context.checked);
	var nodeCompiler;
	if(node instanceof Array) return this.scopeExpression(node, context, emitDebug);
	else if(node) 
		if(node.translate instanceof Function) {
			//inserts prerequisite
			if(node.$prerequisite) context.bootstrapper[node.nodeType] = node.$prerequisite;
			return node.translate(context, emitDebug);
		}
		else switch(node.nodeType){
			case "CodeUnaryExpression": nodeCompiler = "unaryExpression"; break;
			case "JavaScriptCode": nodeCompiler = "jscode"; break;
			case "CodeIdentifierExpression": nodeCompiler = "identifier"; break;
			case "CodeStringExpression": nodeCompiler = "stringExpression"; break;
			case "CodeBinaryExpression": nodeCompiler = "binaryExpression"; break;
			case "CodeIntegerExpression": nodeCompiler = "integerLiteral"; break;
			case "CodeBooleanExpression": nodeCompiler = "booleanLiteral"; break;
			case "CodeBuiltInContractExpression": nodeCompiler = "contractRef"; break;
			case "CodeRealExpression":  nodeCompiler = "realLiteral"; break;
			case "CodeInvocationExpression": nodeCompiler = "invocationExpression"; break;
			case "CodeIndexerExpression": nodeCompiler = "indexerExpression"; break;
			case "CodeLetExpression": nodeCompiler = "letExpression"; break;
			case "CodeFunctionExpression": nodeCompiler = "functionExpression"; break;
			case "CodeBreakpointExpression": nodeCompiler = "breakpointExpression"; break;
			case "CodeContainerExpression": nodeCompiler = "containerExpression"; break;
			case "CodeContainerContractExpression": nodeCompiler = "containerContractExpression"; break;
			case "CodeArrayExpression": nodeCompiler = "arrayExpression"; break;
			case "CodeRegularExpression": nodeCompiler = "regularExpression"; break;
			case "CodeArrayContractExpression": nodeCompiler = "arrayContract"; break;
			case "CodeScopeExpression":  nodeCompiler = "scopeRef"; break;
			case "CodeContextExpression": nodeCompiler = "contextExpression"; break;
			case "CodeWithExpression":  nodeCompiler = "withExpression"; break;
			case "CodeReturnExpression": nodeCompiler = "returnExpression"; break;
			case "CodeFaultExpression": nodeCompiler = "faultExpression"; break;
			case "CodeRepeatExpression": nodeCompiler = "repeatExpression"; break;
			case "CodeBreakExpression": nodeCompiler = "breakExpression"; break;
			case "CodeContinueExpression": nodeCompiler = "continueExpression"; break;
			case "CodeConditionalExpression": nodeCompiler = "conditional"; break;
			case "CodeSehExpression": nodeCompiler = "sehExpression"; break;
			case "CodeAnyValueExpression": nodeCompiler = "anyvalue"; break;
			case "CodeSwitcherExpression": nodeCompiler = "switcher"; break;
			case "CodeForExpression": nodeCompiler = "forExpression"; break;
			case "CodePlaceholderExpression": nodeCompiler = "placeholder"; break;
			case "CodeExpansionExpression": nodeCompiler = "expansion"; break;
			case "CodeQuotedExpression": nodeCompiler = "quotedExpression"; break;
			default: return;
		}
	else return;
	var result = this[nodeCompiler](node, context, emitDebug);
	if(result) result.comment = emitDebug ? ("column: " + node.position.column + ' line: ' + node.position.line) : '';
	return result;
};

ScriptTranslator.prototype.next = function(callback){
	callback = async.asyncCallback(callback);
	function iteration(err, expression){	
		if(err) return callback(err);
		//this code saves the previously translated expression to use in the next expression
		else if(expression instanceof ast.CodeStringExpression){
			expression = this.constructor.directiveOrString(expression, this.context, this.emitDebug);
			return expression === null ? this.next(callback) : callback(undefined, expression);
		}
		else if(expression === null) return callback(undefined, new js.JSNull());
		else if((expression = this.constructor.translate(expression, this.context, this.emitDebug)) instanceof ParserError) return callback(expression);
		else return callback(undefined, expression);
	}
	return this.parser.next(iteration.bind(this));
};

/**
 * Translates AsyncScript to JavaScript
 * @param {Object} source The source represented by string, expression tree or an instance of {SyntaxAnalyzer}.
 * @param {Boolean} emitDebug true to generate debug information; otherwise, false. Optional.
 * @param {Boolean} dynamically true to generate code for the in-memory execution; otherwise, false. Optional.
 * @param {Function} callback A callback that accepts translation error and output.
 */
function translate(source, emitDebug, dynamically, callback){
	switch(arguments.length){
		case 2: callback = emitDebug; emitDebug = false; dynamically = false; break;
		case 3: callback = dynamically; dynamically = false; break;
		case 4: break;
		default: throw new Error("Invalid count of arguments");
	}
	callback = async.asyncCallback(callback);
	//translates from source
	var translator = new ScriptTranslator(source, new GlobalScope(dynamically));
	translator.constructor = ScriptTranslator;
	translator.emitDebug = emitDebug;
	var result = new Array(), jsnode;
	return async.asyncWhile(function(condition){
		this.next(function(err, compiled){
			return err ? callback(err) : condition(jsnode = compiled); 
		});
	}.bind(translator),
	function(next){
		this.push(jsnode);
		return next();
	}.bind(result),
	function(){
		//attaches the bootstrapped code
		Array.prototype.unshift.apply(this, Object.keys(translator.context.bootstrapper).map(function(name){
			return this[name];
		}, translator.context.bootstrapper));
		var result = translator.context.scope.translate(this, emitDebug);
		return result instanceof ParserError ? callback(result) : callback(undefined, result);
	}.bind(result));
}

/**
 * Compiles source code to JavaScript function or code.
 */
function compile(source, emitDebug, asFunction, callback){
	switch(arguments.length){
		case 2: callback = emitDebug; emitDebug = false; asFunction = false; break;
		case 3: callback = asFunction; asFunction = false; break;
		case 4: break;
		default: throw new Error("Invalid count of arguments");
	}
	return translate(source, emitDebug, asFunction, function(err, source){
		if(err) return callback(err);
		else if(asFunction) source = new Function(source.toString());
		return callback(undefined, source.toString());
	});
}

function run(_this, source, emitDebug, callback){
	switch(arguments.length){
		case 4: break;
		case 3: callback = emitDebug; emitDebug = false; break;
		default: throw new Error("Invalid count of arguments");
	}
	return translate(source, emitDebug, true, function(err, source){
		if(err !== undefined) return callback(err);
		source = new Function("return " + source);
		var result;
		try{
			result = source.call(_this);
		}
		catch(e){
			return callback(e);
		}
		return callback(undefined, result);
	});
};

module.exports = {
	'ScriptTranslator': ScriptTranslator,
	'translate': translate,
	'compile': compile,
	'run': run,
	'mangle': mangle,	
	'GenericScope': GenericScope,
	'CatchScope': CatchScope
};
