(function(){

/**
 * Creates a new parsing error.
 * @param {Integer} code Code of the error.
 * @param {String} message Human-readable description of the message.
 * @param {Integer} column Number of the column in the source code.
 * @param {Integer} line Number of the line in the source code.
 * @param {String} fileName The name of the file that contains invalid code.
 */
function ParserError(code, message, column, line, fileName){
	this.position = {'column': column, 'line': line};
	this.code = code;
	this.message = message;
	this.fileName = fileName;
}

ParserError.prototype = {
	get column(){ return this.position.column; },
	get line(){ return this.position.line; }
};

ParserError.prototype.toString = function(){
	return this.fileName ?
		'(' + this.fileName + ':' + this.column + ':' + this.line + ') ' + this.message :
		'(' + this.column + ':' + this.line + ') ' + this.message
}

ParserError.unknownCharacter = function(character, column, line){
	return new this(1, 'Unknown character ' + character, column, line);
};

ParserError.expectedIdentifier = function(column, line){
	return new this(2, 'Identifier expected', column, line);
};

ParserError.expected = function(expected, actual, column, line){
	return new this(3, 'Expected ' + expected + ' but found ' + actual || 'nothing', column, line);
};

ParserError.unexpectedEnd = function(column, line){
	return new this(4, 'Unexpected end of input', column, line);
};

ParserError.expectedOperator = function(actual, column, line){
	return new this(5, 'Expected operator but found ' + actual, column, line);
};

ParserError.duplicatedIdentifier = function(name, column, line){
	return new this(6, 'Duplicated variable declaration ' + name, column, line);
};

ParserError.incompatibleValue = function(value, contract, column, line){
	return new this(7, "Value " + value + " is incompatible with contract " + contract, column, line);
};

ParserError.undeclaredIdentifier = function(name, column, line){
	return new this(8, "Identifier " + name + " was not declared", column, line);
};

ParserError.unsupportedAggregation = function(op, column, line){
	return new this(9, "Operator or expression '" + op + "' is not allowed for aggregation", column, line);
};

ParserError.invalidContinue = function(column, line){
	return new this(10, "'continue' operator is not located inside of the 'repeat' block and used without destination", column, line);
};

ParserError.invalidBreak = function(column, line){
	return new this(10, "'break' operator is not located inside of the 'repeat' block and used without destination", column, line);
};

ParserError.invalidPlaceholder = function(column, line){
	return new this(11, "Expression placeholder(%%) should be located inside of the code quotation or expanded with 'expandq' operator", column, line);
};

//Export for nodejs
if(typeof module == 'undefined') this.ParserError = ParserError;
else module.exports = ParserError;

}).call(this);
