/**
 * Creates a new lexeme.
 * @class Represents AsyncScript lexeme.
 * @param {String} value Represents value of the lexeme.
 * @param {String} kind Represents kind of the lexeme.
 */
function Lexeme(value, kind){
	this.value = value;
	this.kind = kind;
}

/**
 * Returns a string that represents the lexeme.
 * @returns {String} A string that represents the lexeme.
 */
Lexeme.prototype.toString = function(){ return this.value; };

//Punctuations
[';', ':', ',', '@', '(', ')', '{', '}', '[', ']', '->', '@@'].forEach(function(p){
	this[p] = new Lexeme(p, 'punctuation');
}, Lexeme.punctuation = {});

//Operators
['.', '+', '++', '+=', '--', '-', '-=', '==', '=', '===', '?', '??', 
'^', '^=', '|', '|=', '&', '&&', '||', '&=', '*', '**', '*=', '/', '/=', '%', '%=', '%=', '%%',
'>', '>>', '<', '<<', '>=', '<=', '!=', '!==', '!', '!!', '$', 'to', 'is', 'in', '=>', ':='].forEach(function(p){
	this[p] = new Lexeme(p, 'operator');
}, Lexeme.operators = {});

//Keywords
['object', 'let', 'string', 'integer', 'function', 'return',
'typedef', 'checked', 'unchecked', 'expression', 'regexpr', 'quoted',
'boolean', 'void', 'false', 'true', 'real', 'this', 'global', 'for', 
'repeat', 'expandq', 'with', 'continue', 'break', 'fault', 'breakpoint', 'any'].forEach(function(p){
	this[p] = new Lexeme(p, 'keyword');
}, Lexeme.keywords = {});

Lexeme.isDigit = function(ch){ return /\d/.test(ch); };
Lexeme.isWhiteSpace = function(ch) { return /\s/.test(ch); };
Lexeme.isAlpha = function(ch){ return /\w/.test(ch); };
module.exports = Lexeme;
