if("maxTickDepth" in process) process.maxTickDepth = 5000;	//since nodejs 0.9.X, 'thanks' for NodeJS developers...
exports['Syntax analyzing'] = require('./ast.js');
exports['Binary operators'] = require('./binary.js');
exports['Unary operators'] = require('./unary.js');
exports['Invocation'] = require('./invocation.js');
exports['RTL tests'] = require('./common.js');
