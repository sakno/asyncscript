/*
	AsyncScript standard routines
*/

module.exports = {
	//working directory
	get wdir(){ return process.cwd(); }
};

module.exports.puts = $asyncscript.newLambda(function(obj){
	console.info($asyncscript.toString(obj));
}, Object);

var singleParamSignature = new $asyncscript.Signature(Object);

module.exports.pute = $asyncscript.newLambda(function(obj){
	console.error($asyncscript.toString(obj));
}, Object);

//determines whether the specified value represents an error.
module.exports.isError = function(obj){
	$asyncscript.enterLambdaBody();
	return obj instanceof $asyncscript.Promise && obj.isError;
};
module.exports.isError.__$contract$__ = singleParamSignature;
module.exports.isStandaloneLambda = true;

//determines whether the specified value is synchronized
module.exports.isCompleted = function(obj){
	$asyncscript.enterLambdaBody();
	return obj instanceof $asyncscript.Promise ? obj.isCompleted : true;
};
module.exports.isCompleted.__$contract$__ = singleParamSignature;
module.exports.isCompleted.isStandaloneLambda = true;

//determines whether the specified value is uncompleted
module.exports.isPromise = function(obj){ 
	$asyncscript.enterLambdaBody();
	return obj instanceof $asyncscript.Promise && !obj.isCompleted;
};
module.exports.isPromise.__$contract$__ = singleParamSignature;
module.exports.isPromise.isStandaloneLambda = true;

//determines whether the specified value is reactive
module.exports.isReactive = function(obj){
	$asyncscript.enterLambdaBody(); 
	return obj instanceof $asyncscript.Property; 
};
module.exports.isReactive.__$contract$__ = singleParamSignature;
module.exports.isReactive.isStandaloneLambda = true;

//determines whether the lambda-function is overloaded
module.exports.overloaded = $asyncscript.newLambda(function(f){
	return obj instanceof $asyncscript.OverloadList;
}, Function);

module.exports.argv = process.argv;
module.exports.argv.__$contract$__ = new $asyncscript.Vector(String, module.exports.argv.length);

//obtains a new callback for the specified promise
module.exports.callbackOf = function(obj){
	$asyncscript.enterLambdaBody();
	return $asyncscript.createCallback(obj);
};
module.exports.callbackOf.__$contract$__ = singleParamSignature;
module.exports.callbackOf.isStandaloneLambda = true;

//captures the arguments
module.exports.capture = function(p, sig){
	return p instanceof $asyncscript.Promise && !p.isCompleted ?
		function(){
			this.success($asyncscript.invoke(undefined, sig, arguments));
		}.bind(p):
		null;
};
module.exports.capture.__$contract$__ = new $asyncscript.Promise(Object, $asyncscript.typedef);
module.exports.capture.isStandaloneLambda = true;

//runtime library access
module.exports.queue = $asyncscript.container.create(
	{name: "suspend", value: $asyncscript.newLambda($asyncscript.queue.suspend.bind($asyncscript.queue))},
	{name: "resume", value: $asyncscript.newLambda($asyncscript.queue.resume.bind($asyncscript.queue))}
);

module.exports.eval = $asyncscript.newLambda(function(code){
	var result = new $asyncscript.Promise();
	$asyncscript.run(code, null, result.complete.bind(result));
	return result;
}, String);

module.exports.gc = $asyncscript.newLambda(function() { 
});
