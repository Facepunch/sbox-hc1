namespace Facepunch;

public partial class PlayerPawn
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
		if ( !HealthComponent.IsValid() )
			return false;

		if ( IsPossessed )
			return false;

		if ( HealthComponent.State != LifeState.Alive )
			return false;

		if ( Team == Team.Unassigned )
			return false;

		if ( SpectateSystem.Instance.IsSpectating )
			return true;

		if ( HealthComponent.IsGodMode )
			return true;

		var viewer = PlayerState.Viewer;
		return !viewer.IsValid() || Team == viewer.Team;
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

		if ( SpectateSystem.Instance.IsSpectating )
			Outline.ObscuredColor = Team.GetColor();
		else
			Outline.ObscuredColor = PlayerState.Viewer.Team == Team
				? PlayerState.PlayerColor : Color.Transparent;
	}
}
