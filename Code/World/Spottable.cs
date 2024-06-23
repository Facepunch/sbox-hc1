using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Tracks whether something has been seen by the enemy team.
/// </summary>
public sealed class Spottable : Component,
	IGameEventHandler<BetweenRoundCleanupEvent>,
	IGameEventHandler<TeamChangedEvent>
{
	/// <summary>
	/// The team this belongs to.
	/// Any players that aren't of this team will be able to spot this.
	/// </summary>
	[Property, HostSync] public Team Team { get; set; }

	/// <summary>
	/// As the position doesn't move, it'll always be spotted after it's seen once.
	/// </summary>
	[Property] public bool Static { get; set; }

	[HostSync] public bool HasBeenSpotted { get; private set; }
	[HostSync] public TimeSince LastSpotted { get; private set; }

	public Vector3 LastSeenPosition;

	[Property] public float Height { get; set; }

	public bool IsSpotted => Static ? HasBeenSpotted : LastSpotted < 1;
	public bool WasSpotted => !IsSpotted && LastSpotted < 5;

	protected override void OnUpdate()
	{
		if ( IsSpotted )
			LastSeenPosition = Transform.Position;
	}

	public void Spotted( Spotter spotter )
	{
		LastSpotted = 0;
		HasBeenSpotted = true;
	}

	void IGameEventHandler<BetweenRoundCleanupEvent>.OnGameEvent( BetweenRoundCleanupEvent eventArgs )
	{
		LastSpotted = 999;
		HasBeenSpotted = false;
	}

	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Green;
		Gizmo.Draw.Line( Transform.Position, Transform.Position + Vector3.Up * Height );
	}

	void IGameEventHandler<TeamChangedEvent>.OnGameEvent( TeamChangedEvent eventArgs )
	{
		Team = eventArgs.After;
	}
}
