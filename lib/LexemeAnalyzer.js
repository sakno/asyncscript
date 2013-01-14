var Lexeme = require('./Lexeme.js'), 
	ParserError = require('./ParserError.js'),
	isDigit = Lexeme.isDigit,
	isAlpha = Lexeme.isAlpha,
	isWhiteSpace = Lexeme.isWhiteSpace;

/**
 * Creates a new lexeme analyzer.
 * @class Represents lexeme stream.
 * @param {String} source Script source code.
 * @api public
 */
function LexemeAnalyzer(source){
	this.source = source;
	this.position = this.column = this.line = 0;
}

/**
 * Resets analyzer to its initial state.
 * @api public
 */
LexemeAnalyzer.prototype.reset = function(){
	this.position = this.column = this.line = 0;
};

/**
 * Returns the next lexeme in the stream.
 * @returns {Lexeme} A new lexeme from the stream.
 * @api public
 */
LexemeAnalyzer.prototype.next = function(){
	var currentChar = this.source[this.position];
	switch(currentChar){
		case '#':
		case '~': return this.lexeme = parseNameToken.call(this);
		case '@': return this.lexeme = parseAtSign.call(this);
		case '`': return this.lexeme = parseGrave.call(this);
		case ':': return this.lexeme = parseColon.call(this);
		case ';': 
		case ',':
		case '(':
		case ')':
		case '{':
		case '}':
		case '[':
		case ']':
			this.column += 1; 
			this.position += 1;
			return this.lexeme = Lexeme.punctuation[currentChar];
		case '\"': return this.lexeme = parseString.call(this);
		case '\'': return this.lexeme = parseVerbatimString.call(this);
		case '+': return this.lexeme = parsePlus.call(this);
		case '-': return this.lexeme = parseMinus.call(this);
		case '=': return this.lexeme = parseEqualSign.call(this);
		case '^': return this.lexeme = parseCircumflex.call(this);
		case '%': return this.lexeme = parsePercent.call(this);
		case '|': return this.lexeme = parseVerticalBar.call(this);
		case '&': return this.lexeme = parseAmpersand.call(this);
		case '*': return this.lexeme = parseAsterisk.call(this);
		case '/': return this.lexeme = parseSlash.call(this);
		case '!': return this.lexeme = parseExclamation.call(this);
		case '?': return this.lexeme = parseQuesMark.call(this);
		case '<': return this.lexeme = parseLt.call(this);
		case '>': return this.lexeme = parseGt.call(this);
		case '$':
		case '.':
			this.column += 1; 
			this.position += 1;
			return this.lexeme = Lexeme.operators[currentChar];
		case '\n': 
			this.column = 0; 
			this.line += 1;
			this.position += 1;
			return this.next();
		case ' ':
		case '\t':
			this.column += 1;
		case '\r': 
			this.position += 1;
			return this.next();
		default:
			if(isDigit(currentChar)) return this.lexeme = parseNumber.call(this);
			else if(isAlpha(currentChar)) return this.lexeme = parseNameToken.call(this);
			else if(isWhiteSpace(currentChar)){
				this.column += 1; 
				this.position += 1; 
				return this.next(); 
			}
			else return ParserError.unknownCharacter(currentChar, this.column, this.line);
		case undefined: return this.lexeme = undefined;
	}
};

function parseVerbatimString(){
	var result = '', ch;
	this.position += 1;
	while((ch = this.source[this.position])){
		switch(ch){
			case '\n':
				this.column = 0;
				this.line += 1;
				break;
			case '\r': break;
			case '\\': ch = '\\\\';
			default:
				this.column += 1;
				break;
		}
		this.position += 1;
		if(ch == '\'') break;
		else result += ch;
	}
	switch(ch = this.source[this.position]){
		case 'R': 
			this.position += 1;
			this.column += 1;
			return new Lexeme(result, 'regexp');
		default: return new Lexeme(result, 'string'); 
	}
}

function parseString(){
	function parseEscapeSequence(){
		var currentChar = this.source[this.position];
		switch(currentChar){
			case '\\':
				this.column += 1;
				switch(currentChar = this.source[++this.position]){
					case 'r': return {escape: true, value: '\r'};
					case 'n': return {escape: true, value: '\n'};
					case 't': return {escape: true, value: '\t'};
					case 'v': return {escape: true, value: '\v'};
					case 'f': return {escape: true, value: '\f'};
					case 'b': return {escape: true, value: '\b'};
					case 'a': return {escape: true, value: '\a'};
					case '0': return {escape: true, value: '\0'};
					case '\"': return {escape: true, value: '\"'};
				}
			default:
				return currentChar && {escape: false, value: currentChar};
		}
	}
	var result = '', ch;
	this.position += 1;
	this.column += 1;
	while((ch = parseEscapeSequence.call(this))){
		this.position += 1;
		this.column += 1;
		if(ch.value == '"' && !ch.escape) break;
		else result += ch.value;
	}
	switch(ch = this.source[this.position]){
		case 'R': 
			this.position += 1;
			this.column += 1;
			return new Lexeme(result, 'regexp');
		default: return new Lexeme(result, 'string');
	}
	return new Lexeme(result, 'string');
}

function parseNumber(){
	var result = this.source[this.position++], ch, kind = 'integer';
	this.column += 1;
	while((ch = this.source[this.position]) && isDigit(ch)){
		result += ch;
		this.column += 1;
		this.position +=1;
	}
	if(ch == '.') {
		kind = 'real';
		result += ch;
		this.column += 1;
		this.position +=1;
		while((ch = this.source[this.position]) && isDigit(ch)){
			result += ch;
			this.column += 1;
			this.position +=1;
		}
		result += '0';
	}
	if(ch == 'i'){
		kind = 'complex';
		this.column += 1;
		this.position +=1;
	}
	return new Lexeme(result, kind);
}

function parseColon(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case ':':
			result += ch;
			this.position += 1;
			this.column += 1;
			return Lexeme.operators[result];
		default:
			return Lexeme.punctuation[result];
	}
}

function parseGt(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case '=':
		case '>':
			result += ch;
			this.position += 1;
			this.column += 1;
		default:
			return Lexeme.operators[result];
	}
}

function parseLt(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case '<':
		case '=':
			result += ch;
			this.position += 1;
			this.column += 1;
		default:
			return Lexeme.operators[result];
	}
}

function parseExclamation(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case '!':
			result += ch;
			this.position += 1;
			this.column += 1;
		break;
		case '=':
			result += ch;
			this.position += 1;
			this.column += 1;
			switch(ch = this.source[this.position]){
				case '=': 
					result += ch;
					this.position += 1;
					this.column += 1;
			}
	}
	return Lexeme.operators[result];
}

function parsePercent(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case '%':
		case '=':
			result += ch;
			this.position += 1;
			this.column += 1;
		default:
			return Lexeme.operators[result];
	}
}

function parseComment(multiline){
	this.position += 1;
	this.column += 1;
	var ch, result = '';
	while(ch = this.source[this.position++]){
		switch(ch){
			case '*':
				if(multiline){
					ch = this.source[this.position++];
					if(ch === '/') return new Lexeme(result, 'comment')
					else result += '*';
				}
			break;
			case '\n': 
				this.column = 0; this.line += 1;
				if(!multiline) return new Lexeme(result, 'comment');
			break;
			case '\r': break;
			default: this.column += 1; result += ch; break; 
		}
		result += ch;
	}
	return new Lexeme(result, 'comment');
}

function parseSlash(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case '=':
			result += ch;
			this.position += 1;
			this.column += 1;
		case '/': return parseComment.call(this, false);
		case '*': return parseComment.call(this, true);
		default:
			return Lexeme.operators[result];
	}
}

function parseAsterisk(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case '*':
		case '=':
			result += ch;
			this.position += 1;
			this.column += 1;
		default:
			return Lexeme.operators[result];
	}
}

function parseAmpersand(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case '&':
		case '=':
			result += ch;
			this.position += 1;
			this.column += 1;
		default:
			return Lexeme.operators[result];
	}
}

function parseVerticalBar(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case '|':
		case '=':
			result += ch;
			this.position += 1;
			this.column += 1;
		default:
			return Lexeme.operators[result];
	}
}

function parseCircumflex(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case '=':
			result += ch;
			this.position += 1;
			this.column += 1;
		default:
			return Lexeme.operators[result];
	}
}

function parseAtSign(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case '@':
			result += ch;
			this.position += 1;
			this.column += 1;
		default:
			return Lexeme.punctuation[result];
	}
}

function parseQuesMark(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case '?':
			result += ch;
			this.position += 1;
			this.column += 1;
		default:
			return Lexeme.operators[result];
	}
}

function parseEqualSign(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case '=':
			result += ch;
			this.position += 1;
			this.column += 1;
			switch(ch = this.source[this.position]){
				case '=': 
					result += ch;
					this.position += 1;
					this.column += 1;
			}
		break;
		case '>':
			result += ch;
			this.position += 1;
			this.column += 1;
		break;
	}
	return Lexeme.operators[result];
}

function parseMinus(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case '>':
			result += ch;
			this.position += 1;
			this.column += 1;
			return Lexeme.punctuation[result];
		case '=':
		case '-':
			result += ch;
			this.position += 1;
			this.column += 1;
		default:
			return Lexeme.operators[result];
	}
}

function parsePlus(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	switch(ch = this.source[this.position]){
		case '=':
		case '+':
			result += ch;
			this.position += 1;
			this.column += 1;
		default:
			return Lexeme.operators[result];
	}
}

function parseGrave(){
	var result = '', ch;
	this.column += 1;
	this.position += 1;
	var checker = function(ch){ return !isDigit(ch); };
	while((ch = this.source[this.position]) && ch !== '`' && checker(ch)){
			result += ch;
			this.position += 1;
			this.column += 1;
			checker = function(ch){ return true; };	//graved identifiers cannot start with digit
	}
	this.position += 1; this.column += 1;
	return new Lexeme(result, 'nmtoken');
}

function parseNameToken(){
	var result = this.source[this.position++], ch;
	this.column += 1;
	while((ch = this.source[this.position]) && (ch == '#' || ch == '~' || isAlpha(ch) || isDigit(ch))){
		result += ch;
		this.position += 1;
		this.column += 1;
	}
	return Lexeme.keywords[result] || Lexeme.operators[result] || new Lexeme(result, 'nmtoken');
}

/**
 * Parses source code.
 * @param {String} source Script source code.
 * @param {Function} callback A callback that receives tokens from the stream.
 */
LexemeAnalyzer.parse = function(source, callback){
	var lex, analyzer = new this(source), state = {next: true};
	while((lex = analyzer.next()) && state.next) callback(lex, analyzer, state);
}

module.exports = LexemeAnalyzer;
