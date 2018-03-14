using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TsT.Components
{
    public class Config
    {
        private readonly JObject _config;
        private const string FileName = "./config.json";

        public Config()
        {
            try
            {
                var content = File.ReadAllText(FileName);
                _config = JObject.Parse(content);
            }
            catch (Exception)
            {
                _config = new JObject();
            }
        }

        public void SetProperty<T>(string key, T value)
        {
            _config[key] = value.ToString();
        }

        public T GetProperty<T>(string key)
        {
            return _config.Value<T>(key);
        }

        public T GetPropertyOrDefault<T>(string key, T defaultValue)
        {
            var value = GetProperty<T>(key);
            if (value != null)
            {
                return value;
            }

            return defaultValue;
        }

        public IEnumerable<T> GetProperties<T>(string key)
        {
            return _config[key].Values<T>();
        }

        public JObject GetRawConfig() => _config;

        public void Store()
        {
            using (var file = new StreamWriter(FileName))
            {
                var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                file.Write(json);
            }
        }
    }
}