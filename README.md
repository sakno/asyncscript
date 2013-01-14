A new programming language especially designed for NodeJS platform that supports asynchrony natively.

# Features
  * Declarative;
  * Synchronous-like programming style with asynchronous non-blocking execution;
  * Native supporting of Promise programming pattern;
  * Integration with native JavaScript objects (and existed NodeJS libraries);
  * Easy to use and learn;

# How to install

    npm install --global asyncscript
    
# How to run

```js
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

# License (MIT)
Copyright (C) 2013 Sakno Roman

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
