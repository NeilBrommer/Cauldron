using AppKit;
using Foundation;

namespace Papercut.Macos;

[Register("AppDelegate")]
public partial class AppDelegate : NSApplicationDelegate
{
	public AppDelegate()
	{
	}

	public override void DidFinishLaunching(NSNotification notification)
	{
		// Insert code here to initialize your application
	}

	public override void WillTerminate(NSNotification notification)
	{
		// Insert code here to tear down your application
	}

	partial void RunScriptMenuItemClicked(AppKit.NSMenuItem sender)
	{
		ScriptRunner.RunScript(
			NSApplication.SharedApplication.KeyWindow.WindowController
				as MainWindow);
	}

	[Action("validateMenuItem:")]
	public bool ValidateMenuItem(AppKit.NSMenuItem sender)
	{
		return true;
	}
}
