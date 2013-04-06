/**
 * Creates a new asynchronous callback.
 * @param {Function} callback A callback function to be wrapped.
 * @return {Function} Asynchronous callback.
 */
module.exports.asyncCallback = function(callback){
	return function(){
		process.nextTick(function(){ return callback.apply(null, this); }.bind(arguments));
	};
};

module.exports.loop = function(body, callback){
	function iteration(){
		body(function(){ return process.nextTick(iteration); },
		function(){ return process.nextTick(callback); });
	}
	return process.nextTick(iteration);
};

module.exports.asyncWhile = function(condition, body, callback){
	function iteration(){
		condition(function(success){ 
			return process.nextTick(function(){ return success ? body(iteration, callback) : callback(); });
		});
	}
	return process.nextTick(iteration);
};

module.exports.asyncDoWhile = function(body, condition, callback){
	function iteration(){
		//executes body
		body(function(){
			//checks the condition
			condition(function(success){ 
				return process.nextTick(function(){ return success ? iteration() : callback(); }); 
			});
		}, callback);
	}
	return process.nextTick(iteration);
};
