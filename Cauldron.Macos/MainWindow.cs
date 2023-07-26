using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Timers;
using AppKit;
using Cauldron.Macos.SourceWriter;
using Cauldron.Macos.SourceWriter.LanguageFormats;
using Foundation;
using Microsoft.CodeAnalysis;

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

	private SourceTextView ScriptEditorTextBox
	{
		get => (this.MainContentController
			.SplitViewItems[0].ViewController.View as NSScrollView)
			.ContentView.DocumentView as SourceTextView;
	}

	public WebKit.WKWebView ScriptOutputWebView
	{
		get => this.MainContentController
			.SplitViewItems[1].ViewController.View as WebKit.WKWebView;
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

		SourceTextView scriptTextBox = this.ScriptEditorTextBox;
		scriptTextBox.OnFinishedTyping += this.BuildScript;
		scriptTextBox.Font = NSFont.MonospacedSystemFont(new nfloat(14), NSFontWeight.Regular);
		scriptTextBox.AutomaticQuoteSubstitutionEnabled = false;
		scriptTextBox.AutomaticDashSubstitutionEnabled = false;
		scriptTextBox.AutomaticDataDetectionEnabled = false;
		scriptTextBox.AutomaticSpellingCorrectionEnabled = false;
		scriptTextBox.AutomaticTextCompletionEnabled = false;
		scriptTextBox.AutomaticTextReplacementEnabled = false;
		scriptTextBox.AutomaticLinkDetectionEnabled = false;

		scriptTextBox.Formatter = new LanguageFormatter(scriptTextBox, new CSharpDescriptor());
		scriptTextBox.Formatter.Reformat();
	}

	public void BuildScript(object sender, ElapsedEventArgs args)
	{
		ScriptRunner.BuildScript(this);
	}

	public void RunScript(object sender, EventArgs e)
	{
		ScriptRunner.RunScript(this);
	}

	public void CancelScript(object sender, EventArgs e)
	{
		this.ScriptCancellationTokenSource?.Cancel();
	}

	partial void NewTabClicked(AppKit.NSToolbarItem sender)
	{
		this.CreateNewTab();
	}

	partial void NewTabMenuItemClicked(AppKit.NSMenuItem sender)
	{
		this.CreateNewTab();
	}

	public void UpdateScriptDiagnostics(ImmutableArray<Diagnostic> diagnostics)
	{
		Console.WriteLine(diagnostics.Select(d => $"{d.Severity}: {d.GetMessage()}").Join("\n"));

		foreach (Diagnostic diagnostic in diagnostics)
		{
			int start = diagnostic.Location.SourceSpan.Start;
			int end = diagnostic.Location.SourceSpan.End;

			if (start == end && end < this.ScriptText.Length)
				end += 1;
			else if (start == end)
				start -= 1;

			NSRange range = new(start,end);

			this.ScriptEditorTextBox.LayoutManager
				.AddTemporaryAttribute(NSStringAttributeKey.UnderlineStyle,
					new NSNumber((int)(NSUnderlineStyle.Thick | NSUnderlineStyle.PatternDot)),
					range);
			this.ScriptEditorTextBox.LayoutManager
					.AddTemporaryAttribute(NSStringAttributeKey.ToolTip,
						new NSString($"{diagnostic.Id} {diagnostic.GetMessage()}"),
						range);

			if (diagnostic.Severity == DiagnosticSeverity.Error)
				this.ScriptEditorTextBox.LayoutManager
					.AddTemporaryAttribute(NSStringAttributeKey.UnderlineColor, NSColor.SystemRed,
						range);
			else if (diagnostic.Severity == DiagnosticSeverity.Warning)
				this.ScriptEditorTextBox.LayoutManager
					.AddTemporaryAttribute(NSStringAttributeKey.UnderlineColor, NSColor.SystemGreen,
						range);
			else if (diagnostic.Severity == DiagnosticSeverity.Info)
				this.ScriptEditorTextBox.LayoutManager
					.AddTemporaryAttribute(NSStringAttributeKey.UnderlineColor, NSColor.SystemBlue,
						range);
		}
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

	public void CreateNewTab()
	{
		MainWindow newWindow = this.Storyboard.InstantiateInitialController()
			as MainWindow;
		this.Window.AddTabbedWindow(newWindow.Window, NSWindowOrderingMode.Above);
		this.Window.SelectNextTab(this);
	}
}
