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

	MinimapIconType IMinimapIcon.IconType => IsAlive ? MinimapIconType.Player : MinimapIconType.PlayerDead;
	bool IDirectionalMinimapIcon.EnableDirectional => IsAlive;
	Angles IDirectionalMinimapIcon.Direction => (Player as IPawn).IsPossessed || !IsAlive ? Angles.Zero : Player.EyeAngles;
	string ICustomMinimapIcon.CustomStyle => GetMinimapColor();

	Vector3 IMinimapElement.WorldPosition => Transform.Position;
	bool IMinimapElement.IsVisible( PlayerController viewer )
	{
		if ( Player.Tags.Has( "invis" ) )
			return false;

		if ( IsAlive && (Player as IPawn).IsPossessed )
			return false;

		// todo: or seen by enemies?
		return viewer.TeamComponent.Team == Player.TeamComponent.Team;
	}

	string GetMinimapColor()
	{
		if ( (Player as IPawn).IsPossessed )
		{
			return "background-image-tint: rgba( 0, 255, 255, 1 )";
		}

		return $"background-image-tint: {Player.TeamComponent.Team.GetColor().Hex}";
	}
}
