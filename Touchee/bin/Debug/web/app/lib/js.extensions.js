String.prototype.toTitleCase = function() {
  return this.replace(/(^|\s)[a-z]/g, function($1){return $1.toUpperCase();});
};
String.prototype.numberValue = function() {
  return Number(this.replace(/[^0-9\.\-]+/, ""));
};
// String.prototype.encodeForParams = function() {
//   return encodeForParams(this);
// };

// function encodeForParams(str) {
//   return typeof str === 'string' ? encodeURIComponent(str.replace(',', "\\,")) : str;
// }

var second = 1000, minute = 60 * second, hour = 60 * minute;
String.duration = function(ms) {
  var d = "";
  
  var h = Math.floor(ms / hour);
  if (h > 0) d += h + ":";
  ms -= h * hour;
  
  var m = Math.floor(ms / minute);
  if (h > 0 && m < 10) d += "0";
  d += m;
  ms -= m * minute;
  
  if (h > 0)
    d += "h";
  else {
    var s = Math.floor(ms / second);
    
    d += ":";
    if (s < 10) d += "0";
    d += s;
  }
  
  return d;
};





// String.prototype.camelize = function(first_uppercase){
//   if (first_uppercase !== false)
//     return this.replace(/((^|_)[a-z])/g, function($1){return $1.toUpperCase().replace('_','');});
//   else
//     return this.substr(0,1).toLowerCase() + this.substr(1).camelize();
// };
// String.prototype.underscore = function(){
//   return this.replace(/([A-Z])/g, function($1){return "_"+$1.toLowerCase();});
// };
