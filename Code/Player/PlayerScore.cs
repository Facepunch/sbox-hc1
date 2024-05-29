namespace Facepunch.UI;

public sealed class PlayerScore : Component
{
	[Sync] public int Kills { get; set; } = 0;
	[Sync] public int Deaths { get; set; } = 0;
	[Sync] public int Experience { get; set; } = 0;
}
