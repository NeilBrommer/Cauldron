// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Cauldron.Macos
{
	[Register ("DiagnosticsPopoverController")]
	partial class DiagnosticsPopoverController
	{
		[Outlet]
		Cauldron.Macos.SourceList.SourceListView DiagnosticsOutlineView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (DiagnosticsOutlineView != null) {
				DiagnosticsOutlineView.Dispose ();
				DiagnosticsOutlineView = null;
			}
		}
	}
}
