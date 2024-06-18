namespace Facepunch;

/// <summary>
/// A component that handles the state of the player's marker on the minimap and the HUD.
/// </summary>
public partial class PlayerMarker : Component, IMarkerObject, IDirectionalMinimapIcon
{
	/// <summary>
	/// The player.
	/// </summary>
	[RequireComponent] PlayerController Player { get; set; }

	/// <summary>
	/// An accessor to see if the player is alive or not.
	/// </summary>
	private bool IsAlive => Player.HealthComponent.State == LifeState.Alive;

	/// <summary>
	/// Defines a custom marker panel type to instantiate. Might remove this later.
	/// </summary>
	Type IMarkerObject.MarkerPanelTypeOverride => typeof( UI.PlayerMarkerPanel );

	private Vector3 DistOffset
	{
		get
		{
			var dist = Scene.Camera.Transform.Position.Distance( Transform.Position );
			dist *= 0.0225f;
			return Vector3.Up * dist;
		}
	}

	/// <summary>
	/// Where is the marker?
	/// </summary>
	Vector3 IMarkerObject.MarkerPosition => Transform.Position + Vector3.Up * 70 + DistOffset;

	/// <summary>
	/// What type of icon are we using on the minimap?
	/// </summary>
	string IMinimapIcon.IconPath
	{
		get
		{
			if ( IsEnemy ) return IsMissing ? "ui/minimaps/enemy_missing.png" : "ui/minimaps/enemy_icon.png";
			if ( !IsAlive ) return "ui/minimaps/icon-map_skull.png";
			return "ui/minimaps/player_icon.png";
		}
	}


	/// <summary>
	/// Is this a directional icon?
	/// </summary>
	bool IDirectionalMinimapIcon.EnableDirectional => IsAlive;

	/// <summary>
	/// What direction should we be facing? Surely this could be a float?
	/// </summary>
	Angles IDirectionalMinimapIcon.Direction => !IsAlive ? Angles.Zero : Player.EyeAngles;

	/// <summary>
	/// Defines a custom css style for this minimap icon.
	/// </summary>
	string ICustomMinimapIcon.CustomStyle
	{
		get
		{
			if ( IsEnemy ) return "background-image-tint: rgba(255, 0, 0, 1 );";
			return $"background-image-tint: {Player.PlayerColor.Hex}";
		}
	}

	/// <summary>
	/// The minimap element's position in the world.
	/// </summary>
	Vector3 IMinimapElement.WorldPosition => IsEnemy && IsMissing ? Player.Spottable.LastSeenPosition : Transform.Position;

	/// <summary>
	/// Is this player an enemy of the viewer?
	/// </summary>
	bool IsEnemy => GameUtils.Viewer.Controller.TeamComponent.Team != Player.TeamComponent.Team;

	/// <summary>
	/// Did we spot this player recently?
	/// </summary>
	bool IsMissing => Player.Spottable.WasSpotted;

	/// <summary>
	/// Should we render this element at all?
	/// </summary>
	/// <param name="viewer"></param>
	/// <returns></returns>
	bool IMinimapElement.IsVisible( IPawn viewer )
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

		return viewer.Team == Player.TeamComponent.Team;
	}
}
