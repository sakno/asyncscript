/*
 * This syntax extension allows to create filter types.
 */	
var main = require('../lib/main.js'), 
	js = require('../lib/jsnodes.js'), 
	ParserError = require('../lib/ParserError.js'),
	Lexeme = require('../lib/Lexeme.js');

ASYNCSCRIPT_SYNTAX_EXTENSIONS['#defineFilter'] = function(column, line, terminators, callback){
	var lex = this.shouldNextLexeme();
	//parse contract definition	
	return lex instanceof ParserError ? callback(lex) : this.next([Lexeme.punctuation['->']], function(err, contract){
		var lex = this.shouldNextLexeme();
		if(lex instanceof ParserError) return callback(lex);
		//parse filter definition
		return this.next(terminators, function(err, condition){
			//return as invocation
			var result = new main.ast.CodeBinaryExpression(
				new main.ast.CodeBuiltInContractExpression("typedef", column, line),
				main.ast.parseOperator('.', true),
				new main.ast.CodeIdentifierExpression("filter", column, line),
				column,
				line);
			result = new main.ast.CodeInvocationExpression(result, 
				[new main.ast.CodeFunctionExpression(column, line, [{name: "value", contract: contract}], condition)],
				column,
				line);
			return callback(undefined, result);
		}.bind(this));
	}.bind(this));
};
