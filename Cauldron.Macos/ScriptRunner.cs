using System;
using System.Threading;
using System.Threading.Tasks;
using Cauldron.Core;

namespace Cauldron.Macos;

public static class ScriptRunner
{
	public static void RunScript(MainWindow window)
	{
		window.SetScriptRunState(true);
		window.ScriptOutputTextBox.Value = "";
		TaskScheduler uiThread = TaskScheduler.FromCurrentSynchronizationContext();

		CauldronWriter writer = new(obj =>
		{
			if (obj is string str)
			{
				window.BeginInvokeOnMainThread(() =>
					window.ScriptOutputTextBox.Value += str + "\n");
			}

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
						window.ScriptOutputTextBox.Value += ex.ToString());
				}
			}, window.ScriptCancellationTokenSource.Token)
			.ContinueWith((t) => window.SetScriptRunState(false),
				uiThread);
	}
}

