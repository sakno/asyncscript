/*
 * This syntax extension allows to write JavaScript directly inside of the AsyncScript programs.
 */	
var ParserError = require('../lib/ParserError.js'), js = require('../lib/jsnodes.js');

/**
 * Initializes a new syntax block of raw JavaScript code.
 * @param {Integer} column
 * @param {Integer} line
 * @param {Script} script
 */	
function JavaScriptCode(column, line, script){
	this.position = {'column': column, 'line': line};
	this.script = script;
}

JavaScriptCode.prototype.translate = function(){
	return new js.JSCall(new js.JSScope(new js.JSCode(this.script)), "call", new js.JSThis());
};

JavaScriptCode.prototype.toString = function(){
	return "#javascript \"" + this.script + "\"";
};

ASYNCSCRIPT_SYNTAX_EXTENSIONS['#javascript'] = function(column, line, terminators, callback){
	var lex = this.nextLexeme(function(lex){ return lex && lex.kind === "string"; },
								function(column, line, lex){ return ParserError.expected("JavaScript code", lex, column, line); }), jscode;
	if(lex instanceof ParserError) return callback(lex);
	else jscode = lex.value;
	lex = this.shouldNextLexeme();
	return lex instanceof ParserError ? callback(lex) : callback(undefined, new JavaScriptCode(column, line, jscode));
};
