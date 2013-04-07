A new programming language especially designed for NodeJS platform that natively supports the asynchronous programming.

# Features
  * Declarative;
  * Synchronous-like programming style with asynchronous non-blocking execution;
  * Native support for Promise programming pattern;
  * Integration with native JavaScript objects (and existed NodeJS libraries);
  * Easy to use and learn;
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

# Overview
The short overview of the language:
1. Optional typing
```
let a = 2; let b = "str": string;
```
1. Lambda expressions and one-way lambdas:
```
let fact = @i -> (i > 1 ? i * callee(i - 1) : 1);
let oneWay = @@str1, str2 -> puts(str1 + str2); //no return value
//fast signature (specify the signature length, not names)
let sum = @4 -> _0 + _1 + _2 + _3;
```
1. Code quotation, templating and compilation on-the-fly
```
let expr = quoted %%0 + 20 - %%1;
let result = (expandq expr(10, 5)).compile(); //10 + 20 - 5 = 25
```

# License (MIT)
Copyright (C) 2013 Sakno Roman

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
