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
	public ImmutableArray<Diagnostic> Diagnostics { get; set; } = ImmutableArray<Diagnostic>.Empty;

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

		scriptTextBox.TextContainerInset = new CoreGraphics.CGSize(8, 8);
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

	public override void PrepareForSegue(NSStoryboardSegue segue, NSObject sender)
	{
		base.PrepareForSegue(segue, sender);

		if (sender is NSSegmentedControl segmentedControl
			&& segmentedControl.Identifier == "DiagnosticsButtons"
			&& segue.DestinationController is DiagnosticsPopoverController diagPopover)
		{
			diagPopover.Severity = segmentedControl.SelectedSegment switch
			{
				0 => DiagnosticSeverity.Info,
				1 => DiagnosticSeverity.Warning,
				2 => DiagnosticSeverity.Error,
				_ => DiagnosticSeverity.Info
			};
		}
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

	public void UpdateScriptDiagnostics(ImmutableArray<Diagnostic> diagnostics)
	{
		this.Diagnostics = diagnostics;

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
		this.DiagnosticsToolbarGroup.SetEnabled(infoDiagnostics.Count != 0, 0);

		this.DiagnosticsToolbarGroup.SetLabel(warningDiagnostics.Count.ToString(), 1);
		this.DiagnosticsToolbarGroup.SetEnabled(warningDiagnostics.Count != 0, 1);

		this.DiagnosticsToolbarGroup.SetLabel(errorDiagnostics.Count.ToString(), 2);
		this.DiagnosticsToolbarGroup.SetEnabled(errorDiagnostics.Count != 0, 2);

		// Mark text in the 
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
					.AddTemporaryAttribute(NSStringAttributeKey.UnderlineColor, NSColor.SystemYellow,
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
}
