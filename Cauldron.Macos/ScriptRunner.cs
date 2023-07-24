using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cauldron.Core;
using System.Reflection;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace Cauldron.Macos;

public static class ScriptRunner
{
	public static void RunScript(MainWindow window)
	{
		window.SetScriptRunState(true);

		TagBuilder body = new("body");

		window.ScriptOutputWebView.SetOutputPanelContent(body);
		TaskScheduler uiThread = TaskScheduler.FromCurrentSynchronizationContext();

		CauldronWriter writer = new(obj =>
		{
			window.BeginInvokeOnMainThread(() =>
			{
				TagBuilder outputSection = new("section");
				outputSection.AddCssClass("output-section");
				outputSection.InnerHtml.AppendHtml(GenerateValueOutput(obj));
				body.InnerHtml.AppendHtml(outputSection);

				window.ScriptOutputWebView.SetOutputPanelContent(body);
			});

			return Task.CompletedTask;
		});

		window.ScriptCancellationTokenSource = new CancellationTokenSource();

		string script = window.ScriptText;

		Task task = Task
			.Run(async () =>
			{
				try
				{
					await RoslynHost.RunScript(script, Array.Empty<string>(),
						new RoslynHostGlobals(writer),
						window.ScriptCancellationTokenSource.Token);
				}
				catch (Exception ex)
				{
					window.BeginInvokeOnMainThread(() =>
					{
						TagBuilder exTag = new("pre");
						exTag.InnerHtml.Append(ex.ToString());
						body.InnerHtml.AppendHtml(exTag);

						window.ScriptOutputWebView.SetOutputPanelContent(body);
					});
				}
			}, window.ScriptCancellationTokenSource.Token)
			.ContinueWith((t) => window.SetScriptRunState(false), uiThread);
	}

	private static TagBuilder GenerateValueOutput(object value)
	{
		if (value is null)
		{
			TagBuilder tag = new("p");
			TagBuilder code = new("code");
			code.InnerHtml.Append("null");
			tag.InnerHtml.AppendHtml(code);
			return tag;
		}
		if (value.GetType().IsPrimitive || value is string)
		{
			TagBuilder tag = new("p");
			tag.InnerHtml.Append(value.ToString());
			return tag;
		}
		if (value is IEnumerable<object> enumerable)
		{
			return GenerateTable(enumerable);
		}

		TagBuilder defaultTag = new("p");
		defaultTag.InnerHtml.Append(value.ToString());
		return defaultTag;
	}

	private static void SetOutputPanelContent(this WebKit.WKWebView webView, TagBuilder body)
	{
		TagBuilder head = new("head");
		TagBuilder style = new("style");
		style.InnerHtml.AppendHtml(outputCss);
		head.InnerHtml.AppendHtml(style);

		string contents = "<!DOCTYPE html>"
			+ head.RenderAsString()
			+ body.RenderAsString();

		Console.WriteLine("Contents: " + contents);

		webView.LoadHtmlString(new Foundation.NSString(contents), null);
	}

	private static TagBuilder GenerateTable(IEnumerable<object> enumerable)
	{
		var listType = enumerable.GetType().GenericTypeArguments[0];

		if (enumerable.GetType().GenericTypeArguments[0].IsPrimitive
			|| enumerable.GetType().GenericTypeArguments[0] == typeof(string))
			return GenerateSimpleTable(enumerable);

		TagBuilder output = new("table");

		IList<PropertyInfo> properties = enumerable.GetType()
			.GenericTypeArguments[0].GetProperties()
			.Where(p => !p.GetCustomAttributes<DisplayAttribute>(false)
				.Any(a => a.GetAutoGenerateField() == false))
			.ToList();

		// Caption
		TagBuilder caption = new("caption");
		caption.InnerHtml.Append(enumerable.GetType().GetCSharpName());
		output.InnerHtml.AppendHtml(caption);

		// Header Row
		List<TagBuilder> tableHeadCells = properties
			.Select(p =>
			{
				string displayName = p.GetCustomAttributes<DisplayAttribute>(false)
					.Where(a => !string.IsNullOrEmpty(a.GetName()))
					.FirstOrDefault()
					?.GetName()
					?? p.Name;

				TagBuilder th = new("th");
				th.InnerHtml.Append(displayName);

				return th;
			})
			.ToList();

		TagBuilder theadTr = new("tr");
		tableHeadCells.ForEach(th => theadTr.InnerHtml.AppendHtml(th));

		TagBuilder thead = new("thead");
		thead.InnerHtml.AppendHtml(theadTr);
		output.InnerHtml.AppendHtml(thead);

		// Content Rows
		List<TagBuilder> tableRows = enumerable
			.Select(item =>
			{
				List<TagBuilder> cells = properties
					.Select(p =>
					{
						var content = p.GetValue(item);

						TagBuilder td = new("td");
						td.InnerHtml.AppendHtml(GenerateValueOutput(content));
						return td;
					})
					.ToList();

				TagBuilder tr = new("tr");
				cells.ForEach(td => tr.InnerHtml.AppendHtml(td));
				return tr;
			})
			.ToList();

		TagBuilder tbody = new("tbody");
		tableRows.ForEach(tr => tbody.InnerHtml.AppendHtml(tr));
		output.InnerHtml.AppendHtml(tbody);

		return output;
	}

	private static TagBuilder GenerateSimpleTable(IEnumerable<object> enumerable)
	{
		TagBuilder output = new("table");

		// Caption
		TagBuilder caption = new("caption");
		caption.InnerHtml.Append(enumerable.GetType().GetCSharpName());
		output.InnerHtml.AppendHtml(caption);

		// Content Rows
		List<TagBuilder> tableRows = enumerable
			.Select(item =>
			{
				List<TagBuilder> cells = enumerable
					.Select(v =>
					{
						TagBuilder td = new("td");
						td.InnerHtml.AppendHtml(v.ToString());
						return td;
					})
					.ToList();

				TagBuilder tr = new("tr");
				cells.ForEach(td => tr.InnerHtml.AppendHtml(td));
				return tr;
			})
			.ToList();

		TagBuilder tbody = new("tbody");
		tableRows.ForEach(tr => tbody.InnerHtml.AppendHtml(tr));
		output.InnerHtml.AppendHtml(tbody);

		return output;
	}

	private const string outputCss = """
		body {
			font-family: system-ui;
		}

		* {
			box-sizing: border-box;
		}

		.output-section {
			margin-bottom: 1rem;
			padding-left: 6px;
			transition: box-shadow 250ms ease-in-out;
			box-shadow: -2px 0 0 0 rgba(175, 82, 222, 0.25);

			&:hover {
				box-shadow: -4px 0 0 0 rgba(175, 82, 222, 0.5);
			}
		}

		h2 {
			font-size: 1.25rem;
		}

		table {
			border-collapse: collapse;
			margin-bottom: 1rem;
			border: solid 1px lightgray;

			caption {
				margin-bottom: 0.5em;
			}

			th {
				font-weight: bold;
				text-align: center;
				padding: 8px;
				border-bottom: solid 1px lightgray;
			}

			th:not(:last-child) {
				border-right: solid 1px lightgray;
			}

			td {
				padding: 8px;
			}

			tr:nth-child(even) {
				background-color: rgba(0, 0, 0, 0.065);
			}
		}

		code {
			font-family: ui-monospace;
		}
		""";
}
