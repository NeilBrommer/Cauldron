using Foundation;

namespace Cauldron.Macos.SourceWriter
{
	public class LanguageClosure : NSObject
	{
		#region Computed Properties

		/// <summary>Gets or sets the starting character for this closure.</summary>
		/// <value>The starting character.</value>
		public char StartingCharacter { get; set; }

		/// <summary>Gets or sets the ending character for this closure.</summary>
		/// <value>The ending character.</value>
		public char EndingCharacter { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="LanguageClosure"/> class.
		/// </summary>
		public LanguageClosure() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="LanguageClosure"/> class.
		/// </summary>
		/// <param name="character">The character that both starts and ends this closure.</param>
		public LanguageClosure(char character)
		{
			this.StartingCharacter = character;
			this.EndingCharacter = character;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LanguageClosure"/> class.
		/// </summary>
		/// <param name="startingCharacter">The character that starts the closure.</param>
		/// <param name="endingCharacter">The character that ends the closure.</param>
		public LanguageClosure(char startingCharacter, char endingCharacter)
		{
			this.StartingCharacter = startingCharacter;
			this.EndingCharacter = endingCharacter;
		}

		#endregion
	}
}
