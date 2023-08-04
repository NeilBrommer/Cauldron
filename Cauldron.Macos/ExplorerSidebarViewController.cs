using System;
using AppKit;

namespace Cauldron.Macos;

public partial class ExplorerSidebarViewController : NSViewController
{
	public ExplorerSidebarViewController (IntPtr handle) : base (handle) { }

	public ExplorerSidebarViewController(ObjCRuntime.NativeHandle handle) : base(handle) { }

	partial void SidebarTabClicked(NSSegmentedControl sender)
	{
		// Select the corresponding tab
		this.SidebarTabView.SelectAt(sender.SelectedSegment);
	}
}
