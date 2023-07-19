// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Papercut.Macos
{
	[Register ("MainWindow")]
	partial class MainWindow
	{
		[Outlet]
		AppKit.NSToolbarItem RunScriptToolbarButton { get; set; }

		[Action ("BtnRunScriptClicked:")]
		partial void BtnRunScriptClicked (AppKit.NSToolbarItem sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (RunScriptToolbarButton != null) {
				RunScriptToolbarButton.Dispose ();
				RunScriptToolbarButton = null;
			}
		}
	}
}
