using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using Microsoft.CSharp.RuntimeBinder;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Touchee {

    public static class JsonExtensions {

        static char[] _splitChars = new char[] { '.', '|', '/', '\\' };

        public static dynamic Get(this JObject obj, string key) {
            var token = JsonExtensions.GetToken(obj, key.Split(_splitChars));
            return token is JValue && (token as JValue).Value == null ? null : token;
        }

        public static dynamic Get(this JObject obj, string key, object def) {
            var token = JsonExtensions.GetToken(obj, key.Split(_splitChars));
            return token is JValue && (token as JValue).Value != null ? token : def;
        }

        public static bool Contains(this JObject obj, string key) {
            var token = obj.Get(key);
            return token != null;
        }

        static JToken GetToken(JObject obj, string[] parts) {
            string part = parts[0];
            JToken item = obj.Children().FirstOrDefault(t => t is JProperty && (t as JProperty).Name == part);

            if (item == null) return null;

            var val = (item as JProperty).Value;

            if (val is JObject && parts.Length > 1) {
                var newParts = new string[parts.Length - 1];
                Array.Copy(parts, 1, newParts, 0, newParts.Length);
                return JsonExtensions.GetToken(val as JObject, newParts);
            }

            else {
                return val;
            }
        }

    }



    public class Config {

        static dynamic _root;
        static string _filename;
        

        public static Config Load(string filename) {
            try {
                var sr = new StreamReader(filename);
                var reader = new JsonTextReader(sr);
                var serializer = new JsonSerializer();
                _root = serializer.Deserialize(reader);
                _filename = filename;
                return new Config(_root);
            }
            catch (Exception e) {
                throw new ArgumentException("Cannot load configuration. Incorrect filename or JSON? :: " + e.Message);
            }
        }

        static void _Save() {
            Config._Save(_filename);
        }

        static void _Save(string filename) {
            string json = JsonConvert.SerializeObject(_root, Formatting.Indented);
            lock(_root) {
                File.WriteAllText(filename, json);
            }
        }

        JObject _config;

        public Config(JObject config) {
            _config = (JObject)config;
        }

        public void Save() {
            Config._Save();
        }

        public void Save(string filename) {
            Config._Save(filename);
        }

        public dynamic Get(string key) {
            return _config.Get(key);
        }

        public dynamic Set(string key, object def) {
            return ((dynamic)_config)[key] = new JValue(def);
        }

        public void Remove(string key) {
            _config.Remove(key);
        }

        public dynamic this[string key] {
            get {
                return this.Get(key);
            }
        }

        public dynamic Get(string key, object def) {
            return _config.Get(key, def);
        }

        public bool Contains(string key) {
            return _config.Contains(key);
        }

        public Config Sub(string key) {
            var token = this.Get(key);
            return token is JObject ? new Config(token as JObject) : null;
        }

    }



}