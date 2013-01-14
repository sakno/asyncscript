/*
 * This syntax extension allows to sequential code blocks.
 */	
var main = require('../lib/main.js');

ASYNCSCRIPT_SYNTAX_EXTENSIONS['#sequentially'] = function(column, line, terminators, callback){
	var lex = this.nextLexeme(function(lex){ return lex === main.Lexeme.punctuation['{']; },
								function(column, line, lex){ return main.ParserError.expected("{", lex, column, line); });
	//parse expression list
	if(lex instanceof main.ParserError) return callback(lex);
	main.ast.parseExpressionList.call(this, terminators, true, function(err, list){
		if(err) return callback(err);
		var result;
		//generates await for each expression in list
		while(list.length)
			if(result) {
				var await = new main.ast.CodeAwaitExpression(result.position.column, result.position.line);
				var val = new main.ast.CodeLetExpression(result.position.column, result.position.line);
				val.value = result;
				val.name = "acquis";
				await.synchronizedValues.push(val);
				await.body = list.shift();
				result = await;
			}
			else result = list.shift();
		list = this.shouldNextLexeme();
		return list instanceof main.ParserError ? callback(list) : callback(undefined, result);
	}.bind(this));
};
