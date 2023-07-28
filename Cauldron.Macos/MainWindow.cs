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
	public CSharpScriptDocument ScriptDocument { get => (CSharpScriptDocument)this.Document; }

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

	public SourceTextView ScriptEditorTextBox
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

	public override void WindowDidLoad()
	{
		base.WindowDidLoad();

		this.Document ??= new CSharpScriptDocument();

		if (this.ScriptDocument.ScriptText == null)
			this.ScriptDocument.ScriptText = new NSString(this.ScriptText);

		this.RunScriptToolbarButton.Activated += RunScript;

		SourceTextView scriptTextBox = this.ScriptEditorTextBox;
		scriptTextBox.OnFinishedTyping += this.BuildScript;
		scriptTextBox.OnTextChanged += this.UpdateDocument;

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

		this.SetDocumentEdited(this.ScriptDocument.IsDocumentEdited);
	}

	public void UpdateDocument(object sender, EventArgs args)
	{
		this.ScriptDocument.ScriptText = new NSString(this.ScriptText);
		this.SetDocumentEdited(this.ScriptDocument.IsDocumentEdited);
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

	partial void NewTabMenuItemClicked(AppKit.NSMenuItem sender)
	{
		this.CreateNewTab();
	}

	public void UpdateScriptDiagnostics(ImmutableArray<Diagnostic> diagnostics)
	{
		ImmutableList<Diagnostic> infoDiagnostics = diagnostics
			.Where(d => d.Severity == DiagnosticSeverity.Info)
			.ToImmutableList();
		ImmutableList<Diagnostic> warningDiagnostics = diagnostics
			.Where(d => d.Severity == DiagnosticSeverity.Warning)
			.ToImmutableList();
		ImmutableList<Diagnostic> errorDiagnostics = diagnostics
			.Where(d => d.Severity == DiagnosticSeverity.Error)
			.ToImmutableList();

		this.DiagnosticsToolbarGroup.SetLabel(infoDiagnostics.Count.ToString(), 0);
		this.DiagnosticsToolbarGroup.SetLabel(warningDiagnostics.Count.ToString(), 1);
		this.DiagnosticsToolbarGroup.SetLabel(errorDiagnostics.Count.ToString(), 2);

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
