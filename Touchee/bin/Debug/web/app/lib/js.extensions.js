String.prototype.toTitleCase = function() {
  return this.replace(/(^|\s)[a-z]/g, function($1){return $1.toUpperCase();});
};
String.prototype.numberValue = function() {
  return Number(this.replace(/[^0-9\.\-]+/, ""));
};
String.htmlEncodeMap = {
  "&": "&amp;",
  "'": "&#39;",
  '"': "&quot;",
  "<": "&lt;",
  ">": "&gt;"
};
String.prototype.htmlEncode = function() {
  return this.replace(/[&"'\<\>]/g, function(c) {
    return String.htmlEncodeMap[c];
  });
};
String.prototype.encodeForFilter = function() {
  return encodeForFilter(this);
};

function encodeForFilter(str) {
  return typeof str === 'string' ? encodeURIComponent(str.replace(',', "\\,")) : str;
}







// String.prototype.camelize = function(first_uppercase){
//   if (first_uppercase !== false)
//     return this.replace(/((^|_)[a-z])/g, function($1){return $1.toUpperCase().replace('_','');});
//   else
//     return this.substr(0,1).toLowerCase() + this.substr(1).camelize();
// };
// String.prototype.underscore = function(){
//   return this.replace(/([A-Z])/g, function($1){return "_"+$1.toLowerCase();});
// };
