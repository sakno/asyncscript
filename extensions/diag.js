//Diagnostics extensions
	
var main = require('../lib/main.js'), 
ParserError = main.ParserError, 
ScriptTranslator = main.ScriptTranslator;

/**
 * Initializes a new IF DEBUG expression.
 * @param {Integer} column
 * @param {Integer} line
 */	
function IfDebugDirective(column, line){
	this.position = {'column': column, 'line': line};
}

IfDebugDirective.prototype.translate = function(context, emitDebug){
	return emitDebug ? ScriptTranslator.translate(this.expression, context, emitDebug) : "null"; 
};

IfDebugDirective.prototype.toString = function(){
	return "(#ifdebug " + this.expression + ")";
};

ASYNCSCRIPT_SYNTAX_EXTENSIONS['#ifdebug'] = function(column, line, terminators, callback){
	var lex = this.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	var result = new IfDebugDirective(column, line);
	this.next(terminators, function(err, expression){
		if(err) return callback(err);
		result.expression = expression;
		var lex = this.shouldNextLexeme();
		return lex instanceof ParserError ? callback(lex) : callback(undefined, result);
	}.bind(this));
};

/**
 * Initializes a new task name.
 * @param {Integer} column
 * @param {Integer} line
 */	
function TaskName(column, line, name){
	this.position = {'column': column, 'line': line};
	this.name = name;
}

TaskName.prototype.translate = function(context, emitDebug){
	return "(function(a){ if(a instanceof $asyncscript.Promise) { p.name = \"" + this.name + "\"; return true; } else return false; }.call(this, " + ScriptTranslator.translate(this.promise, context, emitDebug) + "))":
};

TaskName.prototype.toString = function(){
	return "(#setTaskName " + this.name + " " + this.promise + ")";
};

ASYNCSCRIPT_SYNTAX_EXTENSIONS['#setTaskName'] = function(column, line, terminators, callback){
	var lex = this.nextLexeme(function(lex){ return lex && lex.kind === "nmtoken"; },
								function(column, line, lex){ return ParserError.expected("Task name", lex, column, line); }), name;
	if(lex instanceof ParserError) return callback(lex);
	else name = lex.value;
	lex = this.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	var result = new TaskName(column, line, name);
	this.next(terminators, function(err, expression){
		if(err) return callback(err);
		result.promise = expression;
		var lex = this.shouldNextLexeme();
		return lex instanceof ParserError ? callback(lex) : callback(undefined, result);
	}.bind(this));
};
