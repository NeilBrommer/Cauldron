using System;
using System.Threading;
using AppKit;

namespace Cauldron.Macos;

public partial class MainWindow : NSWindowController
{
	#region Window components

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

	private NSOutlineView SidebarList
	{
		get => (this.SidebarController.View.Subviews[0] as NSScrollView)
			.ContentView.DocumentView as NSOutlineView;
	}

	private NSTextView ScriptEditorTextBox
	{
		get => (this.MainContentController
			.SplitViewItems[0].ViewController.View as NSScrollView)
			.ContentView.DocumentView as NSTextView;
	}

	public NSTextView ScriptOutputTextBox
	{
		get => (this.MainContentController
			.SplitViewItems[1].ViewController.View as NSScrollView)
			.ContentView.DocumentView as NSTextView;
	}

	public string ScriptText { get => this.ScriptEditorTextBox.Value; }

	#endregion

	#region Shared properties

	public CancellationTokenSource ScriptCancellationTokenSource { get; set; }

	#endregion


	public MainWindow (ObjCRuntime.NativeHandle handle) : base (handle) { }

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
		ScriptRunner.RunScript(this);
	}

	public void CancelScript(object sender, EventArgs e)
	{
		this.ScriptCancellationTokenSource?.Cancel();
	}

	public void SetScriptRunState(bool scriptIsRunning)
	{
		if (scriptIsRunning)
		{
			this.RunScriptToolbarButton.Enabled = false;
		}
		else
		{
			this.RunScriptToolbarButton.Enabled = true;
		}
	}
}
