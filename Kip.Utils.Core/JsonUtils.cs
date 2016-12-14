using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kip.Utils.Core
{
    public static class JsonUtils
    {
        public static string Serializer(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static string Serializer<T>(T t)
        {
            return JsonConvert.SerializeObject(t);
        }

        public static T Deserialize<T>(string jsonString)
        {
            var settings = new JsonSerializerSettings();
            settings.DateFormatString = "YYYY-MM-DD";
            settings.ContractResolver = new NewtonJsonPropertyResolver<T>();

            return JsonConvert.DeserializeObject<T>(jsonString, settings);
        }
    }

    #region [JsonPropertyAttribute 替代品]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class NewtonJsonPropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPropertyAttribute"/> class with the specified name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public NewtonJsonPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

    /// <summary>
    /// 替代JsonPropertyAttribute，实现自定义mapping
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NewtonJsonPropertyResolver<T> : DefaultContractResolver
    {
        private Dictionary<string, string> PropertyMappings { get; set; }

        public NewtonJsonPropertyResolver()
        {
            this.PropertyMappings = new Dictionary<string, string>();

            var properties = typeof(T).GetProperties();
            foreach (var propertyInfo in properties)
            {
                var jsonProperty = Attribute.GetCustomAttribute(propertyInfo, typeof(NewtonJsonPropertyAttribute)) as NewtonJsonPropertyAttribute;
                if (jsonProperty != null)
                {
                    PropertyMappings.Add(propertyInfo.Name, jsonProperty.PropertyName);
                }
            }
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            string resolvedName = null;
            var resolved = this.PropertyMappings.TryGetValue(propertyName, out resolvedName);
            return (resolved) ? resolvedName : base.ResolvePropertyName(propertyName);
        }
    }
    #endregion
}
