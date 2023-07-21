using AppKit;

namespace Cauldron.Macos;

static class MainClass
{
	static void Main(string[] args)
	{
		NSApplication.Init();
		NSApplication.Main(args);
	}
}
