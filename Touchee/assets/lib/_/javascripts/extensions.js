String.prototype.camelize = function(first_uppercase){
  if (first_uppercase !== false)
    return this.replace(/((^|_)[a-z])/g, function($1){return $1.toUpperCase().replace('_','');});
  else
    return this.substr(0,1).toLowerCase() + this.substr(1).camelize();
};
String.prototype.underscore = function(){
  return this.replace(/([A-Z])/g, function($1){return "_"+$1.toLowerCase();});
};


// // Execute function call in new 'thread'
// window.spawn = function() {
//   var timeout = typeof arguments[0] == 'number' ? Array.prototype.shift.call(arguments) : 0,
//       obj = Array.prototype.shift.call(arguments),
//       func = Array.prototype.shift.call(arguments),
//       args = arguments;
//   setTimeout(function(){
//     (typeof func == 'function' ? func : obj[func]).apply(obj, args);
//   }, timeout);
// };
// 
// // Create callback function on given function on with as scope the given object
// window.cb = function(obj, func) {
//   return function() {
//     (typeof func == 'function' ? func : obj[func]).apply(obj, arguments);
//   }
// };
// 
// 
// 
// 
// 
// // String extensions
// String.prototype.capitalize = function() {
//  return this.charAt(0).toUpperCase() + this.slice(1);
// };
// String.prototype.numberValue = function() {
//   return Number(this.replace(/[^0-9\.\-]+/, ""));
// };
// String.prototype.trim = function(){
//   return this.replace(/^\s+|\s+$/g, "");
// };
// String.prototype.camelize = function(first_uppercase){
//   if (first_uppercase !== false)
//     return this.replace(/((^|_)[a-z])/g, function($1){return $1.toUpperCase().replace('_','');});
//   else
//     return this.substr(0,1).toLowerCase() + this.substr(1).camelize();
// };
// String.prototype.underscore = function(){
//   return this.replace(/([A-Z])/g, function($1){return "_"+$1.toLowerCase();});
// };
// 
// // Number functions
// Number.prototype.secondsToTime = function() {
//   var hours = Math.floor(this / (60 * 60));
//   var divisor_for_minutes = this % (60 * 60);
//   var minutes = Math.floor(divisor_for_minutes / 60);
//   var divisor_for_seconds = divisor_for_minutes % 60;
//   var seconds = Math.ceil(divisor_for_seconds);
//   var ret = minutes + ":" + (seconds < 10 ? "0" : "") + seconds;
//   if (hours > 0) ret = hours + ":" + (minutes < 10 ? "0" : "") + ret;
//   return ret;
// };
// 
// // Array functions
// Array.equals = function(one, other) {
//   if (one.length != other.length) return false;
//   for (var i = 0; i < one.length; i++)
//     if (one[i] != other[i])
//       return false;
//   return true;
// };
// 
// // Execute function call in new 'thread'
// window.spawnThread = function() {
//   var timeout = typeof arguments[0] == 'number' ? Array.prototype.shift.call(arguments) : 0,
//       obj = Array.prototype.shift.call(arguments),
//       func = Array.prototype.shift.call(arguments),
//       args = arguments;
//   setTimeout(function(){
//     (typeof func == 'function' ? func : obj[func]).apply(obj, args);
//   }, timeout);
// };
// 
// // Timeout function
// function Timeout(fn, interval, scope) {
//   var timeout = this;
//   var id = setTimeout(function(){
//     timeout.clear();
//     fn.call(scope);
//   }, interval);
//   this.cleared = false;
//   this.clear = function () {
//     this.cleared = true;
//     clearTimeout(id);
//   };
// }