using AppKit;

namespace Cauldron.Macos.SourceWriter;

public class KeywordDescriptor
{
	#region Computed Properties

	/// <summary>Gets or sets the <c>KeywordType</c>.</summary>
	/// <value>The type.</value>
	public KeywordType Type { get; set; } = KeywordType.Keyword;

	/// <summary>
	/// Gets or sets the <c>NSColor</c> that the <see cref="AppKit.TextKit.Formatter.LanguageFormatter"/>
	/// will set this keyword to.
	/// </summary>
	/// <value>The <c>NSColor</c>.</value>
	public NSColor Color { get; set; } = NSColor.Black;

	/// <summary>Gets or sets the tooltip used to define this keyword.</summary>
	/// <value>The tooltip.</value>
	public string Tooltip { get; set; } = "";

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="KeywordDescriptor"/> class.
	/// </summary>
	public KeywordDescriptor() { }

	/// <summary>
	/// Initializes a new instance of the <see cref="KeywordDescriptor"/> class.
	/// </summary>
	/// <param name="type">Specifies the <c>KeywordType</c>.</param>
	/// <param name="color">Specifies the <c>NSColor</c> that this keyword will be set to.</param>
	/// <param name="toolTip">Defines the tool tip for this keyword.</param>
	public KeywordDescriptor(KeywordType type, NSColor color, string toolTip)
	{
		this.Type = type;
		this.Color = color;
		this.Tooltip = toolTip;
	}

	#endregion
}

