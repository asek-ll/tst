using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

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