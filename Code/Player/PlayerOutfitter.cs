namespace Facepunch;

/// <summary>
/// An extremely basic outfitter, which'll swap between two gameobjects (which are holding an outfit)
/// </summary>
public partial class PlayerOutfitter : Component
{
	[Property] public GameObject TerroristClothes { get; set; }
	[Property] public GameObject CounterTerroristClothes { get; set; }

	public void OnResetState( PlayerController player )
	{
		UpdateFromTeam( player.TeamComponent.Team );
	}

	private void UpdateFromTeam( Team team )
	{
		bool isT = team == Team.Terrorist;
		TerroristClothes.Enabled = isT;
		CounterTerroristClothes.Enabled = !isT;
	}
}
