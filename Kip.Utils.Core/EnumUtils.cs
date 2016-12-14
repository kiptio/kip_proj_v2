using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kip.Utils.Core
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EnumInfoAttribute : Attribute
    {
        public string Description { get; set; }
        public string Value { get; set; }
    }
    
    public static class EnumUtils
    {
        #region [验证枚举值]
        public static bool CheckIsValidedEnumValue<TEnum>(string val)
            where TEnum : struct
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(o => o.GetValue<TEnum>()).Contains(val);
        }
        #endregion

        #region [获取枚举值列表]
        public static IEnumerable<string> ValueList<TEnum>()
            where TEnum : struct
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(v => v.GetValue());
        }
        #endregion

        #region [获取描述属性值]
        public static string GetDescription<TEnum>(this TEnum value)
             where TEnum : struct
        {
            EnumInfoAttribute attribute = value.GetEnumInfoAttribute<TEnum>();
            return attribute == null ? value.ToString() : attribute.Description;
        }
        public static string GetValue<TEnum>(this TEnum value)
            where TEnum : struct
        {
            EnumInfoAttribute attribute = value.GetEnumInfoAttribute<TEnum>();
            return attribute == null ? value.ToString() : attribute.Value;
        }

        private static EnumInfoAttribute GetEnumInfoAttribute<TEnum>(this TEnum value)
            where TEnum : struct
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            EnumInfoAttribute attribute
                    = Attribute.GetCustomAttribute(field, typeof(EnumInfoAttribute))
                        as EnumInfoAttribute;
            return attribute;
        }
        #endregion

        #region [获取描述属性值]
        public static string GetDescription<TEnum>(object value)
            where TEnum : struct
        {
            var listResult = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Where(o => o.GetValue<TEnum>() == value.ToString());

            return (listResult.Count() > 0) ? listResult.First().GetDescription<TEnum>() : "";
        }
        #endregion
    }
}
