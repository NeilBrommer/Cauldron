using AppKit;

namespace Papercut.Macos;

static class MainClass
{
	static void Main(string[] args)
	{
		NSApplication.Init();
		NSApplication.Main(args);
	}
}
