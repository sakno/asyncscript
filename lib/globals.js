/*
	AsyncScript standard routines
*/

module.exports = {
	//working directory
	get wdir(){ return process.cwd(); },
	//location of the AsyncScript Runtime Components 
	rtldir: __dirname
};

//Determines size of the container
module.exports.sizeof = $asyncscript.newLambda(function(obj){
	if(obj === null || obj === undefined) return 0;
	else if(obj.__$c$__ || 
		obj.__$cc$__ || 
		obj instanceof $asyncscript.Signature || 
		obj instanceof $asyncscript.Vector || 
		obj instanceof $asyncscript.OverloadList) return obj.__$size$__;
	else if(obj instanceof $asyncscript.Property) return arguments.callee(obj.value);
	else return 0;
}, Object);

//extracts the value from the reactive container
module.exports.valueof = $asyncscript.newLambda(function(obj){  
	return obj instanceof $asyncscript.Property ? obj.value : obj;
}, Object);

module.exports.puts = $asyncscript.newLambda(function(obj){
	console.info($asyncscript.toString(obj));
}, Object);

var singleParamSignature = new $asyncscript.Signature(Object);

module.exports.pute = $asyncscript.newLambda(function(obj){
	console.error($asyncscript.toString(obj));
}, Object);

//determines whether the specified value represents an error.
(module.exports.isError = function(obj){
	$asyncscript.enterLambdaBody();
	return obj instanceof $asyncscript.Promise && obj.isError;
}).__$contract$__ = singleParamSignature;

//determines whether the specified value is synchronized
(module.exports.isCompleted = function(obj){
	$asyncscript.enterLambdaBody();
	return obj instanceof $asyncscript.Promise ? obj.isCompleted : true;
}).__$contract$__ = singleParamSignature;

//determines whether the specified value is uncompleted
(module.exports.isPromise = function(obj){ 
	$asyncscript.enterLambdaBody();
	return obj instanceof $asyncscript.Promise && !obj.isCompleted; 
}).__$contract$__ = singleParamSignature;

//determines whether the specified value is reactive
(module.exports.isReactive = function(obj){
	$asyncscript.enterLambdaBody(); 
	return obj instanceof $asyncscript.Property; 
}).__$contract$__ = singleParamSignature;

//determines whether the lambda-function is overloaded
module.exports.overloaded = $asyncscript.newLambda(function(f){
	return obj instanceof $asyncscript.OverloadList;
}, Function);

//loads the specified module
module.exports.use = $asyncscript.newLambda(function(name, cache){
	try{
		return require(name);	//attempts to load through NodeJS
	}
	catch(e){	//attempts to load AsyncScript module
		var path = require('path'), fs = require('fs');
		//attempts to find in modules path
		var filename = path.join(__dirname, "../modules", name);
		if(fs.existsSync(filename)) return require(filename);
		//attempts to find in the root directory of the executed script
		filename = path.dirname(process.argv[1]);	//extracts filename of the executed script
		filename = path.join(filename, name);
		if(fs.existsSync(filename)) return require(filename);
	}
	return null;
}, String, Boolean);

module.exports.eval = $asyncscript.newLambda(function(code){
	try{
		code = $asyncscript.run(code);
	}catch(e){
		code = code instanceof Error ? code : new Error(e);	
	}
	return code;
}, String);

module.exports.argv = process.argv;
module.exports.argv.__$contract$__ = new $asyncscript.Vector(String, module.exports.argv.length);

//obtains a new callback for the specified promise
(module.exports.callbackOf = function(obj){
	$asyncscript.enterLambdaBody();
	return $asyncscript.createCallback(obj);
}).__$contract$__ = singleParamSignature;