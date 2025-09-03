using Facepunch.UI;
using Sandbox.Events;

namespace Facepunch;

public record CapturePointCapturedEvent : IGameEvent
{
	public CapturePoint CapturePoint { get; set; }
	public Team Team { get; set; }
	public Team PreviousTeam { get; set; }
}

public record ResetCapturePointsEvent() : IGameEvent;

public partial class CapturePoint : Component, IMarkerObject, IMinimapLabel, IMinimapVolume, Component.ITriggerListener,
	IGameEventHandler<ResetCapturePointsEvent>
{
	[Property, Group( "Capture Point" )] public string FullName { get; set; } = "Alpha";
	[Property, Group( "Capture Point" )] public string ShortName { get; set; } = "A";

	/// <summary>
	/// Where is the marker?
	/// </summary>
	Vector3 IMarkerObject.MarkerPosition => WorldPosition + Vector3.Up * 128f;

	/// <summary>
	/// What icon?
	/// </summary>
	string IMarkerObject.MarkerIcon => "/ui/minimaps/cashgrab/cashpoint.png";

	/// <summary>
	/// What text?
	/// </summary>
	string IMarkerObject.DisplayText => $"{ShortName}, {Team}";

	public Color Color => Team.GetColor().WithAlpha( 0.1f );
	public Color LineColor => Team.GetColor().WithAlpha( 0.5f );

	public Vector3 Size => GetComponent<BoxCollider>().Scale;

	string IMinimapLabel.Label => ShortName;
	Color IMinimapLabel.LabelColor => Color.White;
	Angles IMinimapVolume.Angles => GameObject.WorldRotation.Angles();

	Vector3 IMinimapElement.WorldPosition => WorldPosition;

	/// <summary>
	/// Defines a custom marker panel type to instantiate. Might remove this later.
	/// </summary>
	Type IMarkerObject.MarkerPanelTypeOverride => typeof( UI.CapturePointMarker );

	bool IMarkerObject.ShowChevron => true;

	bool IMinimapElement.IsVisible( Pawn viewer ) => true;

	protected static int ArraySize => Enum.GetNames( typeof( Team ) ).Length;

	[Property, Group( "Capture Point" )] public float CaptureTime { get; set; } = 10f;

	[HostSync, Group( "Capture Point" )] public Team Team { get; set; } = Team.Unassigned;
	[HostSync, Group( "Capture Point" )] public Team HighestTeam { get; set; } = Team.Unassigned;
	[HostSync, Group( "Capture Point" )] public NetList<int> OccupantCounts { get; set; } = new();
	[HostSync, Group( "Capture Point" )] public float Captured { get; set; } = 0;
	[HostSync, Group( "Capture Point" ), Change( "OnStateChanged" )] public CaptureState CurrentState { get; set; }

	public Dictionary<Team, HashSet<PlayerPawn>> Occupants { get; protected set; } = new();

	public TimeSince TimeSinceStateChanged { get; protected set; } = 0;

	protected void OnStateChanged( CaptureState then, CaptureState now )
	{
		TimeSinceStateChanged = 0;
	}

	public enum CaptureState
	{
		None,
		Contested,
		Capturing
	}

	public void ResetState()
	{
		if ( Networking.IsHost )
		{
			Team = Team.Unassigned;
			HighestTeam = Team.Unassigned;
			Captured = 0;
			CurrentState = CaptureState.None;
			OccupantCounts.Clear();
			Occupants.Clear();
			TimeSinceStateChanged = 0;

			for ( int i = 0; i < ArraySize; i++ )
				OccupantCounts.Add( 0 );

			// Initialize the dictionary's list values.
			foreach ( Team team in Enum.GetValues( typeof( Team ) ) )
			{
				if ( team == Team.Unassigned )
					continue;

				Occupants[team] = new();
			}
		}
	}

	protected override void OnStart()
	{
		ResetState();
	}

	internal void AddPlayer( PlayerPawn player )
	{
		// Already in the list!
		if ( Occupants[player.Team].Contains( player ) )
			return;

		Occupants[player.Team].Add( player );
		OccupantCounts[(int)player.Team]++;
	}

	internal void RemovePlayer( PlayerPawn player )
	{
		if ( !Occupants.ContainsKey( player.Team ) )
			return;

		if ( !Occupants[player.Team].Contains( player ) )
			return;

		Occupants[player.Team].Remove( player );
		OccupantCounts[(int)player.Team]--;
	}

	public int GetCount( Team team )
	{
		return OccupantCounts[(int)team];
	}

	void ITriggerListener.OnTriggerEnter( Sandbox.Collider other )
	{
		if ( Networking.IsHost && other.GameObject.Root.GetComponentInChildren<PlayerPawn>() is { } player )
		{
			AddPlayer( player );
		}
	}

	void ITriggerListener.OnTriggerExit( Sandbox.Collider other )
	{
		if ( Networking.IsHost && other.GameObject.Root.GetComponentInChildren<PlayerPawn>() is { } player )
		{
			RemovePlayer( player );
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost )
			return;

		if ( Occupants is null || OccupantCounts is null )
			return;

		if ( Occupants.Count == 0 || OccupantCounts.Count == 0 )
			return;

		var lastCount = 0;
		var highest = Team.Unassigned;
		var contested = false;
		for ( int i = 0; i < OccupantCounts.Count; i++ )
		{
			var team = (Team)i;
			var count = OccupantCounts[i];

			if ( lastCount > 0 && count > 0 )
			{
				contested = true;
				break;
			}

			if ( count > 0 )
			{
				lastCount = count;
				highest = team;
			}
		}

		HighestTeam = highest;

		// nobody is fighting for this point (which shouldn't really happen)
		if ( highest == Team.Unassigned )
		{
			CurrentState = CaptureState.None;
			return;
		}

		// Don't do anythig while we're contested
		if ( contested )
		{
			CurrentState = CaptureState.Contested;
			return;
		}
		else
		{
			CurrentState = CaptureState.None;
		}

		// A team is trying to cap. Let's reverse this shit.
		if ( Team != Team.Unassigned && highest != Team )
		{
			float attackMultiplier = MathF.Sqrt( lastCount );
			Captured = MathX.Clamp( Captured - Time.Delta * attackMultiplier / CaptureTime, 0, 1 );

			if ( Captured == 0f )
			{
				Team = Team.Unassigned;
			}
			else
			{
				CurrentState = CaptureState.Capturing;
			}
		}
		else
		{
			float attackMultiplier = MathF.Sqrt( lastCount );
			var last = Captured;

			Captured = MathX.Clamp( Captured + Time.Delta * attackMultiplier / CaptureTime, 0, 1 );

			if ( Captured == 1f )
			{
				if ( last != Captured )
				{
					Toast.Instance.Show( $"Point {FullName} was captured", ToastType.Generic, 3 );
					Scene.Dispatch( new CapturePointCapturedEvent()
					{
						CapturePoint = this,
						Team = highest,
						PreviousTeam = HighestTeam
					} );
				}

				Team = highest;
				HighestTeam = highest;
			}
			else
			{
				CurrentState = CaptureState.Capturing;
				Team = Team.Unassigned;
			}
		}
	}

	void IGameEventHandler<ResetCapturePointsEvent>.OnGameEvent( ResetCapturePointsEvent eventArgs )
	{
		ResetState();
	}
}
