use('modules/eval.q');
var a = 2;
const action = @s: string->void 
{
diag.markf("action"); //marks the current action with human-readable label
diag.bp("Break point #1"); //break point
puts(s);
};
action('hello, world!');
var condition = true;
while condition do 
{
puts('Loop iteration');
diag.bp("Break point #2");
};