namespace Facepunch;

public partial class PlayerController
{
	/// <summary>
	/// What effect should we spawn when a player gets headshot?
	/// </summary>
	[Property, Group( "Effects" )] public GameObject HeadshotEffect { get; set; }

	/// <summary>
	/// What effect should we spawn when a player gets headshot while wearing a helmet?
	/// </summary>
	[Property, Group( "Effects" )] public GameObject HeadshotWithHelmetEffect { get; set; }

	/// <summary>
	/// What effect should we spawn when we hit a player?
	/// </summary>
	[Property, Group( "Effects" )] public GameObject BloodEffect { get; set; }

	/// <summary>
	/// What sound should we play when a player gets headshot?
	/// </summary>
	[Property, Group( "Effects" )] public SoundEvent HeadshotSound { get; set; }

	/// <summary>
	/// What sound should we play when a player gets headshot?
	/// </summary>
	[Property, Group( "Effects" )] public SoundEvent HeadshotWithHelmetSound { get; set; }

	/// <summary>
	/// What sound should we play when we hit a player?
	/// </summary>
	[Property, Group( "Effects" )] public SoundEvent BloodImpactSound { get; set; }

	private bool IsOutlineVisible()
	{
		var localPlayer = GameUtils.Viewer;
		if ( !localPlayer.IsValid() )
			return false;

		if ( localPlayer == this )
			return false;

		if ( TeamComponent.Team == Team.Unassigned )
			return false;

		if ( HealthComponent.IsGodMode )
			return true;

		return HealthComponent.State == LifeState.Alive && TeamComponent.Team == localPlayer.TeamComponent.Team;
	}

	private void UpdateOutline()
	{
		if ( !IsOutlineVisible() )
		{
			Outline.Enabled = false;
			return;
		}

		Outline.Enabled = true;
		Outline.Width = 0.2f;
		Outline.Color = Color.Transparent;
		Outline.InsideColor = HealthComponent.IsGodMode ? Color.White.WithAlpha( 0.1f ) : Color.Transparent;
		Outline.ObscuredColor = GameUtils.Viewer is { TeamComponent.Team: var localTeam } && localTeam == TeamComponent.Team
			? TeamComponent.Team.GetColor() : Color.Transparent;
	}
}
