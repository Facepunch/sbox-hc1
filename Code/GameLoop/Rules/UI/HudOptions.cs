namespace Facepunch;

public partial class HudOptions : Component
{
	/// <summary>
	/// How many players can we display on the round display until we substitute them for a general head-count?
	/// </summary>
	[Property] public int FullAvatarDisplayPlayerLimit { get; set; } = 10;
}
