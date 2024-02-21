using System.ComponentModel;
using System.Reflection;

namespace Simplifier.Core.Extensions;

public static class EnumExtensions
{
	public static string GetDescription(Enum value)
	{
		return
			value
				.GetType()
				.GetMember(value.ToString())
				.FirstOrDefault()
				?.GetCustomAttribute<DescriptionAttribute>()
				?.Description;
	}

	public static T GetEnumValueFromDescription<T>(string description)
	{
		MemberInfo[] fis = typeof(T).GetFields();

		foreach (var fi in fis)
		{
			DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

			if (attributes is not null && attributes.Length > 0 && attributes[0].Description == description)
				return (T)Enum.Parse(typeof(T), fi.Name);
		}

		return default(T);
	}
}