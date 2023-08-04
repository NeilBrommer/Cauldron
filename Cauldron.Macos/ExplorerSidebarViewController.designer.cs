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
	[Register ("ExplorerSidebarViewController")]
	partial class ExplorerSidebarViewController
	{
		[Outlet]
		AppKit.NSTabView SidebarTabView { get; set; }

		[Action ("SidebarTabClicked:")]
		partial void SidebarTabClicked (AppKit.NSSegmentedControl sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (SidebarTabView != null) {
				SidebarTabView.Dispose ();
				SidebarTabView = null;
			}
		}
	}
}
