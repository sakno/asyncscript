/*
 * This syntax extension allows to sequential code blocks.
 */	
var main = require('../lib/main.js'), js = require('../lib/jsnodes.js');

/**
 * Initializes a new syntax block of raw JavaScript code.
 * @param {Integer} column
 * @param {Integer} line
 * @param {Script} tree
 */	
function QuotedCode(column, line, tree){
	this.position = {'column': column, 'line': line};
	this.tree = tree;
}

QuotedCode.prototype.translate = function(){
	return new js.JSNew(new js.JSMemberAccess("$asyncscript", "Expression"), new js.JSCode(JSON.stringify(this.tree)));
};

QuotedCode.prototype.toString = function(){
	return "(#quote " + this.code + ")";
};

ASYNCSCRIPT_SYNTAX_EXTENSIONS['#quote'] = function(column, line, terminators, callback){
	var lex = this.shouldNextLexeme();
	//parse expression list
	if(lex instanceof main.ParserError) return callback(lex);
	this.next(terminators, function(err, expr){
		return err ? callback(err) : callback(undefined, new QuotedCode(column, line, expr));
	}.bind(this));
};
