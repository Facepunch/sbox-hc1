namespace Facepunch;

/// <summary>
/// Tracks wether something has been seen by the enemy team.
/// </summary>
public sealed class Spottable : Component, IRoundStartListener
{
	/// <summary>
	/// The team this belongs to.
	/// Any players that aren't of this team will be able to spot this.
	/// </summary>
	[Property] public Team Team;

	[HostSync] public TimeSince LastSpotted { get; private set; }

	public Vector3 LastSeenPosition;

	[Property] public float Height;

	public bool IsSpotted => LastSpotted < 1;
	public bool WasSpotted => !IsSpotted && LastSpotted < 5;

	protected override void OnEnabled()
	{
		TeamComponent teamComponent = Components.Get<TeamComponent>();
		if ( teamComponent is not null )
		{
			teamComponent.OnTeamChanged += OnTeamChanged;
		}

		base.OnEnabled();
	}

	protected override void OnDisabled()
	{
		TeamComponent teamComponent = Components.Get<TeamComponent>();
		if ( teamComponent is not null )
		{
			teamComponent.OnTeamChanged -= OnTeamChanged;
		}
		base.OnDisabled();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (IsSpotted)
			LastSeenPosition = Transform.Position;
	}

	private void OnTeamChanged( Team oldTeam, Team newTeam )
	{
		Team = newTeam;
	}

	public void Spotted(Spotter spotter)
	{
		LastSpotted = 0;
	}

	void IRoundStartListener.PreRoundStart()
	{
		LastSpotted = 999;
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		Gizmo.Draw.Color = Color.Green;
		Gizmo.Draw.Line(Transform.Position, Transform.Position + Vector3.Up * Height);
	}
}
