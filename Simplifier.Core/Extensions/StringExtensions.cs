using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Simplifier.Core.Extensions;

public static class StringExtensions
{
	public static string Slugify(this string phrase)
	{
		if (string.IsNullOrWhiteSpace(phrase))
		{
			return string.Empty;
		}

		var output = phrase.RemoveAccents().ToLower();

		output = Regex.Replace(output, @"[^A-Za-z0-9\s-]", "");

		output = Regex.Replace(output, @"\s+", " ").Trim();

		output = Regex.Replace(output, @"\s", "-");

		return output;
	}

	public static string RemoveAccents(this string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return text;

		text = text.Normalize(NormalizationForm.FormD);
		char[] chars = text
			.Where(c => CharUnicodeInfo.GetUnicodeCategory(c)
			            != UnicodeCategory.NonSpacingMark).ToArray();

		return new string(chars).Normalize(NormalizationForm.FormC);
	}
}