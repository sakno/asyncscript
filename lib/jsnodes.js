function comment(text){ return text ? ('/*' + text + '*/') : ''; }
//=================================================================================================

function JSMemberAccess(obj, memberName){
	this.target = obj;
	this.member = memberName;
}

JSMemberAccess.prototype.toString = function(){
	var result = comment(this.comment) + '(';
	result += this.target + '.' + this.member + ')';
	return result;
};

//=================================================================================================
function JSCall(_this, methodName){
	this['this'] = _this;
	this.method = methodName;
	this.arguments = new Array();
	if(arguments[2] instanceof Array) this.arguments = arguments[2];
	else for(var i = 2; i < arguments.length; i++) this.arguments.push(arguments[i]);
}

JSCall.prototype.toString = function(){
	var result = comment(this.comment) + '(';
	if(this['this'] !== undefined) result += this['this'] + '.';
	result += this.method + '(';
	this.arguments.forEach(function(a, idx, array){
		result += a + (idx < array.length - 1 ? ', ' : '');
	});	
	result += '))';
	return result;
};

//=================================================================================================

function JSVariableRef(name){
	this.name = name;
}

JSVariableRef.prototype.toString = function(){ return this.name; };

//=================================================================================================

function JSStringLiteral(str){
	this.value = str;
}

JSStringLiteral.prototype.toString = function(){
	return comment(this.comment) + '\'' + this.value + '\'';
};

//=================================================================================================

function JSBooleanLiteral(value){
	switch(value){
		case "true": this.value = new Boolean(true); break;
		case "false": this.value = new Boolean(false); break;
		default: this.value = new Boolean(value ? true : false); break; 
	}
}

JSBooleanLiteral.prototype.toString = function(){
	return comment(this.comment) + this.value;
};

function JSFalse(){ JSBooleanLiteral.call(this, false); }

function JSTrue(){ JSBooleanLiteral.call(this, true); }
JSFalse.prototype.toString = JSTrue.prototype.toString = JSBooleanLiteral.prototype.toString;

//=================================================================================================

function JSNumberLiteral(value){
	this.value = value.constructor == Number ? value : JSON.parse(value);
}

JSNumberLiteral.prototype.toString = function(){
	return comment(this.comment) + this.value;
};

//=================================================================================================

function JSNull(){
}

JSNull.prototype.toString = function(){
	return comment(this.comment) + 'null';
};

//=================================================================================================

function JSThis(){
}

JSThis.prototype.toString = function(){
	return comment(this.comment) + 'this';
};


//=================================================================================================

function JSUndefined(value){
}

JSUndefined.prototype.toString = function(){
	return comment(this.comment) + 'undefined';
};

//=================================================================================================

function JSBinaryOperator(left, operator, right){
	this.left = left;
	this.operator = operator;
	this.right = right;
}

JSBinaryOperator.prototype.toString = function(){
	return comment(this.comment) + '(' + this.left + this.operator + this.right + ')';
};

//=================================================================================================

function JSAssignment(left, right){
	JSBinaryOperator.call(this, left, '=', right);
}

JSAssignment.prototype.toString = JSBinaryOperator.prototype.toString;

//=================================================================================================

function bodyToString(){
	var result = '';	
	this.forEach(function(b, idx, array){
		result += '\n' + comment(b.comment) + b + ';';
		if(idx === array.length - 1) result += '\n';	
	});
	return result;
}

function JSFunction(params, body){
	this.parameters = params;
	this.body = body instanceof Array ? body : [body];
	this.body.toString = bodyToString;
}

JSFunction.prototype.toString = function(onlyBody){
	if(onlyBody) return this.body.toString();
	var result = comment(this.comment) + '(function(';
	this.parameters.forEach(function(p, idx, array){
		result += p + (idx < array.length - 1 ? ', ' : '');	
	});
	result += ') {' + this.body + '})';
	return result;
};


//=================================================================================================

function JSScope(body){
	JSFunction.call(this, [], body);
}

JSScope.prototype.toString = JSFunction.prototype.toString;

//=================================================================================================

function JSBlock(body){
	(this.body = body).toString = bodyToString;
}

JSBlock.prototype.toString = function(){ return "{" + this.body + "}"; };

//=================================================================================================

function JSVariableDecl(names){
	this.names = names instanceof Array ? names : [names];
}

JSVariableDecl.prototype.toString = function(){
	if(this.names.length === 0) return "";
	var result = comment(this.comment) + 'var ';
	this.names.forEach(function(n, idx, array){
		result += n + (idx < array.length - 1 ? ', ' : '');	
	});
	return result;
};

//=================================================================================================

function JSObject(fields){
	this.fields = fields;
}

JSObject.prototype.toString = function(){
	var result = comment(this.comment) + '({';
	Object.keys(this.fields).forEach(function(f, idx, array){
		switch(f){
			case "$PROTOTYPE": result += "__proto__"; break;
			default: result += JSON.stringify(f); break;
		}
		result += ": " + this[f] + (idx < array.length - 1 ? ', ' : '');
	}, this.fields);
	result += '})';
	return result
};

//=================================================================================================

function JSNew(ctor){
	this.ctor = ctor;
	this.arguments = new Array();	
	if(arguments[1] instanceof Array) this.arguments = arguments[1];
	else for(var i = 1; i < arguments.length; i++)
		this.arguments.push(arguments[i]);
}

JSNew.prototype.toString = function(){
	var result = comment(this.comment) + 'new ' + this.ctor + '(';
	this.arguments.forEach(function(a, idx, array){
		result += a + (idx < array.length - 1 ? ', ' : '');	
	});
	result += ')';
	return result;
};


//=================================================================================================

function JSReturn(value){
	this.value = value;
}

JSReturn.prototype.toString = function(){
	return comment(this.comment) + "return " + this.value;
};

//=================================================================================================

function JSCode(code){
	this.code = code;
}

JSCode.prototype.toString = function(){
	return comment(this.comment) + this.code;
};

//=================================================================================================

function JSCallee(){
	JSCode.call(this, "arguments.callee");
}

JSCallee.prototype.toString = JSCode.prototype.toString;

//=================================================================================================

function JSNewArray(){
	this.elements = new Array();
	if(arguments[0] instanceof Array) this.elements = arguments[0];
	else for(var i = 0; i < arguments.length; i++) this.elements.push(arguments[i]);
}

JSNewArray.prototype.toString = function(){
	var result = '[';
	this.elements.forEach(function(e, idx, array){
		result += e + (idx < array.length - 1 ? ', ' : '');
	});
	result += ']';
	return result;
};

//=================================================================================================

function JSConditional(condition, _then, _else){
	this['condition'] = condition;
	this['then'] = _then || new JSUndefined();
	this['else'] = _else || new JSUndefined();
}

JSConditional.prototype.toString = function(){
	return '(' + this.condition + ' ? ' + this['then'] + ': ' + this['else'];
}

//=================================================================================================

function JSTryCatchFinally(){
	for(var i = 0; i < arguments.length; i++){
		var body = arguments[i];
		if(body instanceof Array) body.toString = bodyToString;
		switch(i){
			case 0: this['try'] = body; continue;
			case 1: this['catch'] = body; continue;
			case 2: this['finally'] = body; continue;
		}
	}
}

JSTryCatchFinally.prototype.toString = function(){
	var result = "try {" + this['try'] + "}", hasCatch = false;
	if(this['catch']){	
		hasCatch = true;
		result += "catch(" + (this['catch'].hookName || "_") + ") {" + this['catch'].handler + "}";
	}
	if(this['finally']){
		hasCatch = true;
		result += "finally {" + this['finally'] + "}";
	}
	if(!hasCatch) result += 'catch(_){ }';
	return result;
};

//=================================================================================================

module.exports = {
	'JSBlock': JSBlock,
	'JSTryCatchFinally': JSTryCatchFinally,
	'JSNewArray': JSNewArray,
	'JSCallee': JSCallee,
	'JSCode': JSCode,
	'JSReturn': JSReturn,
	'JSScope': JSScope,
	'JSCall': JSCall,
	'JSVariableRef': JSVariableRef,
	'JSStringLiteral': JSStringLiteral,
	'JSFalse': JSFalse,
	'JSTrue': JSTrue,
	'JSBooleanLiteral': JSBooleanLiteral,
	'JSNumberLiteral': JSNumberLiteral,
	'JSNull': JSNull,
	'JSMemberAccess': JSMemberAccess,
	'JSUndefined': JSUndefined,
	'JSAssignment': JSAssignment,
	'JSFunction': JSFunction,
	'JSThis': JSThis,
	'JSVariableDecl': JSVariableDecl,
	'JSObject': JSObject,
	'JSNew': JSNew,
	'JSConditional': JSConditional,
	'JSBinaryOperator': JSBinaryOperator
};
