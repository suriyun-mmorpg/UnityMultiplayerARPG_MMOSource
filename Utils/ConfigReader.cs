using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public class ConfigReader
    {
        public static bool ReadConfigs(Dictionary<string, object> config, string configName, out string result, string defaultValue = null)
        {
            result = defaultValue;

            string envVal = System.Environment.GetEnvironmentVariable(configName);
            if (!string.IsNullOrEmpty(envVal))
            {
                result = envVal;
                return true;
            }

            if (config == null || !config.ContainsKey(configName))
                return false;

            result = (string)config[configName];
            return true;
        }

        public static bool ReadConfigs(Dictionary<string, object> config, string configName, out int result, int defaultValue = -1)
        {
            result = defaultValue;

            string envVal = System.Environment.GetEnvironmentVariable(configName);
            if (!string.IsNullOrEmpty(envVal) && int.TryParse(envVal, out result))
            {
                return true;
            }

            if (config == null || !config.ContainsKey(configName))
                return false;

            result = (int)(long)config[configName];
            return true;
        }

        public static bool ReadConfigs(Dictionary<string, object> config, string configName, out float result, float defaultValue = -1)
        {
            result = defaultValue;

            string envVal = System.Environment.GetEnvironmentVariable(configName);
            if (!string.IsNullOrEmpty(envVal) && float.TryParse(envVal, out result))
            {
                return true;
            }

            if (config == null || !config.ContainsKey(configName))
                return false;

            result = (float)(double)config[configName];
            return true;
        }

        public static bool ReadConfigs(Dictionary<string, object> config, string configName, out bool result, bool defaultValue = false)
        {
            result = defaultValue;

            string envVal = System.Environment.GetEnvironmentVariable(configName);
            if (!string.IsNullOrEmpty(envVal) && bool.TryParse(envVal.ToLower(), out result))
            {
                return true;
            }

            if (config == null || !config.ContainsKey(configName))
                return false;

            result = (bool)config[configName];
            return true;
        }

        public static bool ReadConfigs(Dictionary<string, object> config, string configName, out List<string> result, List<string> defaultValue = null)
        {
            result = defaultValue;

            if (config == null || !config.ContainsKey(configName))
                return false;

            result = new List<string>();
            JArray objResults = (JArray)config[configName];
            foreach (var objResult in objResults)
            {
                result.Add(objResult.Value<string>());
            }
            return true;
        }

        public static bool ReadConfigs<T>(Dictionary<string, object> config, string configName, out List<T> result, List<T> defaultValue = null)
        {
            result = defaultValue;

            if (config == null || !config.ContainsKey(configName))
                return false;

            result = new List<T>();
            JArray objResults = (JArray)config[configName];
            foreach (var objResult in objResults)
            {
                result.Add(JsonConvert.DeserializeObject<T>(objResult.ToString()));
            }
            return true;
        }

        public static bool ReadArgs(string[] args, string argName, out string result, string defaultValue)
        {
            result = defaultValue;

            if (args == null)
                return false;

            List<string> argsList = new List<string>(args);
            if (!argsList.Contains(argName))
                return false;

            int index = argsList.FindIndex(0, a => a.Equals(argName));
            result = args[index + 1];
            return true;
        }

        public static bool ReadArgs(string[] args, string argName, out int result, int defaultValue)
        {
            result = defaultValue;
            string text;
            if (ReadArgs(args, argName, out text, defaultValue.ToString()) && int.TryParse(text, out result))
                return true;
            return false;
        }

        public static bool ReadArgs(string[] args, string argName, out float result, float defaultValue)
        {
            result = defaultValue;
            string text;
            if (ReadArgs(args, argName, out text, defaultValue.ToString()) && float.TryParse(text, out result))
                return true;
            return false;
        }

        public static bool ReadArgs(string[] args, string argName, out bool result, bool defaultValue)
        {
            result = defaultValue;
            string text;
            if (ReadArgs(args, argName, out text, defaultValue.ToString()) && bool.TryParse(text, out result))
                return true;
            return false;
        }

        public static bool ReadArgs(string[] args, string argName, out List<string> result, List<string> defaultValue)
        {
            result = new List<string>();
            string text;
            if (ReadArgs(args, argName, out text, string.Empty))
            {
                result.AddRange(text.Split('|'));
                return true;
            }
            if (defaultValue != null)
                result.AddRange(defaultValue);
            return false;
        }

        public static bool IsArgsProvided(string[] args, string argName)
        {
            if (args == null)
                return false;

            List<string> argsList = new List<string>(args);
            return argsList.Contains(argName);
        }

        public static bool ReadEnv(string envName, out string result, string defaultValue)
        {
            result = defaultValue;
            string text = System.Environment.GetEnvironmentVariable(envName);
            if (!string.IsNullOrWhiteSpace(text))
            {
                result = text;
                return true;
            }
            return false;
        }

        public static bool ReadEnv(string envName, out int result, int defaultValue)
        {
            result = defaultValue;
            string text = System.Environment.GetEnvironmentVariable(envName);
            if (!string.IsNullOrWhiteSpace(text) && int.TryParse(text, out result))
                return true;
            return false;
        }

        public static bool ReadEnv(string envName, out float result, float defaultValue)
        {
            result = defaultValue;
            string text = System.Environment.GetEnvironmentVariable(envName);
            if (!string.IsNullOrWhiteSpace(text) && float.TryParse(text, out result))
                return true;
            return false;
        }

        public static bool ReadEnv(string envName, out bool result, bool defaultValue)
        {
            result = defaultValue;
            string text = System.Environment.GetEnvironmentVariable(envName);
            if (!string.IsNullOrWhiteSpace(text) && bool.TryParse(text, out result))
                return true;
            return false;
        }

        public static bool ReadEnv(string envName, out List<string> result, List<string> defaultValue)
        {
            result = new List<string>();
            string text = System.Environment.GetEnvironmentVariable(envName);
            if (!string.IsNullOrWhiteSpace(text))
            {
                result.AddRange(text.Split('|'));
                return true;
            }
            if (defaultValue != null)
                result.AddRange(defaultValue);
            return false;
        }
    }
}
