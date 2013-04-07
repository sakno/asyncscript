require("./rtl.js");	//loading AsyncScript Runtime Library

exports.LexemeAnalyzer = require('./LexemeAnalyzer.js');
exports.Lexeme = require('./Lexeme.js');
exports.ParserError = require('./ParserError.js');
exports.ast = require('./ast.js');
exports.SyntaxAnalyzer = require('./SyntaxAnalyzer.js');
var compiler = require('./compiler.js');
Object.keys(compiler).forEach(function(c){ exports[c] = this[c]; }, compiler);
//Script loader
require.extensions['.a'] = function(module, filename){
	var fs = require('fs'), source = '"extension std";' + fs.readFileSync(filename);
	return this.run(null, source, false, function(err, result){
		if(err) return this.fault(err);
		else if(result instanceof $asyncscript.Promise) return result.route(this);
		else return this.success(result);
	}.bind(module.exports = new $asyncscript.Promise()));
}.bind(compiler);
