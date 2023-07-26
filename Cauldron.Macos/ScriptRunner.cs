using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cauldron.Core;
using System.Reflection;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Cauldron.Macos;

public static class ScriptRunner
{
	public static void BuildScript(MainWindow window)
	{
		string script = "";
		window.BeginInvokeOnMainThread(() => script = window.ScriptText);

		TagBuilder body = new("body");

		RoslynHostGlobals globals = new(null);
		window.BeginInvokeOnMainThread(() => globals = new RoslynHostGlobals(CreateCauldronWriter(window, body)));

		Task task = Task
			.Run(() => RoslynHost.BuildScript(script, Array.Empty<string>(), globals))
			.ContinueWith(t => window.BeginInvokeOnMainThread(
				() => window.UpdateScriptDiagnostics(t.Result)));
	}

	public static void RunScript(MainWindow window)
	{
		window.SetScriptRunState(true);

		TagBuilder body = new("body");
		string script = window.ScriptText;
		window.ScriptCancellationTokenSource = new CancellationTokenSource();
		TaskScheduler uiThread = TaskScheduler.FromCurrentSynchronizationContext();

		// Clear the output for the new run
		window.ScriptOutputWebView.SetOutputPanelContent(body);

		Task task = Task
			.Run(async () =>
			{
				try
				{
					await RoslynHost.RunScript(script, Array.Empty<string>(),
						new RoslynHostGlobals(CreateCauldronWriter(window, body)),
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

	private static CauldronWriter CreateCauldronWriter(MainWindow window, TagBuilder body)
	{
		return new CauldronWriter (obj =>
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
		style.InnerHtml.AppendHtml(OutputCss);
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

	private static string _outputCss = null;
	private static string OutputCss
	{
		get
		{
			if (_outputCss is null)
			{
				string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../Resources/ScriptOutput.css");
				_outputCss = File.ReadAllText(path);
			}

			return _outputCss;
		}
	}
}
