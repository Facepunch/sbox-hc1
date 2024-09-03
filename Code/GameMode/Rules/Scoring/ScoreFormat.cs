namespace Facepunch;

/// <summary>
/// Lets us change the scoring format per-gamemode.
/// </summary>
public partial class ScoreFormat : Component
{
	/// <summary>
	/// The scoring format. https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
	/// </summary>
	[Property] public string Format { get; set; } = "";
	[Property] public string Prefix { get; set; } = "";

	protected override void OnStart()
	{
		if ( !GameMode.Instance.IsValid() )
			return;

		var teamScoring = GameMode.Instance.Get<TeamScoring>();
		if ( !teamScoring.IsValid() )
			return
				;
		teamScoring.ScoreFormat = Format;
		teamScoring.ScorePrefix = Prefix;
	}
}
