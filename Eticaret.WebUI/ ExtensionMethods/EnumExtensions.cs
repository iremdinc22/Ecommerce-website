using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Eticaret.WebUI.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            if (enumValue == null)
                return string.Empty;

            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString());
            if (memberInfo.Length == 0)
                return enumValue.ToString();

            var displayAttribute = memberInfo[0].GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.GetName() ?? enumValue.ToString();
        }
    }
}
