namespace Facepunch;

public partial class PlayerMarker : Component, IMarkerObject, IDirectionalMinimapIcon
{
	/// <summary>
	/// The player.
	/// </summary>
	[RequireComponent] PlayerController Player { get; set; }
	private bool IsAlive => Player.HealthComponent.State == LifeState.Alive;

	/// <summary>
	/// Custom marker panel
	/// </summary>
	Type IMarkerObject.MarkerPanelTypeOverride => typeof( UI.PlayerMarkerPanel );

	MarkerFrame IMarkerObject.MarkerFrame
	{
		get => new MarkerFrame()
		{
			Position = Transform.Position + Vector3.Up * 80,
			Rotation = Transform.Rotation,
			DisplayText = Player.GetPlayerName()
		};
	}

	MinimapIconType IMinimapIcon.IconType => GetIconType();
	bool IDirectionalMinimapIcon.EnableDirectional => IsAlive;
	Angles IDirectionalMinimapIcon.Direction => !IsAlive ? Angles.Zero : Player.EyeAngles;
	string ICustomMinimapIcon.CustomStyle => GetMinimapColor();

	Vector3 IMinimapElement.WorldPosition => IsEnemy && IsMissing ? Player.Spottable.LastSeenPosition : Transform.Position;

	bool IsEnemy => GameUtils.Viewer is not null && GameUtils.Viewer.TeamComponent.Team != Player.TeamComponent.Team;
	bool IsMissing => Player.Spottable.WasSpotted;

	private MinimapIconType GetIconType()
	{
		if ( IsEnemy )	return IsMissing ? MinimapIconType.PlayerEnemyMissing : MinimapIconType.PlayerEnemy;
		if ( !IsAlive ) return MinimapIconType.PlayerDead;
		return MinimapIconType.Player;
	}

	bool IMinimapElement.IsVisible( PlayerController viewer )
	{
		if ( Player.Tags.Has( "invis" ) )
			return false;

		if ( IsAlive )
		{
			if ( (Player as IPawn).IsPossessed )
				return false;

			// seen by enemy team
			if ( Player.Spottable.IsSpotted || Player.Spottable.WasSpotted )
				return true;
		}

		return viewer.TeamComponent.Team == Player.TeamComponent.Team;
	}

	private string GetMinimapColor()
	{
		if ( IsEnemy ) return "background-image-tint: rgba(255, 0, 0, 1 );";

		return $"background-image-tint: {Player.TeamComponent.Team.GetColor().Hex}";
	}
}
