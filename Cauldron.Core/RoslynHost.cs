using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Cauldron.Core;

public class RoslynHost
{
	/// <summary>
	/// Run the provided C# script
	/// </summary>
	/// <param name="code">The script to run</param>
	/// <param name="imports">Namespace imports for the script</param>
	/// <param name="globals">Values that will be made available to the script</param>
	/// <param name="cancellationToken"></param>
	public static async Task RunScript(string code, string[] imports,
		RoslynHostGlobals globals,
		CancellationToken cancellationToken = default)
	{
		ScriptOptions options = ScriptOptions.Default
			.AddImports(imports);

		await CSharpScript.RunAsync(code, options, globals,
			cancellationToken: cancellationToken);
	}
}

public class RoslynHostGlobals
{
	public CauldronWriter Cauldron { get; set; }

	public RoslynHostGlobals(CauldronWriter writer)
	{
		this.Cauldron = writer;
	}
}
