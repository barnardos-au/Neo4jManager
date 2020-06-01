using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo4jManager
{
    public class ConfigEditor
    {
        private readonly string configFile;

        public ConfigEditor(string configFile)
        {
            this.configFile = configFile;
        }

        public void SetValue(string key, string value)
        {
            var content = File.ReadAllLines(configFile);
            var newContent = new List<string>();
            var updated = false;

            foreach (var line in content)
            {
                var property = line.Split(new[] { '=' }, 2, StringSplitOptions.None);
                var propKey = property[0];
                if (propKey.StartsWith("#"))
                    propKey = propKey.Substring(1);

                if (propKey == key)
                {
                    if (value == null)
                        propKey = $"#{propKey}";
                    
                    var newLine = $"{propKey}={value}";
                    newContent.Add(newLine);
                    updated = true;
                }
                else
                {
                    newContent.Add(line);
                }
            }

            if (!updated)
            {
                var newLine = $"{key}={value}";
                newContent.Add(newLine);
            }

            File.WriteAllLines(configFile, newContent);
        }

        public string GetValue(string key)
        {
            var content = File.ReadAllLines(configFile);

            return (from line in content
                select line.Split(new[] { '=' }, 2, StringSplitOptions.None)
                into property
                where property[0] == key
                select property[1]).FirstOrDefault();
        }

        public IEnumerable<KeyValuePair<string, string>> FindValues(string searchKey)
        {
            var content = File.ReadAllLines(configFile);

            return from line in content
                select line.Split(new[] { '=' }, 2, StringSplitOptions.None)
                into property
                where property[0].Contains(searchKey) && !property[0].StartsWith("#")
                select new KeyValuePair<string, string>(property[0], property[1]);
        }
    }
}
