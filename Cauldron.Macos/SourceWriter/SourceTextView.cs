﻿using System;
using System.Timers;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Cauldron.Macos.SourceWriter;

[Register("SourceTextView")]
public class SourceTextView : NSTextView
{
	#region Static Constants

	/// <summary>Defines the constant Unicode value of the enter key.</summary>
	public const int EnterKey = 13;

	/// <summary>Defines the constant Unicode value of the tab key.</summary>
	public const int TabKey = 9;

	/// <summary>Defines the constant Unicode value of the shift-tab key.</summary>
	public const int ShiftTabKey = 25;

	#endregion

	#region Private Variables

	/// <summary>The current language formatter used to highlight syntax.</summary>
	private LanguageFormatter _formatter;

	/// <summary>Should the editor auto complete closures.</summary>
	private bool _completeClosures = true;

	/// <summary>Should the editor auto wrap selected text in. </summary>
	private bool _wrapClosures = true;

	/// <summary>
	/// Should the edit select the section of text that has just been wrapped in a closure.
	/// </summary>
	private bool _selectAfterWrap = true;

	/// <summary>Should the editor provide auto completion of partial words.</summary>
	private bool _allowAutoComplete = true;

	/// <summary>
	/// Should the editor auto complete keywords as defined in the current language.
	/// </summary>
	private bool _autoCompleteKeywords = true;

	/// <summary>Should the editor use the default words list for auto complete.</summary>
	private bool _autoCompleteDefaultWords = true;

	/// <summary>Should the editor only use default words if the keyword list is empty.</summary>
	private bool _defaultWordsOnlyIfKeywordsEmpty = true;

	private LineNumberRuler LineNumberRuler;

	#endregion

	#region Computed Properties

	/// <summary>
	/// Gets or sets the <see cref="LanguageFormatter"/> used to perform
	/// syntax highlighting on this <c>NSTextView</c> containing the contents of the document being
	/// edited.
	/// </summary>
	/// <value>The <see cref="LanguageFormatter"/> for the selected language.</value>
	[Export("Formatter")]
	public LanguageFormatter Formatter
	{
		get { return _formatter; }
		set
		{
			WillChangeValue("Formatter");
			_formatter = value;
			DidChangeValue("Formatter");
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="SourceTextView"/> allows auto complete
	/// of partial words.
	/// </summary>
	/// <value><c>true</c> if allows auto complete; otherwise, <c>false</c>.</value>
	[Export("AllowAutoComplete")]
	public bool AllowAutoComplete
	{
		get { return _allowAutoComplete; }
		set
		{
			WillChangeValue("AllowAutoComplete");
			_allowAutoComplete = value;
			DidChangeValue("AllowAutoComplete");
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="SourceTextView"/> auto completes keywords.
	/// </summary>
	/// <value><c>true</c> if auto completes keywords; otherwise, <c>false</c>.</value>
	[Export("AutoCompleteKeywords")]
	public bool AutoCompleteKeywords
	{
		get { return _autoCompleteKeywords; }
		set
		{
			WillChangeValue("AutoCompleteKeywords");
			_autoCompleteKeywords = value;
			DidChangeValue("AutoCompleteKeywords");
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="ASourceTextView"/> auto completes
	/// default words.
	/// </summary>
	/// <value><c>true</c> if auto complete default words; otherwise, <c>false</c>.</value>
	[Export("AutoCompleteDefaultWords")]
	public bool AutoCompleteDefaultWords
	{
		get { return _autoCompleteDefaultWords; }
		set
		{
			WillChangeValue("AutoCompleteDefaultWords");
			_autoCompleteDefaultWords = value;
			DidChangeValue("AutoCompleteDefaultWords");
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="SourceTextView"/> 
	/// uses the default words (provided by OS X) only if keywords empty.
	/// </summary>
	/// <value><c>true</c> if use the default words only if keywords empty; otherwise, <c>false</c>.</value>
	[Export("DefaultWordsOnlyIfKeywordsEmpty")]
	public bool DefaultWordsOnlyIfKeywordsEmpty
	{
		get { return _defaultWordsOnlyIfKeywordsEmpty; }
		set
		{
			WillChangeValue("DefaultWordsOnlyIfKeywordsEmpty");
			_defaultWordsOnlyIfKeywordsEmpty = value;
			DidChangeValue("DefaultWordsOnlyIfKeywordsEmpty");
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="SourceTextView"/> complete closures.
	/// </summary>
	/// <value><c>true</c> if complete closures; otherwise, <c>false</c>.</value>
	[Export("CompleteClosures")]
	public bool CompleteClosures
	{
		get { return _completeClosures; }
		set
		{
			WillChangeValue("CompleteClosures");
			_completeClosures = value;
			DidChangeValue("CompleteClosures");
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="SourceTextView"/> wrap closures.
	/// </summary>
	/// <value><c>true</c> if wrap closures; otherwise, <c>false</c>.</value>
	[Export("WrapClosures")]
	public bool WrapClosures
	{
		get { return _wrapClosures; }
		set
		{
			WillChangeValue("WrapClosures");
			_wrapClosures = true;
			DidChangeValue("WrapClosures");
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="SourceTextView"/> selects
	/// the text that has just been wrapped in a closure.
	/// </summary>
	/// <value><c>true</c> if select after wrap; otherwise, <c>false</c>.</value>
	[Export("SelectAfterWrap")]
	public bool SelectAfterWrap
	{
		get { return _selectAfterWrap; }
		set
		{
			WillChangeValue("SelectAfterWrap");
			_selectAfterWrap = value;
			DidChangeValue("SelectAfterWrap");
		}
	}

	#endregion

	#region Constructors

	/// <summary>Initializes a new instance of the <see cref="SourceTextView"/> class.</summary>
	public SourceTextView()
	{
		// Init
		Initialize();
	}

	/// <summary>Initializes a new instance of the <see cref="SourceTextView"/> class.</summary>
	/// <param name="frameRect">Frame rect.</param>
	public SourceTextView(CGRect frameRect) : base(frameRect)
	{
		// Init
		Initialize();
	}

	/// <summary>Initializes a new instance of the <see cref="SourceTextView"/> class.</summary>
	/// <param name="frameRect">Frame rect.</param>
	/// <param name="container">Container.</param>
	public SourceTextView(CGRect frameRect, NSTextContainer container) : base(frameRect, container)
	{
		// Init
		Initialize();
	}

	/// <summary>Initializes a new instance of the <see cref="SourceTextView"/> class.</summary>
	/// <param name="coder">Coder.</param>
	public SourceTextView(NSCoder coder) : base(coder)
	{
		// Init
		Initialize();
	}

	/// <summary>Initializes a new instance of the <see cref="SourceTextView"/> class.</summary>
	/// <param name="handle">Handle.</param>
	public SourceTextView(IntPtr handle) : base(handle)
	{
		Initialize();
	}

	public SourceTextView(ObjCRuntime.NativeHandle handle) : base(handle)
	{
		Initialize();
	}

	/// <summary>Initialize this instance.</summary>
	private void Initialize()
	{
		this.Delegate = new SourceTextViewDelegate(this);
		this.UsesAdaptiveColorMappingForDarkAppearance = true;
	}

	public override void AwakeFromNib()
	{
		base.AwakeFromNib();

		this.LineNumberRuler = new LineNumberRuler(this);

		this.EnclosingScrollView.VerticalRulerView = this.LineNumberRuler;
		this.EnclosingScrollView.HasVerticalRuler = true;
		this.EnclosingScrollView.RulersVisible = true;

		this.PostsFrameChangedNotifications = true;
		NSView.Notifications.ObserveBoundsChanged((_, _) => this.DrawGutter());
		this.OnTextChanged += (_, _) => this.DrawGutter();
	}

	[Export("drawGutter")]
	public void DrawGutter()
	{
		if (this.LineNumberRuler is not null)
			this.LineNumberRuler.NeedsDisplay = true;
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Calculates the indent level by counting the number of tab characters
	/// at the start of the current line.
	/// </summary>
	/// <returns>The indent level as the number of tabs.</returns>
	/// <param name="line">The line of text being processed.</param>
	private static int CalculateIndentLevel(string line)
	{
		int indent = 0;

		// Process all characters in the line
		for (int n = 0; n < line.Length; ++n)
		{
			var code = (int)line[n];

			// Are we on a tab character?
			if (code == TabKey)
			{
				++indent;
			}
			else
			{
				break;
			}
		}

		// Return result
		return indent;
	}

	/// <summary>
	/// Creates a string of n number of tab characters that will be used to keep
	/// the tab level of the current line of text.
	/// </summary>
	/// <returns>A string of n tab characters.</returns>
	/// <param name="indentLevel">The number of tab characters to insert in the string.</param>
	private static string TabIndent(int indentLevel)
	{
		string indent = "";

		// Assemble string
		for (int n = 0; n < indentLevel; ++n)
		{
			indent += (char)TabKey;
		}

		// Return indention
		return indent;
	}

	/// <summary>
	/// Increases the tab indent on the given section of text.
	/// </summary>
	/// <returns>The text with the tab indent increased by one.</returns>
	/// <param name="text">The text to indent.</param>
	private string IncreaseTabIndent(string text)
	{
		string output = "";

		// Add first intent
		output += (char)TabKey;
		for (int n = 0; n < text.Length; ++n)
		{
			var c = text[n];
			bool found = c == Formatter.Newline
				|| c == Formatter.LineSeparator
				|| c == Formatter.ParagraphSeparator;

			// Include char in output
			output += c;

			// Increase tab level?
			if (found)
			{
				// Yes
				output += (char)TabKey;
			}
		}

		// Return results
		return output;
	}

	/// <summary>Decreases the tab indent for the given text</summary>
	/// <returns>The text with the tab indent decreased by one.</returns>
	/// <param name="text">The text to outdent.</param>
	private string DecreaseTabIndent(string text)
	{
		string output = "";
		bool consume = true;

		// Add first intent
		for (int n = 0; n < text.Length; ++n)
		{
			var c = text[n];
			bool found = (c == Formatter.Newline || c == Formatter.LineSeparator || c == Formatter.ParagraphSeparator);

			// Include char in output?
			if ((int)c == TabKey && consume)
			{
				consume = false;
			}
			else
			{
				output += c;
			}

			// Decrease tab level?
			if (found)
			{
				// Yes
				consume = true;
			}
		}

		// Return results
		return output;
	}

	#endregion

	#region Public Methods

	/// <summary>Indents the currently selected text.</summary>
	public void IndentText()
	{

		// Grab range
		var range = Formatter.FindLineBoundries(TextStorage.Value, SelectedRange);
		var line = TextStorage.Value.Substring((int)range.Location, (int)range.Length);

		// Increase tab indent
		var output = IncreaseTabIndent(line);

		// Reformat section
		TextStorage.BeginEditing();
		Replace(range, output);
		TextStorage.EndEditing();
		SelectedRange = new NSRange(range.Location, output.Length);
		Formatter.HighlightSyntaxRegion(TextStorage.Value, SelectedRange);
	}

	/// <summary>Outdents the currently selected text.</summary>
	public void OutdentText()
	{
		// Grab range
		NSRange range = Formatter.FindLineBoundries(TextStorage.Value, SelectedRange);
		string line = TextStorage.Value.Substring((int)range.Location, (int)range.Length);

		// Decrease tab indent
		string output = DecreaseTabIndent(line);

		// reformat section
		TextStorage.BeginEditing();
		Replace(range, output);
		TextStorage.EndEditing();
		SelectedRange = new NSRange(range.Location, output.Length);
		Formatter.HighlightSyntaxRegion(TextStorage.Value, SelectedRange);
	}

	/// <summary>Performs the formatting command on the currectly selected range of text.</summary>
	/// <param name="command">
	/// The <see cref="AppKit.TextKit.Formatter.LanguageFormatCommand"/> to apply.
	/// </param>
	public void PerformFormattingCommand(LanguageFormatCommand command)
	{
		NSRange range = SelectedRange;

		// Apply to start of line?
		if (command.Postfix == "")
		{
			// Yes, find start
			range = Formatter.FindLineBoundries(TextStorage.Value, SelectedRange);
		}

		// Yes, get selected text
		string line = TextStorage.Value.Substring((int)range.Location, (int)range.Length);

		// Apply command
		string output = command.Prefix;
		output += line;
		output += command.Postfix;
		TextStorage.BeginEditing();
		Replace(range, output);
		TextStorage.EndEditing();
		Formatter.HighlightSyntaxRegion(TextStorage.Value, range);
	}

	#endregion

	#region Override Methods

	private Timer InputTimoutTimer { get; set; }
	/// <summary>
	/// The amount of time with no user input after which <see cref="OnFinishedTyping"/> will be run
	/// </summary>
	public TimeSpan InputTimeoutInterval { get; set; } = new TimeSpan(0, 0, 1);
	/// <summary>
	/// An event triggered when the user has stopped typing for a period of time defined by
	/// <see cref="InputTimeoutInterval"/>
	/// </summary>
	public event ElapsedEventHandler OnFinishedTyping;

	/// <summary>Triggered when the value in the textbox is changed</summary>
	public event EventHandler OnTextChanged;

	/// <summary>
	/// Look for special keys being pressed and does specific processing based on the key.
	/// </summary>
	/// <param name="theEvent">The event.</param>
	public override void KeyDown(NSEvent theEvent)
	{
		NSRange range;
		string line;
		int indentLevel = 0;
		bool consumeKeystroke = false;

		// Avoid processing if no Formatter has been attached
		if (Formatter == null)
			return;

		// Trap all errors
		try
		{
			// Get the code of current character
			char c = theEvent.Characters[0];
			int charCode = (int)theEvent.Characters[0];

			// Preprocess based on character code
			switch (charCode)
			{
				case EnterKey:
					// Get the tab indent level
					range = Formatter.FindLineBoundries(TextStorage.Value, SelectedRange);
					line = TextStorage.Value.Substring((int)range.Location, (int)range.Length);
					indentLevel = CalculateIndentLevel(line);
					break;
				case TabKey:
					// Is a range selected?
					if (SelectedRange.Length > 0)
					{
						// Increase tab indent over the entire selection
						IndentText();
						consumeKeystroke = true;
					}
					break;
				case ShiftTabKey:
					// Is a range selected?
					if (SelectedRange.Length > 0)
					{
						// Increase tab indent over the entire selection
						OutdentText();
						consumeKeystroke = true;
					}
					break;
				default:
					// Are we completing closures
					if (CompleteClosures)
					{
						if (WrapClosures && SelectedRange.Length > 0)
						{
							// Yes, see if we are starting a closure
							foreach (LanguageClosure closure in Formatter.Language.Closures)
							{
								// Found?
								if (closure.StartingCharacter == c)
								{
									// Yes, get selected text
									nint location = SelectedRange.Location;
									line = TextStorage.Value.Substring((int)SelectedRange.Location, (int)SelectedRange.Length);
									string output = "";
									output += closure.StartingCharacter;
									output += line;
									output += closure.EndingCharacter;
									TextStorage.BeginEditing();
									Replace(SelectedRange, output);
									TextStorage.EndEditing();
									if (SelectAfterWrap)
									{
										SelectedRange = new NSRange(location, output.Length);
									}
									consumeKeystroke = true;
									Formatter.HighlightSyntaxRegion(TextStorage.Value, SelectedRange);
								}
							}
						}
						else
						{
							// Yes, see if we are in a language defined closure
							foreach (LanguageClosure closure in Formatter.Language.Closures)
							{
								// Found?
								if (closure.StartingCharacter == c)
								{
									// Is this a valid location for a completion?
									if (Formatter.TrailingCharacterIsWhitespaceOrTerminator(TextStorage.Value, SelectedRange))
									{
										// Yes, complete closure
										consumeKeystroke = true;
										string output = "";
										output += closure.StartingCharacter;
										output += closure.EndingCharacter;
										TextStorage.BeginEditing();
										InsertText(new NSString(output));
										TextStorage.EndEditing();
										SelectedRange = new NSRange(SelectedRange.Location - 1, 0);
									}
								}
							}
						}
					}
					break;
			}

			// Call base to handle event
			if (!consumeKeystroke)
				base.KeyDown(theEvent);

			// Post process based on character code
			switch (charCode)
			{
				case EnterKey:
					// Tab indent the new line to the same level
					if (indentLevel > 0)
					{
						string indent = TabIndent(indentLevel);
						TextStorage.BeginEditing();
						InsertText(new NSString(indent));
						TextStorage.EndEditing();
					}
					break;
			}
		}
		catch
		{
			// Call base to process on any error
			base.KeyDown(theEvent);
		}

		this.Formatter.Reformat();
		this.OnTextChanged.Invoke(this, null);

		this.InputTimoutTimer?.Stop();
		this.InputTimoutTimer?.Close();
		this.InputTimoutTimer = new(this.InputTimeoutInterval)
		{
			AutoReset = false
		};
		this.InputTimoutTimer.Elapsed += this.OnFinishedTyping;
		this.InputTimoutTimer.Start();
	}

	/// <summary>
	/// Called when a drag operation is started for this <see cref="SourceTextView"/>.
	/// </summary>
	/// <returns>The entered.</returns>
	/// <param name="sender">Sender.</param>
	/// <remarks>
	/// See Apple's drag and drop docs for more details (https://developer.apple.com/library/mac/documentation/Cocoa/Conceptual/DragandDrop/DragandDrop.html)
	/// </remarks>
	//public override NSDragOperation DraggingEntered(NSDraggingInfo sender)
	//{
	//	// When we start dragging, inform the system that we will be handling this as
	//	// a copy/paste
	//	return NSDragOperation.Copy;
	//}

	/// <summary>
	/// Process any drag operations initialized by the user to this <see cref="SourceTextView"/>.
	/// If one or more files have dragged in, the contents of those files will be copied into the document at the 
	/// current cursor location.
	/// </summary>
	/// <returns><c>true</c>, if drag operation was performed, <c>false</c> otherwise.</returns>
	/// <param name="sender">The caller that initiated the drag operation.</param>
	/// <remarks>
	/// See Apple's drag and drop docs for more details (https://developer.apple.com/library/mac/documentation/Cocoa/Conceptual/DragandDrop/DragandDrop.html)
	/// </remarks>
	//public override bool PerformDragOperation(NSDraggingInfo sender)
	//{
	//	// Attempt to read filenames from pasteboard
	//	var plist = (NSArray)sender.DraggingPasteboard.GetPropertyListForType(NSPasteboard.NSFilenamesType);

	//	// Was a list of files returned from Finder?
	//	if (plist != null)
	//	{
	//		// Yes, process list
	//		for (nuint n = 0; n < plist.Count; ++n)
	//		{
	//			// Get the current file
	//			var path = plist.GetItem<NSString>(n);
	//			var url = NSUrl.FromString(path);
	//			var contents = File.ReadAllText(path);

	//			// Insert contents at cursor
	//			NSRange range = SelectedRange;
	//			TextStorage.BeginEditing();
	//			Replace(range, contents);
	//			TextStorage.EndEditing();

	//			// Expand range to fully encompass new content and 
	//			// reformat
	//			range = new NSRange(range.Location, contents.Length);
	//			range = Formatter.FindLineBoundries(TextStorage.Value, range);
	//			Formatter.HighlightSyntaxRegion(TextStorage.Value, range);
	//		}

	//		// Inform caller of success
	//		return true;
	//	}
	//	else
	//	{
	//		// No, allow base class to handle
	//		return base.PerformDragOperation(sender);
	//	}
	//}

	/// <summary>Reads the selection from pasteboard.</summary>
	/// <returns>
	/// <c>true</c>, if the selection was read from the pasteboard, <c>false</c> otherwise.
	/// </returns>
	/// <param name="pboard">The pasteboard being read.</param>
	/// <remarks>
	/// This method is overridden to update the formatting after the user pastes text into the view.
	/// </remarks>
	public override bool ReadSelectionFromPasteboard(NSPasteboard pboard)
	{
		// Console.WriteLine ("Read selection from pasteboard");
		bool result = base.ReadSelectionFromPasteboard(pboard);
		Formatter?.Reformat();
		this.OnTextChanged?.Invoke(this, null);
		return result;
	}

	/// <summary>Reads the selection from pasteboard.</summary>
	/// <returns>
	/// <c>true</c>, if the selection was read from the pasteboard, <c>false</c> otherwise.
	/// </returns>
	/// <param name="pboard">The pasteboard being read.</param>
	/// <param name="type">The type of data being read from the pasteboard.</param>
	/// <remarks>
	/// This method is overridden to update the formatting after the user pastes text into the view.
	/// </remarks>
	public override bool ReadSelectionFromPasteboard(NSPasteboard pboard, string type)
	{
		// Console.WriteLine ("Read selection from pasteboard also");
		var result = base.ReadSelectionFromPasteboard(pboard, type);
		Formatter?.Reformat();
		this.OnTextChanged?.Invoke(this, null);
		return result;
	}

	#endregion

	#region Events

	/// <summary>Occurs when source cell clicked.</summary>
	/// <remarks>NOTE: This replaces the built-in <c>CellClicked</c> event because we
	/// are providing a custom <c>NSTextViewDelegate</c> and it is unavialable.</remarks>
	public event EventHandler<NSTextViewClickedEventArgs> SourceCellClicked;

	/// <summary>Raises the source cell clicked event.</summary>
	/// <param name="sender">The controller raising the event.</param>
	/// <param name="e">Arguments defining the event.</param>
	internal void RaiseSourceCellClicked(object sender, NSTextViewClickedEventArgs e)
	{
		this.SourceCellClicked?.Invoke(sender, e);
	}

	/// <summary>Occurs when source cell double clicked.</summary>
	/// <remarks>NOTE: This replaces the built-in <c>CellDoubleClicked</c> event because we
	/// are providing a custom <c>NSTextViewDelegate</c> and it is unavialable.</remarks>
	public event EventHandler<NSTextViewDoubleClickEventArgs> SourceCellDoubleClicked;

	/// <summary>Raises the source cell double clicked event.</summary>
	/// <param name="sender">The controller raising the event.</param>
	/// <param name="e">Arguments defining the event.</param>
	internal void RaiseSourceCellDoubleClicked(object sender, NSTextViewDoubleClickEventArgs e)
	{
		this.SourceCellDoubleClicked?.Invoke(sender, e);
	}

	/// <summary>Occurs when source cell dragged.</summary>
	/// <remarks>
	/// NOTE: This replaces the built-in <c>DragCell</c> event because we are providing a custom
	/// <c>NSTextViewDelegate</c> and it is unavialable.
	/// </remarks>
	public event EventHandler<NSTextViewDraggedCellEventArgs> SourceCellDragged;

	/// <summary>Raises the source cell dragged event.</summary>
	/// <param name="sender">The controller raising the event.</param>
	/// <param name="e">Arguments defining the event.</param>
	internal void RaiseSourceCellDragged(object sender, NSTextViewDraggedCellEventArgs e)
	{
		this.SourceCellDragged?.Invoke(sender, e);
	}

	/// <summary>Occurs when source selection changed.</summary>
	/// <remarks>
	/// NOTE: This replaces the built-in <c>DidChangeSelection</c> event because we are providing a
	/// custom <c>NSTextViewDelegate</c> and it is unavialable.
	/// </remarks>
	public event EventHandler SourceSelectionChanged;

	/// <summary>Raises the source selection changed event.</summary>
	/// <param name="sender">The controller raising the event.</param>
	/// <param name="e">Arguments defining the event.</param>
	internal void RaiseSourceSelectionChanged(object sender, EventArgs e)
	{
		this.SourceSelectionChanged?.Invoke(sender, e);
	}

	/// <summary>Occurs when source typing attributes changed.</summary>
	/// <remarks>NOTE: This replaces the built-in <c>DidChangeTypingAttributes</c> event because we
	/// are providing a custom <c>NSTextViewDelegate</c> and it is unavialable.</remarks>
	public event EventHandler SourceTypingAttributesChanged;

	/// <summary>Raises the source typing attributes changed event.</summary>
	/// <param name="sender">The controller raising the event.</param>
	/// <param name="e">Arguments defining the event.</param>
	internal void RaiseSourceTypingAttributesChanged(object sender, EventArgs e)
	{
		this.SourceTypingAttributesChanged?.Invoke(sender, e);
	}

	#endregion
}
