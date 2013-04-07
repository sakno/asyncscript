A new programming language especially designed for NodeJS platform that natively supports the asynchronous programming.

# Features
  * Declarative;
  * Synchronous-like programming style with asynchronous non-blocking execution;
  * Native support for Promise programming pattern;
  * Integration with native JavaScript objects (and existed NodeJS libraries);
  * Compact syntax, easy to use and learn;
  * Written on pure JavaScript;
  * Custom syntax extensions for creating DSL languages

# How to install

    npm install --global asyncscript
    
# How to run

```
//save into program.as
let helloWorld = "Hello, world!";
console.info(helloWorld);
```
```bash
asc run ./program.as
```
You will see `Hello, world!` on the screen

# How to compile
```bash
asc compile ./program.as ./program.js -b 
```
Open `program.js` with text editor and you will see the equivalent JavaScript code.

# Short examples

1. Optional typing
```
let a = 2; 
let b = "str": string;
```

1. Lambda expressions and one-way lambdas:
```
let fact = @i -> (i > 1 ? i * callee(i - 1) : 1);
let oneWay = @@str1, str2 -> puts(str1 + str2); //no return value
//fast signature (specify the signature length, not names)
let sum = @4 -> _0 + _1 + _2 + _3;
```

1. Lambda types (delegates)
```
let Callback = @a, b;
let f = @@a: integer, cb: Callback -> cb(a, a + 20);
```

1. Code quotation, templating and compilation on-the-fly
```
let expr = quoted %%0 + 20 - %%1;
let result = (expandq expr(10, 5)).compile(); //10 + 20 - 5 = 25
```

1. Simple object construction and complex type definition
```
let Point = <<let x: integer, let y: integer>>; //type definition
let pt1 = Point(1, 2); 
let pt2 = <let x = 1 : integer, let y = 10 : integer>; //inline construction
console.info(pt1 is Point); //true
console.info(pt2 is Point); //also true
```

1. Context-oriented programming
```
let OutStream = <<let write: (@@str: string)>>; //contract for output stream
let Logger = <<let out: OutStream>>; //contract for logging subsystem
let obj = <
  let log = (@@str -> _ctx_.out.write(this.name + str)) with Logger, 
  let name = "object name"
>;
//obj.log is not available directly, only inside of the Logger context
let stdout = OutStream(@@str -> console.log(str));
let onlyLogging = obj with <let out = stdout>; //specify the context for the obj
onlyLogging.log("Some log entry"); //obj representation inside of the Logger-compliant context
//onlyLogging.name is not available
```

1. Everything is object
```
let a = "str";
let a_type = $a; //obtains type of the 'a' (== string)
let b = "str2": $a; //declares 'b' with the same type as 'a'
```

1. Compact syntax for conditional expression and 'switch-case'
```
let cond = i > 10 ? "str" : false;
let switch_case = cond ??
  10, 20: "str2", //if cond == 10 OR cond == 20 then return "str2"
  any: "str3";    //default branch
//switch-case supports overriding comparison mechanism
let switch_case2 = 20 ??
  == : @a, b -> a - b < 10, //override comparison mechanism
  12: "str2", //20 - 12 = 8, 8 < 10 = true, "str2" returned
  any: "str3";
```

# License (MIT)
Copyright (C) 2013 Sakno Roman

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
