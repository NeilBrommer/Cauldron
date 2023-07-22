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

		string contents = "<!DOCTYPE html>"
			+ head.RenderAsString()
			+ body.RenderAsString();

		Console.WriteLine("Contents: " + contents);

		webView.LoadHtmlString(new Foundation.NSString(contents), null);
	}
}
