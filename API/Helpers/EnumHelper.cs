using API.Domain.Enums;
using API.Shared.Models;
using System.ComponentModel;
using System.Reflection;

namespace API.Helpers
{
    public static class EnumHelper
    {
        public static string GetDescription(this object obj, Language language = Language.Arabic)
        {
            return DescriptionAnnotation.GetDescription(obj, language);
        }


        public static List<SelectEnumListDTO> ToSelectableList<T>(params T[] exludedValues)
        {
            return
            (from item in Enum.GetValues(typeof(T)).Cast<T>().Select(x => x).ToList()
             where !exludedValues.Contains(item)
             select new SelectEnumListDTO
             {
                 ID = Convert.ToInt32(item),
                 Name = DescriptionAnnotation.GetDescription(item, Language.Arabic),
             }).ToList();
        }

        public static List<SelectListDTO> ToSelectableList<T>(this IEnumerable<T> items)
        {
            return
            (from item in items
             select new SelectListDTO
             {
                 ID = Convert.ToInt32(item),
                 Name = DescriptionAnnotation.GetDescription(item, Language.Arabic),
             }).ToList();
        }

        public static bool EqualsAny<T>(this T obj, params T[] values) where T : Enum
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (obj.ToString() == values[i].ToString())
                    return true;
            }

            return false;
        }

        public static TEnum[] GetEnumValuesWithAttribute<TEnum, TAttribute>()
            where TEnum : Enum
            where TAttribute : Attribute
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Where(value => IsEnumValueAttributed<TEnum, TAttribute>(value))
                .ToArray();
        }

        public static bool IsEnumValueAttributed<TEnum, TAttribute>(TEnum value)
            where TEnum : Enum
            where TAttribute : Attribute

        {
            var fieldInfo = typeof(TEnum).GetField(value.ToString());

            if (fieldInfo != null)
            {
                var deprecatedAttribute = fieldInfo.GetCustomAttribute<TAttribute>();
                return deprecatedAttribute != null;
            }

            return false;
        }


    }
} 