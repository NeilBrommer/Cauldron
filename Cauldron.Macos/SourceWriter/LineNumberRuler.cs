using System;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Cauldron.Macos.SourceWriter;

public class LineNumberRuler : NSRulerView
{
	private NSColor _foregroundColor = NSColor.DisabledControlText;
	private NSColor _backgroundColor = NSColor.TextBackground;

	public float GutterWidth { get; private set; } = 40f;
	public NSColor ForegroundColor
	{
		get => this._foregroundColor;
		set
		{
			this._foregroundColor = value;
			this.NeedsDisplay = true;
		}
	}
	public NSColor BackgroundColor
	{
		get => this._backgroundColor;
		set
		{
			this._backgroundColor = value;
			this.NeedsDisplay = true;
		}
	}

	public LineNumberRuler(NSTextView textView)
		: base(textView.EnclosingScrollView, NSRulerOrientation.Vertical)
	{
		this.ClientView = textView;
		this.RuleThickness = this.GutterWidth;
	}

	public LineNumberRuler(NSTextView textView, NSColor foregroundColor, NSColor backgroundColor)
		: base(textView.EnclosingScrollView, NSRulerOrientation.Vertical)
	{
		this.ClientView = textView;
		this.ForegroundColor = foregroundColor;
		this.BackgroundColor = backgroundColor;
		this.RuleThickness = this.GutterWidth;
	}

	public override void DrawHashMarksAndLabels(CGRect rect)
	{
		this.BackgroundColor.Set();
		NSGraphicsContext.CurrentContext.CGContext.FillRect(rect);

		if (this.ClientView is not NSTextView textView)
			return;

		NSLayoutManager layoutManager = textView.LayoutManager;
		NSTextContainer textContainer = textView.TextContainer;

		if (layoutManager is null || textContainer is null)
			return;

		NSString content = new NSString(textView.Value);
		NSRange visibleGlyphsRange = layoutManager
			.GetGlyphRangeForBoundingRect(textView.VisibleRect(), textContainer);

		int lineNumber = 1;

		NSRegularExpression newlineRegex = new NSRegularExpression(new NSString("\n"),
			new NSRegularExpressionOptions(), out NSError error);
		if (error is not null)
			return;

		lineNumber += (int)newlineRegex.GetNumberOfMatches(content, new NSMatchingOptions(),
			new NSRange(0, visibleGlyphsRange.Location));

		nint firstGlyphOfLineIndex = visibleGlyphsRange.Location;

		while (firstGlyphOfLineIndex < visibleGlyphsRange.Location + visibleGlyphsRange.Length)
		{
			NSRange charRangeOfLine = content.LineRangeForRange(
				new NSRange((nint)layoutManager.GetCharacterIndex((nuint)firstGlyphOfLineIndex), 0));
			NSRange glyphRangeOfLine = layoutManager.GetGlyphRange(charRangeOfLine);

			nint firstGlyphOfRowIndex = firstGlyphOfLineIndex;
			int lineWrapCount = 0;

			while (firstGlyphOfRowIndex < glyphRangeOfLine.Location + glyphRangeOfLine.Length)
			{
				CGRect lineRect = layoutManager.GetLineFragmentRect((nuint)firstGlyphOfRowIndex,
					out NSRange effectiveRange, true);

				if (lineWrapCount == 0)
					this.DrawLineNumber(lineNumber, (float)lineRect.GetMinY()
						+ (float)textView.TextContainerInset.Height);
				else
					break;

				// Move to next row
				firstGlyphOfRowIndex = effectiveRange.Location + effectiveRange.Length;
			}

			firstGlyphOfLineIndex = glyphRangeOfLine.Location + glyphRangeOfLine.Length;
			lineNumber += 1;
		}

		if (layoutManager.ExtraLineFragmentTextContainer != null)
			this.DrawLineNumber(lineNumber, (float)layoutManager.ExtraLineFragmentRect.GetMinY()
				+ (float)textView.TextContainerInset.Height);
	}

	private void DrawLineNumber(int lineNumber, float yPosition)
	{
		if (this.ClientView is not NSTextView textView)
			return;

		NSDictionary attributes = new(NSStringAttributeKey.Font, textView.Font,
			NSStringAttributeKey.ForegroundColor, this.ForegroundColor);
		NSAttributedString attributedLineNumber = new(lineNumber.ToString(), attributes);
		CGPoint relativePoint = this.ConvertPointFromView(CGPoint.Empty, textView);
		nfloat xPosition = this.GutterWidth - (attributedLineNumber.Size.Width + 8);

		attributedLineNumber.DrawAtPoint(new CGPoint(xPosition, relativePoint.Y + yPosition));
	}
}

