// Out of global scope
(function(){
  
  // Default locale
  var Locale = {
    
    touchee:  "Touchee",
    unknown:  "Unknown",
    
    back:     "Back",

    models: {
      media: {
        one:  'medium',
        more: 'media'
      }
    },
    
    // For plugins
    p: {},
    
    time: {
      minute: {
        one:  'minute',
        more: 'minutes',
        short: {
          one:  'min.',
          more: 'mins.'
        }  
      }
    },
    
    browser: {
      connecting:   "Connecting to %s...",
      reconnecting: "Connection lost. Reconnecting to %s...",
      moreViews:    "More..."
    },

    queue: {
      add:      "Add",
      edit:     "Edit",
      history:  "History"
    }
  
  };
  
  
  
  // Translate method
  Locale.t = function(item, options) {
    options || (options = {});
    
    var trans;
    
    try { trans = eval('Locale.' + item); }
    catch(e) { trans = null; }
    
    if (typeof trans == 'object' && typeof options.count == 'number')
      trans = trans[options.count == 1 ? 'one' : 'more'];
    else if (trans == null)
      trans = item;
    
    return trans;
  };
  
  
  
  // Define as module
  define({
    root:     Locale,
    'nl-nl':  true
  });
  
})();