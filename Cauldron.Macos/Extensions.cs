using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Cauldron.Macos;

public static class Extensions
{
	public static string RenderAsString(this IHtmlContent htmlContent)
	{
		using (var writer = new StringWriter())
		{
			htmlContent.WriteTo(writer, HtmlEncoder.Default);
			return writer.ToString();
		}
	}

	public static string GetCSharpName(this Type type)
	{
		string name = type.Name;

		if (!type.IsGenericType)
			return name;

		StringBuilder sb = new();

		// Get just the name without the generic parameters
		sb.Append(name[..name.IndexOf('`')]);

		// Add the generic parameters surrounded by < and >
		sb.Append('<');
		sb.Append(type.GetGenericArguments()
			.Select(t => t.GetCSharpName())
			.Join(", "));
		sb.Append('>');

		return sb.ToString();
	}

	public static string Join(this IEnumerable<string> strings, string separator)
	{
		return string.Join(separator, strings);
	}
}
