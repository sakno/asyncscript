/*
 * This syntax extension allows to write JavaScript directly inside of the AsyncScript programs.
 */	
var ParserError = require('../lib/ParserError.js'), 
	JavaScriptCode = require('../lib/ast.js').JavaScriptCode;

ASYNCSCRIPT_SYNTAX_EXTENSIONS['#javascript'] = function(column, line, terminators, callback){
	var lex = this.nextLexeme(function(lex){ return lex && lex.kind === "string"; },
								function(column, line, lex){ return ParserError.expected("JavaScript code", lex, column, line); }), jscode;
	if(lex instanceof ParserError) return callback(lex);
	else jscode = lex.value;
	lex = this.shouldNextLexeme();
	return lex instanceof ParserError ? callback(lex) : callback(undefined, new JavaScriptCode(column, line, jscode));
};
