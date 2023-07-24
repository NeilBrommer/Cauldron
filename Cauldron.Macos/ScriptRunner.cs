using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cauldron.Core;

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
				body.InnerHtml.AppendHtml(GenerateValueOutput(obj));
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
			.ContinueWith((t) => window.SetScriptRunState(false),
				uiThread);
	}

	private static TagBuilder GenerateValueOutput(object value)
	{
		if (value is string str)
		{
			TagBuilder tag = new("p");
			tag.InnerHtml.Append(str);
			return tag;
		}
		if (value.GetType().IsPrimitive)
		{
			TagBuilder tag = new("p");
			tag.InnerHtml.Append(value.ToString());
			return tag;
		}
		if (value is IEnumerable<object> enumberable)
		{

		}

		return null;
	}

	private static void SetOutputPanelContent(this WebKit.WKWebView webView,
		TagBuilder body)
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

	private const string outputCss = """
		body {
			font-family: -apple-system;
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
