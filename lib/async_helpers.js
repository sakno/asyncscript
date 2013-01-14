module.exports.asyncCallback = function(callback){
	return function(){
		process.nextTick(function(){ return callback.apply(null, this); }.bind(arguments));
	};
};

module.exports.asyncWhile = function(condition, body, callback){
	function iteration(){
		condition(function(success){ return success ? body(iteration, callback) : callback(); });
	}
	return process.nextTick(iteration);
};

module.exports.asyncDoWhile = function(body, condition, callback){
	function iteration(){
		//executes body
		body(function(){
			//checks the condition
			condition(function(success){ return success ? iteration() : callback(); });
		}, callback);
	}
	return process.nextTick(iteration);
};
