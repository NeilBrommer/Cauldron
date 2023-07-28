using System;
using AppKit;
using Foundation;

namespace Cauldron.Macos;

[Register("CSharpScriptDocument")]
public class CSharpScriptDocument : NSDocument
{
	public NSString _scriptText = new("");
	[Export("ScriptText")]
	public NSString ScriptText
	{
		get => this._scriptText;
		set
		{
			this._scriptText = value;
			if (this.MainWindow != null)
				this.MainWindow.ContentViewController.RepresentedObject = value;

			this.MainWindow?.SetDocumentEdited(this.IsDocumentEdited);
		}
	}
	public NSString SavedScriptText { get; set; }
	public MainWindow MainWindow { get; set; }

	public CSharpScriptDocument() : base() { }
	public CSharpScriptDocument(ObjCRuntime.NativeHandle handle) : base(handle) { }

	public override void MakeWindowControllers()
	{
		NSStoryboard storyboard = NSStoryboard.FromName("Main", null);
		MainWindow windowController = (MainWindow)storyboard
			.InstantiateControllerWithIdentifier("MainWindowController");
		windowController.ContentViewController.RepresentedObject = this.ScriptText;

		windowController.ScriptEditorTextBox.Value = this.ScriptText;
		windowController.ScriptEditorTextBox.Formatter.Reformat();
		windowController.WindowTitleForDocumentDisplayName(this.DisplayName);
		windowController.SynchronizeWindowTitleWithDocumentName();
		windowController.Window.Subtitle = this.FileUrl?.FilePathUrl.Path ?? "";
		windowController.SetDocumentEdited(this.IsDocumentEdited);

		this.AddWindowController(windowController);
	}

	public override bool IsDocumentEdited { get => this.ScriptText != this.SavedScriptText; }

	public override NSData GetAsData(string typeName, out NSError outError)
	{
		outError = null;
		return NSData.FromString(this.ScriptText);
	}

	public override bool ReadFromData(NSData data, string typeName, out NSError outError)
	{
		outError = null;
		this.SavedScriptText = this.ScriptText = new NSString(data, NSStringEncoding.UTF8);
		return true;
	}

	[Export("autosavesInPlace")]
	public static bool AutosaveInPlace()
	{
		return false;
	}
}
