using AppKit;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using Cauldron.Macos.SourceList;
using Foundation;

namespace Cauldron.Macos
{
	public partial class DiagnosticsPopoverController : NSViewController
	{
		public DiagnosticSeverity Severity { get; set; }

		public DiagnosticsPopoverController (ObjCRuntime.NativeHandle handle) : base (handle) { }

		public override void ViewWillAppear()
		{
			base.ViewWillAppear();

			// Build the list of diagnostics info

			MainWindow window = NSApplication.SharedApplication.KeyWindow.WindowController
				as MainWindow;

			ImmutableArray<Diagnostic> diagnostics = window.Diagnostics;

			this.DiagnosticsOutlineView.Initialize();

			SourceListItem errors = new("Errors")
			{
				IsHeader = true
			};
			SourceListItem warnings = new("Warnings")
			{
				IsHeader = true
			};
			SourceListItem infos = new("Information")
			{
				IsHeader = true
			};

			foreach (var diagnostic in diagnostics)
			{
				SourceListItem item = new($"{diagnostic.Id} {diagnostic.GetMessage()}\n{diagnostic.Location}", "",
					() =>
					{
						window.ScriptEditorTextBox.SetSelectedRange(
							new NSRange(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.End));
						this.DismissController(this);
					});

				if (diagnostic.Severity == DiagnosticSeverity.Error)
					errors.AddItem(item);
				else if (diagnostic.Severity == DiagnosticSeverity.Warning)
					warnings.AddItem(item);
				else if (diagnostic.Severity == DiagnosticSeverity.Info)
					infos.AddItem(item);
			}

			this.DiagnosticsOutlineView.AddItem(errors);
			this.DiagnosticsOutlineView.AddItem(warnings);
			this.DiagnosticsOutlineView.AddItem(infos);

			this.DiagnosticsOutlineView.ReloadData();
			this.DiagnosticsOutlineView.ExpandItem(null, true);
			this.DiagnosticsOutlineView.UsesAutomaticRowHeights = true;
		}
	}
}
