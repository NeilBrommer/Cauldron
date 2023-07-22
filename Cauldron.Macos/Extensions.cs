using System.IO;
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
}
