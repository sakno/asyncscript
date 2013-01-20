function nextBreakpoint(flow, bpdata){
	return flow.exit(bpdata.error, bpdata.result);
}

function exitProcess(){ return process.exit(); }

function printContext(flow, bpdata){
	bpdata = bpdata.context;
	console.info("Visible identifiers:");
	Object.keys(bpdata).forEach(function(name){
		switch(name){
			case "result":
			case "this": return;
			default: return console.log("Name: %s \t Value: %s", name, $asyncscript.toString(this[name]));
		}
	}, bpdata || {});
	return flow.next();
}

function forceGarbageCollection(flow){
	//not implemented
	return flow.next();
}

function printQueueState(flow){
	console.info($asyncscript.queue.suspended ? "Suspended" : "Normal");
	return flow.next();
}

function printQueueItemsCount(flow){
	console.info("%s", $asyncscript.queue.count);
	return flow.next();
}

function suspendQueue(flow){
	console.info("Suspending queue: %s", $asyncscript.queue.suspend() ? "OK" : "Already suspended");
	return flow.next();
}

function resumeQueue(flow){
	console.info("Resuming queue: %s", $asyncscript.queue.resume() ? "OK" : "Already resumed");
	return flow.next();
}

function resumeSingleItem(flow){
	console.info("Resuming single item: %s", $asyncscript.queue.resume(1) ? "OK" : "No suspended tasks");
	return flow.next();
}

function printProcessArgs(flow){
	process.argv.forEach(function(val, i){
		console.info("%s = %s", i, val);
	});
	return flow.next();
}

function printProcessArgs(flow){
	console.info(process.cwd());
	return flow.next();
}

function printWorkingDir(flow){
	console.info(process.cwd());
	return flow.next();
}

function printNodejsVersion(flow){
	console.info(process.version);
	return flow.next();
}

function printProcessId(flow){
	console.info(process.pid);
	return flow.next();
}

function printMemoryUsage(flow){
	var mem = process.memoryUsage();
	console.info("V8 Heap Total: %s", mem.heapTotal);
	console.info("V8 Heap Used: %s", mem.heapUsed);
	console.info("Resident Set Size: %s", mem.rss);
	return flow.next();
}

function printEnvironmentVars(flow){
	Object.keys(process.env).forEach(function(name){
		console.info("$%s = %s", name, this[name]);
	}, process.env);
	return flow.next();
}

function printHelp(flow){
	console.info("debugger next \t\t Go to the next breakpoint and return the last execution result");
	console.info("process exit \t\t Terminates the process");
	console.info("process args \t\t Displays process arguments");
	console.info("process cwd \t\t Displays working directory");
	console.info("process id \t\t Displays current process ID");
	console.info("process env \t\t Displays environment variables");
	console.info("process mem \t\t Displays memory usage");
	console.info("nodejs version \t\t Displays NodeJS version");
	console.info("show context \t\t Displays inner identifiers");
	console.info("show help \t\t Displays this message");
	console.info("show program \t\t Displays code typed in debug mode");
	console.info("clear \t\t\t Clears code typed in debug mode");
	console.info("exec \t\t\t Executes code");
	console.info("undo \t\t\t Undo the last code line");
	console.info("force gc \t\t Reserved for future use");
	console.info("queue state \t\t Displays state of the AsyncScript Task Queue");
	console.info("queue count \t\t Displays count of enqueued tasks in the AsyncScript Task Queue");
	console.info("queue suspend \t\t Suspends execution of the enqueued tasks");
	console.info("queue resume \t\t Resumes execution of the enqueued tasks");
	console.info("queue resume single \t Executes a single suspended task");
	console.info("queue names \t\t Displays names of the enqueued tasks (if queue is suspended)");
	return flow.next(); 
}

function printTaskNames(flow){
	$asyncscript.queue.bufferedTasks.forEach(console.info, console);
	return flow.next();
}

function clear(flow, bpdata){
	bpdata.program = [];
	return flow.next();
}

function printProgram(flow, bpdata){
	console.info(bpdata.program.reduce(function(result, line) { return result.length ? result + "\n" + line : line; }, ""));
	return flow.next();
}

function execute(flow, bpdata){
	var lambda = "return @";
	var names = [], values = [];
	Object.keys(bpdata.context).forEach(function(name){
		switch(name){
			case "result":
			case "this": return;
			default: names.push(name); values.push(this[name]); return;
		}
	}, bpdata.context);
	//parameters
	names.forEach(function(name, i, names){
		lambda += name + (i < names.length - 1 ? ", " : "");
	});
	lambda += " -> {" + bpdata.program.reduce(function(result, line) { return result + line; }, "") + "};";
	console.info("PROG %s", lambda);
	//compiles lambda with typed program
	$asyncscript.run(lambda, null, function(err, result){
		if(err) return console.error("ERROR OCCURED:\n%s", bpdata.error = err), flow.next();
		//lambda is compiled successfully
		else if(result instanceof Function && result.isStandaloneLambda)
			return $asyncscript.fork(function(result){
				console.info("%s", $asyncscript.toString(this.result = result));
				return flow.next();
			},
			$asyncscript.invoke(bpdata.context['this'], result, values),
			function(err){
				console.error("ERROR OCCURED:\n%s", $asyncscript.toString(bpdata.error = err));
				return flow.next();
			});
		//this should never be happen
		else return console.error(bpdata.error = "Invalid program written in debug mode"), flow.next();
	});
}

function undoLastCommand(flow, bpdata){
	console.info(bpdata.program.length ? bpdata.program.pop() : "");
	return flow.next();
}

function debuggerQuestion(readline, bpdata, callback){
	var utils = require('util');
	readline.question(utils.format("breakpoint %s at (%s, %s): ", bpdata.name, bpdata.position.column, bpdata.position.line), function(command){
		switch(command){
			case "debugger next": command = nextBreakpoint; break;
			case "process exit": command = exitProcess; break;
			case "process args": command = printProcessArgs; break;
			case "process cwd": command = printWorkingDir; break;
			case "process id": command = printProcessId; break;
			case "process env": command = printEnvironmentVars; break;
			case "process mem": command = printMemoryUsage; break;
			case "nodejs version": command = printNodejsVersion; break;
			case "show context": command = printContext; break;
			case "force gc": command = forceGarbageCollection; break;
			case "queue state": command = printQueueState; break;
			case "queue count": command = printQueueItemsCount; break;
			case "queue suspend": command = suspendQueue; break;
			case "queue resume": command = resumeQueue; break;
			case "queue resume single": command = resumeSingleItem; break;
			case "queue names": command = printTaskNames; break;
			case "show help": command = printHelp; break;
			case "clear": command = clear; break;
			case "exec": command = execute; break;
			case "undo": command = undoLastCommand; break;
			case "show program": command = printProgram; break;
			default: 
				bpdata.program.push(command);
			case "":
			case null:
			case undefined: 
				return debuggerQuestion(readline, bpdata, callback);
		}
		return command({
			"exit": function(err, result){
				readline.close();
				//resumes queue
				$asyncscript.queue.resume();
				return callback(err, result);
			}, 
			"next": function(){ 
				return debuggerQuestion(readline, bpdata, callback); 
			}
		}, 
		bpdata);
	});
}

module.exports.breakpoint = function(context, name, position, callback){
	var readline = require('readline').createInterface({input: process.stdin, output: process.stdout});
	//suspends the queue
	$asyncscript.queue.suspend();
	console.info("AsyncScript Command-Line Debugger");
	console.info("Type 'show help' for help\n");
	return debuggerQuestion(readline, {"context": context, "name": name, "position": position, program: [], result: null}, callback);
};
