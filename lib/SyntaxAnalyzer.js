if(typeof ASYNCSCRIPT_SYNTAX_EXTENSIONS === "undefined") global.ASYNCSCRIPT_SYNTAX_EXTENSIONS = {};

var LexemeAnalyzer = require('./LexemeAnalyzer.js'),
	ParserError = require('./ParserError.js'),
	Lexeme = require('./Lexeme.js'),
	ast = require('./ast.js'),
	async = require('./async_helpers.js');
/**
 * Creates a new syntax analyzer.
 * @class Represents AsyncScript syntax analyzer.
 * @param {String | LexemeAnalyzer} source The source code.
 * @param {Object} extensions Syntactic extensions. Optional.
 */
function SyntaxAnalyzer(source, extensions){
	this.lexer = source instanceof LexemeAnalyzer ? source : new LexemeAnalyzer(source);
	this.extensions = extensions || ASYNCSCRIPT_SYNTAX_EXTENSIONS;
}

SyntaxAnalyzer.prototype = {
	get column(){ return this.lexer.column; },
	get line(){ return this.lexer.line; },
	get lookup(){ return this.lexer.lexeme; }
};

Object.defineProperty(SyntaxAnalyzer, "associativity", {get: function(){ return ast.associativity; }});

Object.defineProperty(SyntaxAnalyzer, "highestOperatorPriority", {get: function(){ return ast.highestOperatorPriority; }});

SyntaxAnalyzer.parseOperator = ast.parseOperator;

SyntaxAnalyzer.prototype.parseExtension = function(extension, column, line, terminators, callback){
	return extension.call(this, column, line, terminators, callback);
};

SyntaxAnalyzer.prototype.parseTree = function(expr, check, terminators, priority, callback){
	//creates a new callback for the subtree parser
	var subtreeCallback = function(check){
		return function(err, expr){ return err ? callback(err): this.parseTree(expr, check, terminators, priority, callback); }.bind(this);
	}.bind(this);
	var column = this.column, line = this.line;
	if(check && !(this.lookup = this.lexer.next())) return callback(ParserError.unexpectedEnd(column, line));
	if(terminators.indexOf(this.lookup) >= 0) return callback(undefined, expr);
	switch(this.lookup){
		//Parse symbol declaration
		case Lexeme.keywords['let']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)):
				ast.CodeLetExpression.parse(this, column, line, terminators, subtreeCallback(false));
		//parse simple built-in contracts
		case Lexeme.keywords['object']: 
		case Lexeme.keywords['function']:
		case Lexeme.keywords['integer']:
		case Lexeme.keywords['typedef']:
		case Lexeme.keywords['boolean']:
		case Lexeme.keywords['real']:
		case Lexeme.keywords['expression']:
		case Lexeme.keywords['regexpr']:
		case Lexeme.keywords['string']:
		case Lexeme.keywords['void']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				this.parseTree(new ast.CodeBuiltInContractExpression(this.lookup.value, column, line), true, terminators, priority, callback);
		case Lexeme.keywords['any']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				this.parseTree(new ast.CodeAnyValueExpression(column, line), true, terminators, priority, callback);
		//scopes
		case Lexeme.keywords['this']:
		case Lexeme.keywords['global']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				this.parseTree(new ast.CodeScopeExpression(column, line, this.lookup.value), true, terminators, priority, callback);
		//for
		case Lexeme.keywords['for']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				ast.CodeForExpression.parse(this, column, line, terminators, subtreeCallback(false));
		case Lexeme.keywords['quoted']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				ast.CodeQuotedExpression.parse(this, column, line, terminators, subtreeCallback(false));
		//expansion
		case Lexeme.keywords['expandq']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				ast.CodeExpansionExpression.parse(this, column, line, subtreeCallback(true)); 
		//loop expression
		case Lexeme.keywords['repeat']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				ast.CodeRepeatExpression.parse(this, column, line, terminators, subtreeCallback(false));
		//return
		case Lexeme.keywords['return']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				ast.CodeReturnExpression.parse(this, column, line, terminators, subtreeCallback(false));
		//continue flow
		case Lexeme.keywords['continue']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				ast.CodeContinueExpression.parse(this, column, line, terminators, subtreeCallback(false));
		//break flow
		case Lexeme.keywords['break']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				ast.CodeBreakExpression.parse(this, column, line, terminators, subtreeCallback(false));
		//fault flow
		case Lexeme.keywords['fault']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				ast.CodeFaultExpression.parse(this, column, line, terminators, subtreeCallback(false));
		//parse boolean literals
		case Lexeme.keywords['true']:
		case Lexeme.keywords['false']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				this.parseTree(new ast.CodeBooleanExpression(this.lookup.value, column, line), true, terminators, priority, callback);
		//parse braces
		case Lexeme.punctuation['(']:
			if(!expr) return (expr = this.shouldNextLexeme()) instanceof ParserError ? callback(expr) : this.parseTree(undefined, false, [Lexeme.punctuation[')']], -Number.MAX_VALUE, subtreeCallback(true));
			else if(priority >= this.constructor.highestOperatorPriority) return callback(undefined, expr);
			else return ast.CodeInvocationExpression.parse(expr, this, column, line, subtreeCallback(true));
		//parse brackets
		case Lexeme.punctuation['[']:
			if(!expr) return ast.CodeArrayExpression.parse(this, column, line, subtreeCallback(true));
			else if(priority >= this.constructor.highestOperatorPriority) return callback(undefined, expr);
			else return ast.CodeIndexerExpression.parse(expr, this, column, line, subtreeCallback(true));
		//parse context
		case Lexeme.keywords['checked']:
		case Lexeme.keywords['unchecked']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				ast.CodeContextExpression.parse(this, column, line, terminators, subtreeCallback(false));
		case Lexeme.punctuation['{']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				 ast.parseExpressionList.call(this, terminators, true, subtreeCallback(true));
		case Lexeme.operators['%%']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				 ast.CodePlaceholderExpression.parse(this, column, line, terminators, subtreeCallback(false));
		case Lexeme.punctuation['@']:
		case Lexeme.punctuation['@@']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				ast.CodeFunctionExpression.parse(this, column, line, terminators, subtreeCallback(false));
		case Lexeme.keywords['breakpoint']:
			return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				ast.CodeBreakpointExpression.parse(this, column, line, terminators, subtreeCallback(true));
		case undefined: return callback();
		default: switch(this.lookup.kind){ //Attempts to parse by lexeme kind
			default: return callback(ParserError.expected(terminators, this.lookup, column, line));
			case 'comment': return this.parseTree(expr, true, terminators, priority, callback);
			case 'nmtoken':
				if(this.extensions[this.lookup.value] instanceof Function) 
				return this.parseExtension(this.extensions[this.lookup.value], column, line, terminators, subtreeCallback(false));
				//parse in binary operator style
				else if(expr) return this.parseTree(undefined, true, terminators, -Number.MAX_VALUE, function(err, arg){
					return err ? 
						callback(err) : 
						callback(undefined, new ast.CodeInvocationExpression(
							new ast.CodeBinaryExpression(expr, ast.parseOperator('.', true), new ast.CodeIdentifierExpression(this.value, column, line), column, line),
							[arg], expr.position.column, expr.position.line)
						);
				}.bind(this.lookup));
				else return this.parseTree(new ast.CodeIdentifierExpression(this.lookup.value, column, line), true, terminators, priority, callback);
			case 'integer':
				return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				this.parseTree(new ast.CodeIntegerExpression(this.lookup.value, column, line), true, terminators, priority, callback); 
			case 'real': 
				return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				this.parseTree(new ast.CodeRealExpression(this.lookup.value, column, line), true, terminators, priority, callback);
			case 'string':
				return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				this.parseTree(new ast.CodeStringExpression(this.lookup.value, column, line), true, terminators, priority, callback);
			case 'regexp': 
				return expr ? callback(ParserError.expectedOperator(this.lookup, column, line)) :
				this.parseTree(new ast.CodeRegularExpression(this.lookup.value, column, line), true, terminators, priority, callback);
			case 'operator':
				var operator = this.constructor.parseOperator(this.lookup, expr);
				if(!expr) switch(operator.value){
					case '<': 
						return ast.CodeContainerExpression.parse(this, column, line, subtreeCallback(true));
					case '<<':
						return ast.CodeContainerContractExpression.parse(this, column, line, subtreeCallback(true));
					case 'with':
						return ast.CodeWithExpression.parse(this, column, line, terminators, subtreeCallback(false));
				}
				switch(operator && operator.arity){
					case 1:
						return expr ? this.parseTree(new ast.CodeUnaryExpression(operator, expr, column, line), true, terminators, priority, callback):
						ast.CodeUnaryExpression.parse(this, operator, column, line, terminators, subtreeCallback(false));
					case 2:
						return operator.priority > priority - operator.associativity ?
						ast.CodeBinaryExpression.parse(expr, operator, this, column, line, terminators, subtreeCallback(false)):
						callback(undefined, expr);
					case 3:
						if(!expr) return callback(ParserError.expectedIdentifier(column, line));
						if(operator.priority > priority - operator.associativity)
							switch(operator.value){
								case '??': return ast.CodeSwitcherExpression.parse(this, expr, column, line, terminators, subtreeCallback(false));
								case '?': return ast.CodeConditionalExpression.parse(this, expr, column, line, terminators, subtreeCallback(false));
								case '!!': return ast.CodeSehExpression.parse(this, expr, column, line, terminators, subtreeCallback(false)); 
							}
						else return callback(undefined, expr);			
				}
			}
	}
	return this.parseTree(expr, true, terminators, priority, callback);
}

/**
 * Parses the current expression.
 * @param {Array} terminators An array of expression terminators.
 * @returns {Object} An expression tree.
 */
SyntaxAnalyzer.prototype.next = function(terminators, priority, callback){
	if(terminators instanceof Function){ callback = terminators; terminators = []; priority = -Number.MAX_VALUE; }
	else if(priority instanceof Function){ callback = priority; priority = -Number.MAX_VALUE; }
	callback = async.asyncCallback(callback);
	if(terminators.length == 0) {
		terminators = [Lexeme.punctuation[';']];
		if(!this.lexer.next()) return callback();
	}
	return this.parseTree(undefined, false, terminators, priority, callback);
};

/**
 * @param {Function} check
 * @param {Function} error
 */
SyntaxAnalyzer.prototype.nextLexeme = function(check, error, callback){
	var column = this.column, line = this.line, lex = this.lexer.next();
	switch(arguments.length){
		//synchronous version
		case 2:
			if(!lex) return ParserError.unexpectedEnd(column, line);
			else if(lex instanceof ParserError || check(lex)) return lex;
			else return error(this.column, this.line, lex);
		//asynchronous version
		case 3:
			callback = async.asyncCallback(callback);
			if(!lex) return callback(ParserError.unexpectedEnd(column, line));
			else if(lex instanceof ParserError) return callback(lex);
			else if(check(lex)) return callback(undefined, lex);
			else return callback(error(this.column, this.line, lex));	
	}
};

SyntaxAnalyzer.prototype.shouldNextLexeme = function(callback){
	function hasLexeme(lex){ return lex; }
	function unexpectedEnd(column, line, lex){ return ParserError.unexpectedEnd(column, line); }
	switch(arguments.length){
		//synchronous version
		case 0: return this.nextLexeme(hasLexeme, unexpectedEnd);
		//asynchronous version
		case 1: return this.nextLexeme(hasLexeme, unexpectedEnd, callback);
	}
};

SyntaxAnalyzer.parse = function(source, callback){
	var analyzer = new this(source);
	analyzer.constructor = this;
	function iteration(err, tree){
		if(err) return callback(err);
		else if(tree && callback(undefined, tree)) return this.next(iteration.bind(this));
	}
	return analyzer.next(iteration);
};

module.exports = SyntaxAnalyzer;
