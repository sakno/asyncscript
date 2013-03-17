var ParserError = require('./ParserError.js'), Lexeme = require('./Lexeme.js'), async = require('./async_helpers.js');

var associativity = {left: 0, right: 1}, highestOperatorPriority = 12;

function parseOperator(op, lvalue){
	var result = {value: typeof op === 'string' ? op : op.value};
	switch(op){
		case Lexeme.operators['++']:
		case Lexeme.operators['--']:
		case Lexeme.operators['**']:
			result.arity = 1;
			if(lvalue){
				result.associativity = associativity.left;
				result.priority = highestOperatorPriority;
				result.style = 'postfix';
			}
			else {
				result.associativity = associativity.right;
				result.priority = highestOperatorPriority - 1;
				result.style = 'prefix';
			}
		break;
		case Lexeme.operators['$']:
			result.associativity = associativity.left;
			result.priority = highestOperatorPriority;
			result.arity = 1;
		break;
		case Lexeme.operators['.']:
			result.associativity = associativity.left;
			result.priority = highestOperatorPriority;
			result.arity = 2;
		break;
		case Lexeme.operators['*']:
		case Lexeme.operators['%']:
		case Lexeme.operators['/']:
			result.associativity = associativity.left;
			result.priority = highestOperatorPriority - 2;
			result.arity = 2;
		break;
		case Lexeme.operators['-']:
		case Lexeme.operators['+']: 
			result.associativity = associativity.left;
			if(lvalue){
				result.arity = 2;
				result.priority = highestOperatorPriority - 3;
			}
			else {
				result.arity = 1;
				result.priority = highestOperatorPriority - 1;
			}
		break;
		case Lexeme.operators['>>']:
		case Lexeme.operators['<<']:
			result.associativity = associativity.left;
			result.priority = highestOperatorPriority - 4;
			result.arity = 2;
		case Lexeme.operators['<']:
		case Lexeme.operators['>']:
		case Lexeme.operators['<=']:
		case Lexeme.operators['>=']:
		case Lexeme.operators['<>']:
		case Lexeme.operators['is']:
		case Lexeme.operators['in']:
		case Lexeme.operators['to']:
			result.associativity = associativity.left;
			result.priority = highestOperatorPriority - 5;
			result.arity = 2;
		break;
		case Lexeme.operators['&']:
			result.associativity = associativity.left;
			result.priority = highestOperatorPriority - 6;
			result.arity = 2;
		break;
		case Lexeme.operators['^']:
			result.associativity = associativity.left;
			if(lvalue){
				result.priority = highestOperatorPriority - 7;
				result.arity = 2;
			}
			else {
				result.arity = 1;
				result.priority = highestOperatorPriority;
			}
		break;
		case Lexeme.operators['|']:
			result.associativity = associativity.left;
			result.priority = highestOperatorPriority - 8;
			result.arity = 2;
		break;
		case Lexeme.operators['&&']:
			result.associativity = this.associativity.left;
			result.priority = this.highestOperatorPriority - 9;
			result.arity = 2;
		break;
		case Lexeme.operators['||']:
			result.associativity = associativity.left;
			result.priority = highestOperatorPriority - 10;
			result.arity = 2;
		break;
		case Lexeme.operators['==']:
		case Lexeme.operators['===']:
		case Lexeme.operators['!=']:
		case Lexeme.operators['!==']:
			result.associativity = associativity.left;
			result.priority = highestOperatorPriority - 11;
			result.arity = 2;
		break;
		case Lexeme.operators['?']:
		case Lexeme.operators['!!']:
		case Lexeme.operators['??']:
			result.associativity = associativity.right;
			result.priority = highestOperatorPriority - 11;
			result.arity = 3;		
		break;
		case Lexeme.operators['=>']:
			result.associativity = associativity.left;
			result.priority = highestOperatorPriority - 12;
			result.arity = 2;		
		break;
		case Lexeme.operators['+=']:
		case Lexeme.operators['-=']:
		case Lexeme.operators['*=']:
		case Lexeme.operators['/=']:
		case Lexeme.operators['^=']:
		case Lexeme.operators['&=']:
		case Lexeme.operators['|=']:
		case Lexeme.operators['%=']:
		case Lexeme.operators['=']:
		case Lexeme.operators[':=']:
			result.associativity = associativity.right;
			result.priority = highestOperatorPriority - 12;
			result.arity = 2;
		break;
		default: 
			result.associativity = associativity.left;
			result.priority = -1;
			result.arity = 0;
	}
	return result;
};

function parseExpressionList(terminators, move, exprs, callback){
	if(exprs instanceof Function) 
		if(this.lookup !== Lexeme.punctuation['{']) return this.next(terminators, callback = exprs);
		else {
			callback = exprs;
			exprs = new Array();
			exprs.position = {column: this.column, line: this.line};
			exprs.toString = expressionListToString;
		};
	var lex = move && this.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	else if(lex !== Lexeme.punctuation['}']) this.next([Lexeme.punctuation['}'], Lexeme.punctuation[';']], function(err, expr){
		if(err) return callback(err);
		exprs.push(expr);
		//moves to next statement
		return this.lookup === Lexeme.punctuation[';'] ? parseExpressionList.call(this, terminators, true, exprs, callback) : callback(undefined, exprs);
	}.bind(this));
	//moves to next statement
	else return this.lookup === Lexeme.punctuation[';'] ? parseExpressionList.call(this, terminators, true, exprs, callback) : callback(undefined, exprs);
}
	
/**
 * Creates a new boolean expression.
 * @class Represents boolean value expression.
 * @param {String} value The stringified value of the boolean.
 * @param {Integer} column The number of the column in the source code.
 * @param {Integer} line The number of the line in the source code.
 */
function CodeBooleanExpression (value, column, line) {
	if(arguments.length === 0) return;	//deserialization case
	this.value = JSON.parse(value);
	this.isPrimitive = true;
	this.position = {'column': column, 'line': line};
	this.nodeType = "CodeBooleanExpression";
}

CodeBooleanExpression.prototype.convertTo = function(contract, explicitly){
	if(contract === undefined) return this;
	else if(explicitly && contract instanceof CodeFunctionExpression && contract.implementation === undefined)
		return new CodeFunctionExpression(this.position.column, this.position.line, contract.parameters, this);
	else if(contract instanceof CodeBuiltInExpression)
		switch(contract.value){
			case "function": return explicitly ? new CodeFunctionExpression(this.position.column, this.position.line, [], this) : undefined;
			case "void": return contract;
			case "real": 
			case "integer": return new CodeIntegerExpression(this.value ? "1" : "0", this.position.column, this.position.line);
			case "object":
			case "boolean": return this;
			case "string": return explicitly ? new CodeStringExpression(this.value ? "true" : "false", this.position.column, this.position.line) : undefined;
			case "regexp": return explicitly ? new CodeRegularExpression(this.value ? "true" : "false", this.position.column, this.position.line) : undefined;
			case "cplx": return explicitly ? new CodeBinaryExpression(this, 'to', contract) : undefined;
			default: return;		
		}
	else return;
};

CodeBooleanExpression.prototype.toString = function(){ return this.value.toString(); };

//=================================================================================================

/**
 * Creates a new built-in contract expression.
 * @class Represents built-in contract tree.
 * @param {String} value The name of the contract.
 * @param {Integer} column The number of the column in the source code.
 * @param {Integer} line The number of the line in the source code.
 */
function CodeBuiltInContractExpression (value, column, line) {
	if(arguments.length === 0) return;	//deserialization case
	this.value = value;
	this.isPrimitive = true;
	this.position = {'column': column, 'line': line};
	this.nodeType = "CodeBuiltInContractExpression";
}

CodeBuiltInContractExpression.prototype.toString = function(){ return this.value.toString(); };

//=================================================================================================

/**
 * Creates a new identifier name.
 * @class Represents code identifier name.
 * @param {String} name The name of the identifier.
 * @param {Integer} column
 * @param {Integer} line
 */
function CodeIdentifierExpression(name, column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.name = name;
	this.position = {'column': column, 'line': line};
	this.nodeType = "CodeIdentifierExpression";
}

CodeIdentifierExpression.prototype.toString = function(){ return this.name; };

//=================================================================================================

/**
 * Creates a new integer expression.
 * @class Represents integer value expression.
 * @param {String} value The stringified value of the integer.
 * @param {Integer} column The number of the column in the source code.
 * @param {Integer} line The number of the line in the source code.
 */
function CodeIntegerExpression (value, column, line) {
	if(arguments.length === 0) return;	//deserialization case
	this.value = JSON.parse(value);
	this.isPrimitive = true;
	this.position = {'column': column, 'line': line};
	this.nodeType = "CodeIntegerExpression";
}

CodeIntegerExpression.prototype.convertTo = function(contract, explicitly){
	if(contract === undefined) return this;
	else if(explicitly && contract instanceof CodeFunctionExpression && contract.implementation === undefined)
		return new CodeFunctionExpression(this.position.column, this.position.line, contract.parameters, this);
	else if(contract instanceof CodeBuiltInExpression)
		switch(contract.value){
			case "function": return explicitly ? new CodeFunctionExpression(this.position.column, this.position.line, [], this) : undefined;
			case "void": return contract;
			case "real": 
			case "object":
			case "integer": return this;
			case "cplx": return explicitly ? this : undefined;
			case "boolean": return new CodeBooleanExpression(this.value ? "true" : "false", this.position.column, this.position.line);
			case "string": return explicitly ? new CodeStringExpression(this.value.toString(), this.position.column, this.position.line) : undefined;
			case "regexp": return explicitly ? new CodeRegularExpression(this.value.toString(), this.position.column, this.position.line) : undefined;
			case "cplx": return explicitly ? new CodeBinaryExpression(this, 'to', contract) : undefined;			
			default: return;		
		}
	else return;
};

CodeIntegerExpression.prototype.toString = function(){ return this.value.toString(); };

//=================================================================================================

/**
 * Represents a new expression that declares a new symbol.
 */
function CodeLetExpression(column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.position = {'column': column, 'line': line};
	this.nodeType = "CodeLetExpression";
}

CodeLetExpression.prototype.toString = function(format){
	return (format == 'omit-lef' ? '' : 'let ') + this.name + ' = ' + this.value + (this.contract ? ' : ' + this.contract : '');
};

CodeLetExpression.parseAccessor = function(parser, column, line, terminators, callback){
	var lex = parser.nextLexeme(function(lex){ return lex && lex.kind === "nmtoken" && (lex.value === "get" || lex.value === "set"); },
			function(column, line, lex){ return ParserError.expected('get', lex, column, line); });
	if(lex instanceof ParserError) return callback(lex);
	var accessor = lex.value;
	lex = parser.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	//parse accessor body
	parser.next(terminators, function(err, lex){
		return err ? callback(err) : callback(undefined, {'name': accessor, 'implementation': lex});
	});
};

/**
 * Parses LET expression.
 * @param {SyntaxAnalyzer} parser The syntax parser.
 * @param {Integer} column 
 * @param {Integer} line
 * @param {Array} terminators
 */
CodeLetExpression.parse = function(parser, column, line, terminators, callback){
	var result = new this(column, line);
	function parseContract(parser, result, callback){
		//Parsing contract
		switch(parser.lookup){
			case Lexeme.punctuation[':']:
				var lex = parser.shouldNextLexeme();
				if(lex instanceof ParserError) return callback(lex);
				return parser.next(terminators, function(err, contract){
					if(err) return callback(err);
					else result.contract = contract;
					callback(undefined, result);
				});
			default: return callback(undefined, result);
		}
	}
	//move to the next expression to parse identifier name
	var lex = parser.nextLexeme(
		function(lex){ return lex && lex.kind === 'nmtoken'; },
		function(column, line){ return ParserError.expectedIdentifier(column, line); });
	if(lex instanceof ParserError) return callback(lex);
	else result.name = lex.value;
	lex = parser.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	switch(lex){
		case Lexeme.operators['=']: 
			//moves to the expression	
			lex = parser.shouldNextLexeme();
			if(lex instanceof ParserError) return callback(lex);
			terminators.push(Lexeme.punctuation[':']);
			//parse init expression
			return parser.next(terminators, function(err, lex){
				if(err) return callback(err);			
				terminators.pop();	
				result.value = lex;	//save init expression
				parseContract(parser, result, callback);	
			});	
		case Lexeme.punctuation['{']:
			//moves to the accessor name
			
			return this.parseAccessor(parser, parser.column, parser.line, [Lexeme.punctuation['}'], Lexeme.punctuation[',']], function(err, accessor){
				if(err) return callback(err);
				result[accessor.name] = accessor.implementation;
				switch(parser.lookup){
					case Lexeme.punctuation['}']://no more accessors
						var lex = parser.shouldNextLexeme();
						return lex instanceof ParserError ? callback(lex) : parseContract(parser, result, callback);
					case Lexeme.punctuation[',']://one more accessor
						return this.parseAccessor(parser, parser.column, parser.line, [Lexeme.punctuation['}']], function(err, accessor){
							if(err) return callback(err);
							result[accessor.name] = accessor.implementation;
							var lex = parser.shouldNextLexeme();
							return lex instanceof ParserError ? callback(lex) : parseContract(parser, result, callback);
						});
					//failed to parse LET
					default: return callback(ParserError.expected('}', parser.lookup, parser.column, parser.line));
				}
			}.bind(this));
		default: return callback(ParserError.expected('=', lex, parser.column, parser.line));
	}
};

//=================================================================================================

/**
 * Creates a new real expression.
 * @class Represents real value expression.
 * @param {String} value The stringified value of the real.
 * @param {Integer} column The number of the column in the source code.
 * @param {Integer} line The number of the line in the source code.
 */
function CodeRealExpression (value, column, line) {
	if(arguments.length === 0) return;	//deserialization case
	this.value = JSON.parse(value);
	this.isPrimitive = true;
	this.position = {'column': column, 'line': line};
	this.nodeType = "CodeRealExpression";
}

CodeRealExpression.prototype.convertTo = function(contract, explicitly){
	if(contract === undefined) return this;
	else if(explicitly && contract instanceof CodeFunctionExpression && contract.implementation === undefined)
		return new CodeFunctionExpression(this.position.column, this.position.line, contract.parameters, this);
	else if(contract instanceof CodeBuiltInExpression)
		switch(contract.value){
			case "function": return explicitly ? new CodeFunctionExpression(this.position.column, this.position.line, [], this) : undefined;
			case "void": return contract;
			case "integer": return explicitly ? new CodeIntegerExpression(Math.round(this.value).toString(), this.position.column, this.position.line) : undefined;
			case "object": 
			case "real": return this; 
			case "cplx": return explicitly ? this : undefined;
			case "boolean": return explicitly ? new CodeBooleanExpression(this.value ? "true" : "false", this.position.column, this.position.line) : undefined;
			case "string": return explicitly ? new CodeStringExpression(this.value.toString(), this.position.column, this.position.line) : undefined;
			case "regexp": return explicitly ? new CodeRegularExpression(this.value.toString(), this.position.column, this.position.line) : undefined;
			case "cplx": return explicitly ? new CodeBinaryExpression(this, 'to', contract) : undefined;
			default: return;		
		}
	else return;
};

CodeRealExpression.prototype.toString = function(){ return this.value.toString(); };

//=================================================================================================

/**
 * Creates a new string literal.
 * @class Represents string literal.
 * @param {String} value Represents string literal.
 * @param {Integer} column The number of the column in the source code.
 * @param {Integer} line The number of the line in the source code.
 */
function CodeStringExpression(value, column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.value = value;
	this.isPrimitive = true;
	this.position = {'column': column, 'line': line};
	this.nodeType = "CodeStringExpression";
}

CodeStringExpression.prototype.convertTo = function(contract, explicitly){
	if(contract === undefined) return this;
	else if(explicitly && contract instanceof CodeFunctionExpression && contract.implementation === undefined)
		return new CodeFunctionExpression(this.position.column, this.position.line, contract.parameters, this);
	else if(contract instanceof CodeBuiltInContractExpression)
		switch(contract.value){
			case "function": return explicitly ? new CodeFunctionExpression(this.position.column, this.position.line, [], this) : undefined;
			case "void": return contract;
			case "object":
			case "string": return this;
			case "boolean": return explicitly ? new CodeBooleanExpression(this.value.length > 0 ? "true" : "false", this.position.column, this.position.line) : undefined;
			case "regexp": return explicitly ? new CodeRegularExpression(this.value, this.position.column, this.position.line) : undefined;
		}
	else return;
};

CodeStringExpression.prototype.toString = function(){ return '\'' + this.value + '\''; };

//=================================================================================================

/**
 * Creates a new regexp rule.
 * @class Represents regexp rule.
 * @param {String} value Represents regexp rule.
 * @param {Integer} column The number of the column in the source code.
 * @param {Integer} line The number of the line in the source code.
 */
function CodeRegularExpression(value, column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeRegularExpression";
	this.value = value;
	this.position = {'column': column, 'line': line};	
}

CodeRegularExpression.prototype.toString = function(){ return '\'' + this.value + '\'R'; };

//=================================================================================================

/**
 * Creates a new binary expression.
 * @param {Object} left The left operand.
 * @param {Object} operator The operator.
 * @param {Object} right The right operand.
 * @param {Integer} column The number of the column in the source code.
 * @param {Integer} line The number of the line in the source code.
 */
function CodeBinaryExpression(left, operator, right, column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeBinaryExpression";
	this.position = {'column': column, 'line': line};
	this.left = left;
	this.right = right;
	this.operator = operator;
}

CodeBinaryExpression.prototype = {
	get operands(){	//operands with this operator
		var result = new Array(), operator = this.operator;
		function exploreOperands(operand){
			if(operand instanceof CodeBinaryExpression && operand.operator === operator){
				exploreOperands.call(this, operand.left);
				exploreOperands.call(this, operand.right);			
			}
			else this.push(operand);
		}
		exploreOperands.call(result, this);
		return result;
	}
};

CodeBinaryExpression.prototype.reduce = function(checked){
	var operator = this.operator;
	switch(operator.value){
		case '+=': operator = '+'; break;
		case '-=': operator = '-'; break;
		case '*=': operator = '*'; break;
		case '/=': operator = '/'; break;
		case '^=': operator = '^'; break;
		case '%=': operator = '%'; break;
		case '&=': operator = '&'; break;
		case '|=': operator = '|'; break;
		case '=>':
			if(this.left instanceof CodeInvocationExpression || this.left instanceof CodeIndexerExpression){
				this.left.destination = this.right;
				return this.left;
			}
		default: return this;	
	}
	return new CodeBinaryExpression(this.left, 
		parseOperator('=', true), 
		new CodeBinaryExpression(this.left, operator, this.right, this.position.column, this.position.line), 
		this.position.column, 
		this.position.line);
};

CodeBinaryExpression.parse = function(left, operator, parser, column, line, terminators, callback){
	//moves to the next lexeme
	var lex = parser.shouldNextLexeme();
	return lex instanceof ParserError ? callback(lex) :
	parser.next(terminators, operator.priority, function(err, right){
		return err ? callback(err) : callback(undefined, new this(left, operator, right, column, line));
	}.bind(this));
};

CodeBinaryExpression.prototype.toString = function(){
	return '(' + this.left + ') ' + this.operator.value + ' (' + this.right + ')';
};

//=================================================================================================

/**
 * Creates a new unary expression.
 * @param {Object} operator
 * @param {Object} operand
 * @param {Object} column The number of the column in the source code.
 * @param {Integer} line The number of the line in the source code.
 */
function CodeUnaryExpression(operator, operand, column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeUnaryExpression";
	this.position = {'column': column, 'line': line};
	this.operator = operator;
	this.operand = operand;
}

CodeUnaryExpression.prototype.toString = function(){
	return this.operator.style == "postfix" ? 
		'(' + this.operand + this.operator.value + ')' :
		'(' + this.operator.value + this.operand + ')';
};

CodeUnaryExpression.parse = function(parser, operator, column, line, terminators, callback){
	var lex = parser.shouldNextLexeme();
	return lex instanceof ParserError ? callback(lex):
	parser.next(terminators, operator.priority, function(err, operand){
		return err ? callback(err) : callback(undefined, new this(operator, operand, column, line));
	}.bind(this));
}

//=================================================================================================

function parseArguments(move, separator, terminators, args, callback){	
	if(args instanceof Function) { callback = args; args = new Array(); }
	var lex = move ? this.shouldNextLexeme() : this.lookup;
	if(lex instanceof ParserError) return callback(lex);
	else if(terminators.indexOf(lex) < 0){
		terminators.push(separator);
		this.next(terminators, function(err, expr){
			if(err) return callback(err);
			terminators.pop();
			args.push(expr);
			//parse next arg
			return this.lookup === separator ? parseArguments.call(this, true, separator, terminators, args, callback) : callback(undefined, args);
		}.bind(this));
	}
	//parse next arg
	else return this.lookup === separator ? parseArguments.call(this, true, separator, terminators, args, callback) : callback(undefined, args);
}

/**
 * Creates a new invocation expression.
 * @param {Object} target The call site.
 * @param {Array} args An array of arguments.
 * @param {Integer} column The number of the column in the source code.
 * @param {Integer} line The number of the line in the source code.
 */
function CodeInvocationExpression(target, args, column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeInvocationExpression";
	this.position = {'column': column, 'line': line};
	this.target = target;
	this.arguments = args;
	this.nodeType = "CodeInvocationExpression";
}

CodeInvocationExpression.prototype = {
	get self(){ 
		if(this.target instanceof CodeBinaryExpression && this.target.operator.value === '.') return this.target.left;
	},
	get method(){ 
		if(this.target instanceof CodeBinaryExpression && this.target.operator.value === '.') return this.target.right;
		else return this.target;
	}
};

CodeInvocationExpression.prototype.toString = function(){
	var result = '(' + this.target + ') (';
	if(this.arguments.length > 0)
		this.arguments.forEach(function(a, idx, args){
			result += a + (idx < args.length - 1 ? ', ' : ')');	
		});
	else result += ')';
	if(this.destination) result += ' => ' + this.destination;
	return result;
};

CodeInvocationExpression.parse = function(target, parser, column, line, callback){
	parseArguments.call(parser, true, Lexeme.punctuation[','], [Lexeme.punctuation[')']], function(err, args){
		return err ? callback(err) : callback(undefined, new this(target, args, column, line));
	}.bind(this));
};

//=================================================================================================

/**
 * Initializes a new array expression.
 * @param {Integer} column The number of the column in the source code.
 * @param {Integer} line The number of the line in the source code.
 */
function CodeArrayExpression(column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeArrayExpression";
	this.position = {'column': column, 'line': line};
}

CodeArrayExpression.prototype.toString = function(){
	var result = '[';
	this.elements.forEach(function(e, idx, elements){
		result += e + (idx < elements.length - 1 ? ', ' : '');	
	});
	return result += ']';
};

function parseArrayElements(move, args, callback){
	if(args instanceof Function) { callback = args; args = new Array(); }
	var lex = move && this.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	else if(lex !== Lexeme.punctuation[']'])
		this.next([Lexeme.punctuation[']'], Lexeme.punctuation[',']], function(err, expr){
			if(err) return callback(err);
			args.push(expr);
			return this.lookup === Lexeme.punctuation[','] ? parseArrayElements.call(this, true, args, callback) : callback(undefined, args);
		}.bind(this));
	else return this.lookup === Lexeme.punctuation[','] ? parseArrayElements.call(this, true, args, callback) : callback(undefined, args);
}

CodeArrayExpression.parse = function(parser, column, line, callback){
	parseArrayElements.call(parser, true, function(err, elements){
		if(err) return callback(err);
		this.elements = elements;
		return callback(undefined, this);
	}.bind(new this(column, line)));
};

//=================================================================================================

function CodeArrayContractExpression(element, column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeArrayContractExpression";
	this.position = {'column': column, 'line': line};
	this.element = element;
}

CodeArrayContractExpression.prototype.toString = function(){
	return '(' + this.element + '[])';
};

//=================================================================================================

function CodeIndexerExpression(target, indicies, column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeIndexerExpression";
	this.position = {'column': column, 'line': line};
	this.indicies = indicies;
	this.target = target;
}

CodeIndexerExpression.prototype.toString = function(){
	var result = '(' + this.target + '[';
	this.indicies.forEach(function(e, idx, array){
		result += e + (idx < array.length - 1 ? ', ' : '');	
	});
	result += ']';
	if(this.destination) result += ' => ' + this.destination;
	result += ')';
	return result;
};

CodeIndexerExpression.parse = function(target, parser, column, line, callback){
	//moves to the next lexeme, if it is "," then the current expression is CodeArrayContract
	var lex = parser.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	else if(lex == Lexeme.punctuation[']']) return callback(undefined, new CodeArrayContractExpression(target, column, line));
	//parse indicies	
	parseArrayElements.call(parser, false, function(err, indicies){
		if(err) return callback(err);
		this.indicies = indicies;
		return callback(undefined, this);
	}.bind(new this(target, lex, column, line)));
};

//=================================================================================================

function CodeConditionalExpression(condition, column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeConditionalExpression";
	this.position = {'column': column, 'line': line};
	this.condition = condition;
};

CodeConditionalExpression.prototype.toString = function(){
	return '(' + this.condition + ' ? ' + this['then'] + ' : ' + this['else'] + ')'; 
};

CodeConditionalExpression.parse = function(parser, condition, column, line, terminators, callback){
	//parse then branch	
	var lex = parser.shouldNextLexeme();
	return lex instanceof ParserError ? callback(lex):
		parser.next([Lexeme.punctuation[':']], function(err, _then){
			if(err) return callback(err);
			this['then'] = _then;
			var lex = parser.shouldNextLexeme();
			if(lex instanceof ParserError) return callback(lex);
			parser.next(terminators, function(err, _else){
				if(err) return callback(err);
				this['else'] = _else;
				return callback(undefined, this);
			}.bind(this));
		}.bind(new this(condition, column, line)));
};

//=================================================================================================

function CodeSehExpression(tryCode, column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeSehExpression";
	this.position = {'column': column, 'line': line};
	this['try'] = tryCode;
}

CodeSehExpression.prototype.toString = function(){
	var result = '(' + this['try'];
	if(this['catch']) result += ' !! ' + this['catch'];
	if(this['finally']) result += ' finally ' + this['finally'];
	result += ')';
	return result;
};

CodeSehExpression.parse = function(parser, tryCode, column, line, terminators, callback){
	//parse catch branch	
	var lex = parser.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	terminators.push(Lexeme.punctuation[':']);
	parser.next(terminators, function(err, _catch){
		if(err) return callback(err);
		terminators.pop();
		this['catch'] = _catch;
		//parse finally branch
		switch(parser.lookup){
			case Lexeme.punctuation[':']:
				var lex = parser.shouldNextLexeme();
				if(lex instanceof ParserError) return callback(lex);
				return parser.next(terminators, function(err, _finally){
					if(err) return callback(err);
					this['finally'] = _finally;
					return callback(undefined, this);
				}.bind(this));
			default: return callback(undefined, this);
		}
	}.bind(new this(tryCode, column, line)));
};

//=================================================================================================

function CodeSwitcherExpression(value, column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeSwitcherExpression";
	this.target = value;
	this.position = {'column': column, 'line': line};
	this.cases = new Array();
}

CodeSwitcherExpression.prototype.toString = function(){
	var result = '(' + this.target + ' ?? ';
	if(this.comparer) result += '== : ' + this.comparer + ', ';
	this.cases.forEach(function(c, idx, array){
		c.values.forEach(function(v){
			result += v + ' : ';		
		});
		result += c.handler + (idx < array.length - 1 ? ', ' : '');
	});
	if(this['else']) result += ', any: ' + this['else'];
	result += ')';
	return result;
};

CodeSwitcherExpression.parse = function(parser, target, column, line, terminators, callback){
	var result = new this(target, column, line);
	//parse cases branch	
	async.asyncDoWhile(function(nextCase){
		var lex = parser.shouldNextLexeme();
		if(lex instanceof ParserError) return callback(lex);
		switch(lex){
			case Lexeme.operators['==']:
				lex = parser.nextLexeme(function(lex){ return lex === Lexeme.punctuation[':']; },
							function(column, line, lex){ return ParserError.expected(':', lex, column, line); });
				if(lex instanceof ParserError) return callback(lex);
				lex = parser.shouldNextLexeme();
				if(lex instanceof ParserError) return callback(lex);
				terminators.push(Lexeme.punctuation[',']);
				return parser.next(terminators, function(err, comparer){
					if(err) return callback(err);
					terminators.pop();
					this.comparer = comparer;
					return nextCase();
				}.bind(this));
			case Lexeme.keywords['any']:
				lex = parser.nextLexeme(function(lex){ return lex === Lexeme.punctuation[':']; },
							function(column, line, lex){ return ParserError.expected(':', lex, column, line); });
				if(lex instanceof ParserError) return callback(lex);
				lex = parser.shouldNextLexeme();
				if(lex instanceof ParserError) return callback(lex);
				terminators.push(Lexeme.punctuation[',']);
				return parser.next(terminators, function(err, def){
					if(err) return callback(err);
					terminators.pop();
					this['else'] = def;
					return nextCase();
				}.bind(this));
			default:
				var selectionCase = {values: [], handler: null}, condition = {value: true};
				this.cases.push(selectionCase);
				//parse case values
				async.asyncWhile(function(callback){
					return callback(this.value);
				}.bind(condition), 
				function(nextValue){
					terminators.push(Lexeme.punctuation[','], Lexeme.punctuation[':']);
					parser.next(terminators, function(err, value){
						if(err) return callback(err);
						terminators.pop();
						terminators.pop();
						switch(parser.lookup){
							default:
								selectionCase.handler = value;
								this.value = false;
							return nextValue();
							case Lexeme.punctuation[':']:
								selectionCase.values.push(value);
								lex = parser.shouldNextLexeme();
							return lex instanceof ParserError ? callback(lex) : nextValue();
						}			
					}.bind(this));
				}.bind(condition),
				nextCase);
		}
	}.bind(result),
	//condition
	function(callback){
		return callback(this.lookup === Lexeme.punctuation[',']);
	}.bind(parser),
	//after loop
	function(){
		return callback(undefined, this);
	}.bind(result));
};

//================================================================================================

function CodeContextExpression(column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeContextExpression";
	this.position = {'column': column, 'line': line};
}

CodeContextExpression.prototype.toString = function(){
	return '(' + (this.checked ? 'checked ' : 'unchecked ') + this.expr + ')'; 
};

CodeContextExpression.parse = function(parser, column, line, terminators, callback){
	var result = new this(column, line);
	result.checked = parser.lookup === Lexeme.keywords['checked'];
	var lex = parser.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	else parser.next(terminators, function(err, body){
		if(err) return callback(err);
		this.expr = body;
		return callback(undefined, this);
	}.bind(result));
};

//================================================================================================

function CodeContainerContractExpression(column, line, fields){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeContainerContractExpression";
	this.position = {'column': column, 'line': line};
	this.fields = fields || new Array();
}

function parseFieldDefinitions(separator, terminators, fields, callback){
	if(fields instanceof Function) {callback = fields; fields = new Array(); }
	var lex = this.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	else if(terminators.indexOf(lex) < 0)
		switch(lex){
			case Lexeme.keywords['let']: //parse named slot
				lex = this.nextLexeme(function(lex){ return lex && lex.kind === "nmtoken"; },
										function(column, line){ return ParserError.expectedIdentifier(column, line); });
				if(lex instanceof ParserError) return callback(lex);
				var name = lex.value;
				lex = this.shouldNextLexeme();
				if(lex instanceof ParserError) return callback(lex);
				else if(lex === Lexeme.punctuation[':']){	//parse contract
					lex = this.shouldNextLexeme();
					if(lex instanceof ParserError) return callback(lex);
					terminators.push(separator);
					return this.next(terminators, function(err, contract){
						if(err) return callback(err);
						terminators.pop();
						fields.push({'name': name, contract: contract});
						//next field definition
						return this.lookup === separator ? parseFieldDefinitions.call(this, separator, terminators, fields, callback) : callback(undefined, fields);			
					}.bind(this));
				}
				else fields.push({'name': name});
			break;
			default: //parse contract
				terminators.push(separator);
				return this.next(terminators, function(err, contract){
					if(err) return callback(err);
					terminators.pop();
					fields.push({contract: contract});
					return this.lookup === separator ? parseFieldDefinitions.call(this, separator, terminators, fields, callback) : callback(undefined, fields);
				}.bind(this));
		}
	return this.lookup === separator ? parseFieldDefinitions.call(this, separator, terminators, fields, callback) : callback(undefined, fields);
};

CodeContainerContractExpression.parse = function(parser, column, line, callback){
	parseFieldDefinitions.call(parser, Lexeme.punctuation[','], [Lexeme.operators['>>']], function(err, definitions){
		return err ? callback(err) : callback(undefined, new this(column, line, definitions));
	}.bind(this));
};

CodeContainerContractExpression.prototype.toString = function(){
	var result = '<<';
	this.fields.forEach(function(f, idx, array){
		if(f.name && f.contract) result += 'let ' + f.name + ': ' + f.contract;
		else if(f.name) result += 'let ' + f.name;
		else if(f.contract) result += f.contract;
		result += idx < array.length - 1 ? ', ' : '';
	});
	result += '>>';
	return result;
};

//================================================================================================

/**
 * Represents container expression.
 * @param {Integer} column
 * @param {Integer} line
 * @param {Object} fields
 */
function CodeContainerExpression(column, line, fields){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeContainerExpression";
	this.position = {'column': column, 'line': line};
	this.fields = fields || new Array();
}

CodeContainerExpression.prototype.toString = function(){
	var result = '<';
	this.fields.forEach(function(f, idx, array){
		result += f + (idx < array.length - 1 ? ', ' : '');	
	});
	result += '>';
	return result;
};

CodeContainerExpression.parse = function(parser, column, line, callback){
	parseArguments.call(parser, true, Lexeme.punctuation[','], [Lexeme.operators['>']], function(err, args){
		return err ? callback(err) : callback(undefined, new this(column, line, args));
	}.bind(this));
};

//================================================================================================

function CodeContinueExpression(column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeContinueExpression";
	this.position = {'column': column, 'line': line};
}

CodeContinueExpression.prototype.toString = function(){
	var result = '(continue ';
	this.values.forEach(function(v, idx, array){
		result += v + (idx < array.length - 1 ? ', ' : '');
	});
	if(this.destination) result += ' => ' + this.destination;
	result += ')';
	return result;
};

CodeContinueExpression.parse = function(parser, column, line, terminators, callback){
	terminators.push(Lexeme.operators['=>']);
	parseArguments.call(parser, true, Lexeme.punctuation[','], terminators, function(err, values){
		if(err) return callback(err);
		terminators.pop();
		this.values = values;
		if(parser.lookup === Lexeme.operators['=>']){
			var lex = parser.shouldNextLexeme();
			return lex instanceof ParserError ? callback(lex):
				parser.next(terminators, function(err, destination){
					if(err) return callback(err);
					this.destination = destination;
					return callback(undefined, this);
				}.bind(this));
		}
		else return callback(undefined, this);
	}.bind(new this(column, line)));
};

//================================================================================================

function CodeBreakExpression(column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeBreakExpression";
	this.position = {'column': column, 'line': line};
}

CodeBreakExpression.prototype.toString = function(){
	var result = '(break ';
	this.values.forEach(function(v, idx, array){
		result += v + (idx < array.length - 1 ? ', ' : '');
	});
	if(this.destination) result += ' => ' + this.destination;
	result += ')';
	return result;
};

CodeBreakExpression.parse = CodeContinueExpression.parse;

//================================================================================================

function CodeReturnExpression(column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeReturnExpression";
	this.position = {'column': column, 'line': line};
}

CodeReturnExpression.prototype.toString = function(){
	var result = '(return ' + this.value;
	if(this.destination) result += ' => ' + this.destination;
	result += ')';
	return result;
};

CodeReturnExpression.parse = function(parser, column, line, terminators, callback){
	var lex = parser.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	terminators.push(Lexeme.operators['=>']);
	parser.next(terminators, function(err, value){
		if(err) return callback(err);
		this.value = value;
		terminators.pop();
		//parse destination
		if(parser.lookup === Lexeme.operators['=>']){
			var lex = parser.shouldNextLexeme();
			return lex instanceof ParserError ? callback(lex):
				parser.next(terminators, function(err, destination){
					if(err) return callback(err);
					this.destination = destination;
					return callback(undefined, this);
				}.bind(this));	
		}
		else return callback(undefined, this);
	}.bind(new this(column, line)));
};


//================================================================================================

function CodeFaultExpression(column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeFaultExpression";
	this.position = {'column': column, 'line': line};
}

CodeFaultExpression.prototype.toString = function(){
	var result = '(fault ' + this.error;
	if(this.destination) result += ' => ' + this.destination;
	result += ')';
	return result;
};

CodeFaultExpression.parse = function(parser, column, line, terminators, callback){
	var lex = parser.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	terminators.push(Lexeme.operators['=>']);
	parser.next(terminators, function(err, value){
		if(err) return callback(err);
		this.error = value;
		terminators.pop();
		//parse destination
		if(parser.lookup === Lexeme.operators['=>']){
			var lex = parser.shouldNextLexeme();
			return lex instanceof ParserError ? callback(lex):
				parser.next(terminators, function(err, destination){
					if(err) return callback(err);
					this.destination = destination;
					return callback(undefined, this);
				}.bind(this));	
		}
		else return callback(undefined, this);
	}.bind(new this(column, line)));
};

//================================================================================================

function CodePlaceholderExpression(column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodePlaceholderExpression";
	this.position = {'column': column, 'line': line};
}

CodePlaceholderExpression.prototype.toString = function(){
	return '%%' + this.index + ":" + this['default'];
};

CodePlaceholderExpression.parse = function(parser, column, line, terminators, callback){
	var result = new this(column, line);
	var lex = parser.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	else if(lex.kind === 'integer') result.index = JSON.parse(lex.value);
	else return callback(ParserError.expected('Integer literal', lex, column, line));
	//parses default expr
	lex = parser.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	else if(lex === Lexeme.punctuation[':']){
		lex = parser.shouldNextLexeme();
		return lex instanceof ParserError ? callback(lex):
			parser.next(terminators, function(err, def){
				if(err) return callback(err);
				this['default'] = def;
				return callback(undefined, this);
			}.bind(result));
	}
	else return callback(undefined, result);
};

//================================================================================================

function CodeFunctionExpression(column, line, parameters, body){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeFunctionExpression";
	this.position = {'column': column, 'line': line};
	this.oneWay = false;
	this.parameters = parameters || new Array();
	this.parameters.toString = signatureToString;
	this.implementation = body;
}

CodeFunctionExpression.prototype.toString = function(){
	var result = '(' + (this.oneWay ? '@@' : '@') + this.parameters;
	if(this.implementation) result += ' -> ' + this.implementation;
	result += ')';
	return result;
};

function signatureToString(){
	var result = '';
	this.forEach(function(a, idx, array){
		result += a.name;
		if(a.contract) result += ': ' + a.contract;
		result += idx < array.length - 1 ? ', ' : '';			
	});
	return result;
}

function parseSignature(move, separator, terminators, params, callback){
	if(params instanceof Function){
		callback = params;
		params = new Array();
		params.toString = signatureToString;	
	}
	var lex = move ? this.shouldNextLexeme() : this.lookup;
	if(lex instanceof ParserError) return callback(lex);
	else if(terminators.indexOf(lex) < 0){
		var result = new CodeLetExpression(this.column, this.line);
		result.name = lex.value;
		lex = this.shouldNextLexeme();
		if(lex instanceof ParserError) return callback(lex);
		//Parsing contract
		else switch(this.lookup){
			case Lexeme.punctuation[':']:
				lex = this.shouldNextLexeme();
				if(lex instanceof ParserError) return callback(lex);
				terminators.push(separator);
				return this.next(terminators, function(err, contract){
					if(err) return callback(err);
					terminators.pop();
					result.contract = contract;
					params.push(result);
					return this.lookup === separator ? parseSignature.call(this, true, separator, terminators, params, callback) : callback(undefined, params);
				}.bind(this));
			default: 
				params.push(result);
			break;
		}	
	}
	return this.lookup === separator ? parseSignature.call(this, true, separator, terminators, params, callback) : callback(undefined, params);
}

CodeFunctionExpression.parse = function(parser, column, line, terminators, callback){
	//implementation parser
	function parseImplementation(parser, result, callback){
		switch(parser.lookup){
			case Lexeme.punctuation['->']:
				lex = parser.shouldNextLexeme();
				if(lex instanceof ParserError) return callback(lex);
				return parser.next(terminators, function(err, implementation){
					if(err) return callback(err);
					this.implementation = implementation;
					return callback(undefined, this);
				}.bind(result));
			default:
				delete result.implementation;
				return callback(undefined, result);
		}	
	}
	var result = new this(column, line);
	result.oneWay = parser.lookup === Lexeme.punctuation['@@'];
	var lex = parser.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	else if(lex.kind === 'nmtoken'){
		terminators.push(Lexeme.punctuation['->']); 
		return parseSignature.call(parser, false, Lexeme.punctuation[','], terminators, function(err, parameters){
			if(err) return callback(err);
			this.parameters = parameters;
			terminators.pop();
			return parseImplementation(parser, this, callback);
		}.bind(result));
	}
	else if(lex === Lexeme.punctuation['->']) { result.arguments = []; return parseImplementation(parser, result, callback); }
	else return callback(undefined, result);
};

//================================================================================================

function CodeQuotedExpression(column, line, tree){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeQuotedExpression";
	this.position = {'column': column, 'line': line};
	this.tree = tree;
}

CodeQuotedExpression.parse = function(parser, column, line, terminators, callback){
	var lex = parser.shouldNextLexeme(), result = new CodeQuotedExpression(column, line);
	if(lex instanceof ParserError) return callback(lex);
	//parse operand
	parser.next(terminators, function(err, operand){
		if(err) return callback(err);
		result.tree = operand;
		return callback(undefined, result);
	});
};

CodeQuotedExpression.prototype.toString = function(){
	return "(quoted " + this.tree + ")";
};

//================================================================================================

function CodeWithExpression(column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeWithExpression";
	this.position = {'column': column, 'line': line};
	this.layer = null;
	this.fields = new Array();
}

CodeWithExpression.prototype.toString = function(){
	var result = '(with ';
	if(this.fields.length > 0){
		this.fields.forEach(function(f, i, fields){
			result += f + (i < fields.length - 1 ? ", " : "");
		});
		result += " in ";
	}
	result += this.layer;
	result += this.body + ')';
	return result;
};

CodeWithExpression.parse = function(parser, column, line, terminators, callback){
	var result = new this(column, line);
	//parse layers
	async.asyncWhile(function(callback){ callback(this.lookup !== Lexeme.punctuation['->']); }.bind(parser),
	function(next, exit){
		var lex = parser.shouldNextLexeme(); 
		if(lex instanceof ParserError) return callback(lex);
		else parser.next([Lexeme.punctuation['->'], Lexeme.punctuation[','], Lexeme.operators['in']], function(err, expr){
			if(err) return callback(err);
			switch(parser.lookup){
				case Lexeme.punctuation[',']: 	//next field name expected
					if(expr instanceof CodeIdentifierExpression) this.fields.push(expr.name);
					else return callback(ParserError.expectedIdentifier(parser.column, parser.line));
				return next();
				case Lexeme.punctuation['->']:
					this.layer = expr;
				return exit();
				case Lexeme.operators['in']:
					if(expr instanceof CodeIdentifierExpression) this.fields.push(expr.name);
					else return callback(ParserError.expectedIdentifier(parser.column, parser.line));
					expr = parser.shouldNextLexeme();
					if(expr instanceof ParserError) return callback(expr);
					else return parser.next([Lexeme.punctuation['->']], function(err, layer){
						if(err) return callback(err);
						this.layer = layer;
						return exit();
					}.bind(this));
				default: return callback(ParserError.expected('->', parser.lookup, parser.column, parser.line));
			}
		}.bind(this));
	}.bind(result),
	//parse body
	function(){
		//parse body
		var lex = parser.shouldNextLexeme();//pass through ->
		if(lex instanceof ParserError) return callback(lex);
		parser.next(terminators, function(err, body){
			if(err) return callback(err);
			this.body = body;
			return callback(undefined, this);
		}.bind(this));
	}.bind(result));
};

//================================================================================================

function CodeRepeatExpression(column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeRepeatExpression";
	this.position = {'column': column, 'line': line};
}

CodeRepeatExpression.prototype.toString = function(){
	var result = '(repeat ' + (this.loopVar ? this.loopVar : '') + '->'  + this.body;
	if(this.aggregator) result += ', ' + this.aggregator;
	result += ')';
	return result;
};

CodeRepeatExpression.parse = function(parser, column, line, terminators, callback){
	var result = new this(column, line), lex = parser.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	else if(lex.kind === 'nmtoken'){	//parse loop variable name
		result.loopVar = lex.value;
		lex = parser.shouldNextLexeme();
		if(lex instanceof ParserError) return callback(lex);
	}
	if(lex !== Lexeme.punctuation['->']) return callback(ParserError.expected('->', lex, parser.column, parser.line));
	lex = parser.shouldNextLexeme();	//pass through -> token
	terminators.push(Lexeme.punctuation[',']);
	return parser.next(terminators, function(err, body){
		if(err) return callback(err);
		terminators.pop();
		result.body = body;
		//parse aggregator	
		if(parser.lookup === Lexeme.punctuation[',']){
			var lex = parser.shouldNextLexeme();
			if(lex instanceof ParserError) return callback(lex);
			if(lex.kind === 'operator'){
				this.aggregator = lex.value;
				lex = parser.shouldNextLexeme();
				return lex instanceof ParserError ? callback(lex) : callback(undefined, this);
			}
			else return parser.next(terminators, function(err, aggregator){
				if(err) return callback(err);
				this.aggregator = aggregator;
				return callback(undefined, this);
			}.bind(this));
		}
		else return callback(undefined, this);
	}.bind(result));
};

//================================================================================================

function CodeForExpression(column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeForExpression";
	this.position = {'column': column, 'line': line};
}

CodeForExpression.prototype.toString = function(){
	var result = '(for ' + this.loopVar.name;
	if(this.loopVar.contract) result += ': ' + this.loopVar.contract;
	result += ' in ' + this.source + ' -> ' + this.body;
	if(result.aggregator) result += ', ' + this.aggregator;
	result += ')';
	return result;
};

CodeForExpression.parse = function(parser, column, line, terminators, callback){
	//source collection and loop body parser
	function parseLoopSource(parser, result, callback){
		//pass through IN keyword
		var lex = parser.shouldNextLexeme();
		if(lex instanceof ParserError) return callback(lex);
		//parse iteration source
		parser.next([Lexeme.punctuation['->']], function(err, source){
			if(err) return callback(err);
			this.source = source;
			//pass through ->
			var lex = parser.shouldNextLexeme();
			if(lex instanceof ParserError) return callback(lex);
			//parse body
			terminators.push(Lexeme.punctuation[',']);
			return parser.next(terminators, function(err, body){
				if(err) return callback(err);
				terminators.pop();
				this.body = body;
				//parse aggregator	
				if(parser.lookup === Lexeme.punctuation[',']){
					var lex = parser.shouldNextLexeme();
					if(lex instanceof ParserError) return callback(lex);
					if(lex.kind === 'operator'){
						this.aggregator = lex.value;
						lex = parser.shouldNextLexeme();
						return lex instanceof ParserError ? callback(lex) : callback(undefined, this);
					}
					else return parser.next(terminators, function(err, aggregator){
						if(err) return callback(err);
						this.aggregator = aggregator;
						return callback(undefined, this);
					}.bind(this));
				}
				else return callback(undefined, this);
			}.bind(this));
		}.bind(result));
	}
	var result = new this(column, line),
	//parse loop variable 
	lex = parser.nextLexeme(function(lex){ return lex && lex.kind === 'nmtoken'; },
				function(column, line){ return ParserError.identifierExpected(column, line); });
	if(lex instanceof ParserError) return callback(lex);
	result.loopVar = new CodeLetExpression(parser.column, parser.line);
	result.loopVar.name = lex.value;
	//parse IN or contract
	lex = parser.shouldNextLexeme();
	if(lex instanceof ParserError) return callback(lex);
	else if(lex === Lexeme.punctuation[':']){
		lex = parser.shouldNextLexeme();
		return lex instanceof ParserError? callback(lex):
			parser.next([Lexeme.operators['in']], function(err, contract){
				if(err) return callback(err);
				this.loopVar.contract = contract;
				return parseLoopSource(parser, this, callback);
			}.bind(result));
	}
	else if(lex != Lexeme.operators['in']) return callback(ParserError.expected('in', lex, parser.column, parser.line));
	return parseLoopSource(parser, result, callback);
};

//================================================================================================

function CodeExpansionExpression(column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeExpansionExpression";
	this.position = {'column': column, 'line': line};
	this.arguments = [];
}

CodeExpansionExpression.prototype.toString = function(){
	var result = "(expandq (" + this.target + ") (";
	this.arguments.forEach(function(a, idx, args){
		result += a + (idx < args.length - 1 ? ', ' : '');	
	});
	result += "))";
	return result;
};

CodeExpansionExpression.parse = function(parser, column, line, callback){
	var result = new this(column, line),
	lex = parser.shouldNextLexeme();	//pass through keyword
	if(lex instanceof ParserError) return callback(lex);
	//parse target
	return parser.next([Lexeme.punctuation['(']], function(err, target){
		if(err) return callback(err);
		this.target = target;
		//parse arguments
		parseArguments.call(parser, true, Lexeme.punctuation[','], [Lexeme.punctuation[')']], function(err, args){
			if(err) return callback(err);
			this.arguments = args;
			return callback(undefined, this);
		}.bind(this));
	}.bind(result));
};

//================================================================================================

function CodeScopeExpression(column, line, scope){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeScopeExpression";
	this.position = {'column': column, 'line': line};
	this.scope = scope;
}

CodeScopeExpression.prototype.toString = function(){ return this.scope; };

//================================================================================================

function CodeBreakpointExpression(column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeBreakpointExpression";
	this.position = {'column': column, 'line': line};
}

CodeBreakpointExpression.parse = function(parser, column, line, terminators, callback){
	var result = new this(column, line),
	lex = parser.nextLexeme(function(lex){ return lex && (lex.kind == 'nmtoken' || terminators.indexOf(lex) >= 0); },
				function(column, line, lex){ return ParserError.expectedIdentifier(column, line); });
	if(lex instanceof ParserError) return callback(lex);
	else if(lex.kind === 'nmtoken') result.name = lex.value;
	return callback(undefined, result);
};

CodeBreakpointExpression.prototype.toString = function(){
	return this.name ? 'breakpoint ' + this.name : 'breakpoint';
};

//================================================================================================

function CodeAnyValueExpression(column, line){
	if(arguments.length === 0) return;	//deserialization case
	this.nodeType = "CodeAnyValueExpression";
	this.position = {'column': column, 'line': line};
}

CodeAnyValueExpression.prototype.toString = function(){ return "any"; };

//================================================================================================

/**
 * Represents a block of native JavaScript code.
 * @param {Number} column 
 * @param {Number} line
 * @param {String} code A portion of JavaScript code.
 */
function JavaScriptCode(column, line, code){
	if(arguments.length === 0) return; //deserialization case
	this.position = {"column": column, "line": line};
	this.nodeType = "JavaScriptCode";
	this.code = code;	
}

JavaScriptCode.prototype.toString = function(){
	return "#javascript " + JSON.stringify(this.code); 
};

//================================================================================================
module.exports = {
	'JavaScriptCode': JavaScriptCode,
	'CodeBooleanExpression': CodeBooleanExpression,
	'CodeBuiltInContractExpression': CodeBuiltInContractExpression,
	'CodeIdentifierExpression': CodeIdentifierExpression,
	'CodeIntegerExpression': CodeIntegerExpression,
	'CodeLetExpression': CodeLetExpression,
	'CodeRealExpression': CodeRealExpression,
	'CodeRegularExpression': CodeRegularExpression,
	'CodeBinaryExpression': CodeBinaryExpression,
	'CodeUnaryExpression': CodeUnaryExpression,
	'CodeInvocationExpression': CodeInvocationExpression,
	'CodeArrayExpression': CodeArrayExpression,
	'CodeIndexerExpression': CodeIndexerExpression,
	'CodeArrayContractExpression': CodeArrayContractExpression,
	'CodeConditionalExpression': CodeConditionalExpression,
	'parseExpressionList': parseExpressionList,
	'CodeSehExpression': CodeSehExpression,
	'CodeSwitcherExpression': CodeSwitcherExpression,
	'CodeContextExpression': CodeContextExpression,
	'CodeContainerExpression': CodeContainerExpression,
	'CodeContinueExpression': CodeContinueExpression,
	'CodeBreakExpression': CodeBreakExpression,
	'CodeFaultExpression': CodeFaultExpression,
	'CodePlaceholderExpression': CodePlaceholderExpression,
	'CodeFunctionExpression': CodeFunctionExpression,
	'CodeReturnExpression': CodeReturnExpression,
	'CodeWithExpression': CodeWithExpression,
	'CodeRepeatExpression': CodeRepeatExpression,
	'CodeForExpression': CodeForExpression,
	'CodeExpansionExpression': CodeExpansionExpression,
	'CodeScopeExpression': CodeScopeExpression,
	'parseOperator': parseOperator,
	'CodeBreakpointExpression': CodeBreakpointExpression,
	'CodeStringExpression': CodeStringExpression,
	'CodeContainerContractExpression': CodeContainerContractExpression,
	'CodeAnyValueExpression': CodeAnyValueExpression,
	'CodeQuotedExpression': CodeQuotedExpression,
	'highestOperatorPriority': highestOperatorPriority,
	'associativity': associativity,
	//restores expression tree
	restore: function(tree, loader, result){
		if("nodeType" in tree)
			if(tree.nodeType in this) result = new this[tree.nodeType]();	//creates instance of the standard syntax tree
			else if(tree.$extension in ASYNCSCRIPT_SYNTAX_EXTENSIONS) {
				//load extension and restore tree from it
				loader(tree.$extension);
				result = ASYNCSCRIPT_SYNTAX_EXTENSIONS[tree.$extension].restore.call(this, tree, loader);
			}
			else throw new Error("Unknown expression type: " + tree.nodeType);
		else if(tree === null) return null;
		else if(tree === undefined) return;
		else if(typeof tree === 'boolean' || typeof tree === 'number' || typeof tree === 'string') return tree;
		else if(tree instanceof Array) return tree.map(function(e){ return this.restore(e, loader, result); }, this);
		else result = {};
		//analyze each key
		Object.keys(tree).forEach(function(key){
			result[key] = this.restore(tree[key], loader);
		}, this);
		return result;
	},
	//visit each node in the expression tree
	visit: function(tree, visitor){
		if(tree instanceof Array)
			for(var i = 0; i < tree.length; i++){
				var element = tree[i], result;
				if(result = visitor(element)) tree[i] = result;
				else this.visit(element, visitor);
			}
		else if("nodeType" in tree) {
			Object.keys(tree).forEach(function(name){
				var element = tree[name], result;
				if(result = this.visit(element)) tree[name] = element;
			}, this);
			return visitor(tree);
		}
	},
	//finds the node
	findOne: function(tree, predicate){
		if(tree instanceof Array)
			for(var i = 0, element; i < tree.length; i++)
				if(predicate(element = tree[i])) return element;
				else if(element = this.findOne(element)) return element;
				else continue;
		else if("nodeType" in tree)
			if(predicate(tree)) return tree;
			else return this.findOne(Object.keys(tree).map(function(key){ return this[key]; }, tree));		
	}
};
