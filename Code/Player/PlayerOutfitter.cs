namespace Facepunch;

/// <summary>
/// An extremely basic outfitter, which'll swap between two gameobjects (which are holding an outfit)
/// </summary>
public partial class PlayerOutfitter : Component
{
	[Property] public GameObject TerroristClothes { get; set; }
	[Property] public GameObject CounterTerroristClothes { get; set; }
	[Property] public TeamComponent TeamComponent { get; set; }
	[Property] public PlayerController PlayerController { get; set; }

	public void OnResetState( PlayerController player )
	{
		UpdateFromTeam( player.TeamComponent.Team );
	}

	private void OnTeamChanged( Team oldTeam, Team newTeam )
	{
		if ( PlayerController.IsSpectating )
			return;
		
		UpdateFromTeam( newTeam );
	}

	protected override void OnEnabled()
	{
		TeamComponent.OnTeamChanged += OnTeamChanged;
		base.OnEnabled();
	}

	protected override void OnDisabled()
	{
		TeamComponent.OnTeamChanged -= OnTeamChanged;
		base.OnDisabled();
	}

	private void UpdateFromTeam( Team team )
	{
		var isT = team == Team.Terrorist;
		TerroristClothes.Enabled = isT;
		CounterTerroristClothes.Enabled = !isT;

		PlayerController.Body.ReapplyVisibility();
	}
}
