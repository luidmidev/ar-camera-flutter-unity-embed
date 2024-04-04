using Newtonsoft.Json.Linq;

namespace Extensions
{
    public static class JObjectExtensions
    {
        public static T GetValueOrDefault<T>(this JObject jObject, string key, T defaultValue = default)
        {
            var value = jObject.GetValue(key);
            return value != null ? value.Value<T>() : defaultValue;
        }
    }
}