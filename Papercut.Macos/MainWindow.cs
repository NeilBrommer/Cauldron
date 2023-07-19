using System;
using System.Threading.Tasks;
using AppKit;
using Papercut.Core;

namespace Papercut.Macos;

public partial class MainWindow : NSWindowController
{
	private NSSplitViewController MainContentController
	{
		get => (this.ContentViewController as NSSplitViewController)
			.SplitViewItems[1].ViewController as NSSplitViewController;
	}

	private NSViewController SidebarController
	{
		get => (this.ContentViewController as NSSplitViewController)
			.SplitViewItems[0].ViewController;
	}

	private NSTextView ScriptEditorTextBox
	{
		get => (this.MainContentController
			.SplitViewItems[0].ViewController.View as NSScrollView)
			.ContentView.DocumentView as NSTextView;
	}

	private NSTextView ScriptOutputTextBox
	{
		get => (this.MainContentController
			.SplitViewItems[1].ViewController.View as NSScrollView)
			.ContentView.DocumentView as NSTextView;
	}


	public MainWindow (IntPtr handle) : base (handle) { }

	public override void AwakeFromNib()
	{
		base.AwakeFromNib();

		this.RunScriptToolbarButton.Activated += RunScript;

		NSTextView scriptTextBox = this.ScriptEditorTextBox;
		scriptTextBox.Font = NSFont.MonospacedSystemFont(new nfloat(14), NSFontWeight.Regular);
		scriptTextBox.AutomaticQuoteSubstitutionEnabled = false;
		scriptTextBox.AutomaticDashSubstitutionEnabled = false;
		scriptTextBox.AutomaticDataDetectionEnabled = false;
		scriptTextBox.AutomaticSpellingCorrectionEnabled = false;
		scriptTextBox.AutomaticTextCompletionEnabled = false;
		scriptTextBox.AutomaticTextReplacementEnabled = false;
		scriptTextBox.AutomaticLinkDetectionEnabled = false;
	}

	public void RunScript(object sender, EventArgs e)
	{
		this.RunScriptToolbarButton.Enabled = false;
		this.ScriptOutputTextBox.Value = "";
		TaskScheduler uiThread = TaskScheduler.FromCurrentSynchronizationContext();

		PapercutWriter writer = new(obj =>
		{
			if (obj is string str)
			{
				this.BeginInvokeOnMainThread(() =>
					this.ScriptOutputTextBox.Value += str + "\n");
			}

			return Task.CompletedTask;
		});

		string script = this.ScriptEditorTextBox.Value;

		Task<bool> _ = Task
			.Run(() => RoslynHost.RunScript(script, Array.Empty<string>(),
				new RoslynHostGlobals(writer)))
			.ContinueWith((t) => this.RunScriptToolbarButton.Enabled = true,
				uiThread);
	}
}
