using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Tracks whether something has been seen by the enemy team.
/// </summary>
public sealed class Spottable : Component,
	IRoundCleanup,
	IGameEventHandler<TeamChangedEvent>
{
	ITeam teamComponent => GetComponent<ITeam>();

	/// <summary>
	/// The team this belongs to.
	/// Any players that aren't of this team will be able to spot this.
	/// </summary>
	public Team Team
	{
		get => teamComponent?.Team ?? Team.Unassigned;
		set
		{
			if ( teamComponent.IsValid() ) teamComponent.Team = value;
		}
	}

	/// <summary>
	/// As the position doesn't move, it'll always be spotted after it's seen once.
	/// </summary>
	[Property] public bool Static { get; set; }

	[Sync( SyncFlags.FromHost )] public bool HasBeenSpotted { get; private set; }
	[Sync( SyncFlags.FromHost )] public TimeSince LastSpotted { get; private set; }

	public Vector3 LastSeenPosition;

	[Property] public float Height { get; set; }

	public bool IsSpotted => Static ? HasBeenSpotted : LastSpotted < 1;
	public bool WasSpotted => !IsSpotted && LastSpotted < 5;

	protected override void OnUpdate()
	{
		if ( IsSpotted )
			LastSeenPosition = WorldPosition;
	}

	public void Spotted( Spotter spotter )
	{
		LastSpotted = 0;
		HasBeenSpotted = true;
	}

	void IRoundCleanup.OnRoundCleanup()
	{
		LastSpotted = 999;
		HasBeenSpotted = false;
	}

	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Green;
		Gizmo.Draw.Line( WorldPosition, WorldPosition + Vector3.Up * Height );
	}

	void IGameEventHandler<TeamChangedEvent>.OnGameEvent( TeamChangedEvent eventArgs )
	{
		Team = eventArgs.After;
	}
}
