using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using YaTools.Yaml;

namespace Touchee {

    static class YamlDoc {
        public static dynamic Load(string fileName) {
            var document = YamlLanguage.FileTo(fileName);
            
            object result;
            if (TryMapValue(document, out result))
                return result;
            
            throw new Exception("Unexpected parsed value");
        }
        
        private static object MapValue(object value) {
            object result;
            TryMapValue(value, out result);
            return result;
        }

        internal static bool TryMapValue(object value, out object result) {
            if (value is string) {
                result = value as string;
                return true;
            }

            if (value is ArrayList) {
                result = (value as ArrayList).Cast<object>().Select(MapValue).ToList();
                return true;
            }

            if (value is Hashtable) {
                result = new ConfigObject(value as Hashtable);
                return true;
            }

            if (value is TaggedScalar) {
                result = (value as TaggedScalar).Value;
                return true;
            }

            result = null;
            return false;
        }
    }

    public class YamlMapping : DynamicObject {
        protected readonly Hashtable _mapping;

        public YamlMapping(Hashtable mapping) {
            _mapping = mapping;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            if (TryGetValue(binder.Name, out result))
                return true;
            
            return base.TryGetMember(binder, out result);
        }

        public virtual bool TryGetValue(string key, out object result) {
            if (_mapping.ContainsKey(key)) {
                var value = _mapping[key];

                if (YamlDoc.TryMapValue(value, out result))
                    return true;
            }

            result = null;
            return false;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
            var key = indexes[0] as string;
            if (key != null) {
                if (TryGetValue(key, out result))
                    return true;
            }

            return base.TryGetIndex(binder, indexes, out result);
        }
    }

    public class ConfigObject : YamlMapping {

        public static ConfigObject Load(string filename) {
            return YamlDoc.Load(filename);
        }

        public ConfigObject(Hashtable mapping) : base(mapping) { }

        public bool ContainsKey(string key) {
            return _mapping.ContainsKey(key);
        }

        public IEnumerable<string> Keys { get {
            return ((ICollection)(_mapping.Keys)).Cast<string>().ToList();
        } }

        static char[] _splitChars = new char[] { '.', '|', '/', '\\' };
        public override bool TryGetValue(string key, out object result) {

            // Check if we have multiple keys
            var parts = key.Split(_splitChars);
            if (parts.Length == 1)
                return base.TryGetValue(key, out result);

            bool found = true;
            object value = this;
            foreach(string part in parts) {

                if (!(value is ConfigObject)) {
                    found = false;
                    break;
                }

                object nodeResult;
                var nextNode = ((ConfigObject)value).TryGetSingleKeyValue(part, out nodeResult);

                if (nextNode)
                    value = nodeResult;
                else {
                    found = false;
                    break;
                }
            }

            result = found ? value : null;

            return found;
        }
        bool TryGetSingleKeyValue(string key, out object result) {
            return base.TryGetValue(key, out result);
        }

        public bool TryGetString(string key, out string result, string defaultValue = null) {
            object value = null;
            TryGetValue(key, out value);

            if (value is string) {
                var str = (string)value;
                if (String.IsNullOrWhiteSpace(str)) {
                    result = defaultValue;
                    return false;
                }
                else {
                    result = str;
                    return true;
                }
            }
            else {
                result = defaultValue;
                return false;
            }
        }

        public bool TryGetBool(string key, out bool result, bool defaultValue = false) {
            string value = null;
            TryGetString(key, out value);

            if (value == null) {
                result = defaultValue;
                return false;
            }
            else {
                result = (value ?? "").ToLower() == "true";
                return true;
            }
        }

        public bool TryGetInt(string key, out int result, int defaultValue = 0) {
            string value = null;
            TryGetString(key, out value);

            if (value == null) {
                result = defaultValue;
                return false;
            }
            else {
                var ok = Int32.TryParse(value, out result);
                if (!ok) result = defaultValue;
                return ok;
            }
        }

        public bool TryGetStringArray(string key, out string[] result) {
            object value = null;
            TryGetValue(key, out value);

            if (value is IEnumerable) {
                result = ((IEnumerable)value).Cast<string>().ToArray();
                return true;
            }
            else if (value is string) {
                result = new string[] { (string)value };
                return true;
            }
            else {
                result = new string[0];
                return false;
            }

        }

        public bool TryGetHashtable(string key, out Hashtable result) {
            object value = null;
            TryGetValue(key, out value);

            if (value is Hashtable) {
                result = (Hashtable)value;
                return true;
            }
            else {
                result = new Hashtable();
                return false;
            }
        }


        public object GetValue(string key, object defaultValue = null) {
            if (defaultValue == null)
                return _mapping[key];
            else {
                object value;
                TryGetValue(key, out value);
                return value ?? defaultValue;
            }
        }

        public string GetString(string key, string defaultValue = null) {
            if (defaultValue == null)
                return (string)_mapping[key];
            else {
                string value;
                TryGetString(key, out value, defaultValue);
                return value;
            }
        }

        public bool GetBool(string key, bool? defaultValue = null) {
            if (defaultValue == null)
                return bool.Parse((string)_mapping[key]);
            else {
                bool value;
                TryGetBool(key, out value, (bool)defaultValue);
                return value;
            }
        }

        public int GetInt(string key, int? defaultValue = 0) {
            if (defaultValue == null)
                return Int32.Parse((string)_mapping[key]);
            else {
                int value;
                TryGetInt(key, out value, (int)defaultValue);
                return value;
            }
        }

        public string[] GetStringArray(string key) {
            string[] value;
            TryGetStringArray(key, out value);
            return value;
        }

        public Hashtable GetHashtable(string key) {
            Hashtable value;
            TryGetHashtable(key, out value);
            return value;
        }

    }

}