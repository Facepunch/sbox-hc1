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

	protected override void OnStart()
	{
		var teamScoring = GameMode.Instance.Get<TeamScoring>();
		teamScoring.ScoreFormat = Format;
	}
}
