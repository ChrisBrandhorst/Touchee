Logger = new JS.Singleton({
  
  Levels: {
    NONE:   0,
    ERROR:  1,
    WARN:   2,
    INFO:   3,
    DEBUG:  4
  },
  
  level: function(value) {
    if (value) this._level = value;
    return this._level || this.levels.error;
  },
  
  error: function(message) { console.error(message); },
  warn: function(message) { if (this.level() >= this.Levels.WARN) console.warn(message); },
  info: function(message) { if (this.level() >= this.Levels.INFO) console.info(message); },
  debug: function(message) { if (this.level() >= this.Levels.DEBUG) console.debug(message); }
  
});