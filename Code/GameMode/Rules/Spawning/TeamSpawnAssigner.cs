using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Use team-specific spawn points.
/// </summary>
public sealed class TeamSpawnAssigner : Component,
	ISpawnAssigner
{
	/// <summary>
	/// Use spawns that include at least one of these tags.
	/// </summary>
	[Property, Title( "Tags" )]
	public List<string> SpawnTags { get; private set; } = new();

	public Transform GetSpawnPoint( PlayerController player )
	{
		var team = player.TeamComponent.Team;
		var spawns = GameUtils.GetSpawnPoints( team ).Shuffle();

		if ( spawns.Count == 0 && player.TeamComponent.Team != Team.Unassigned )
		{
			throw new Exception( $"No spawn points for team {team}!" );
		}

		var playerPositions = GameUtils.ActivePlayers
			.Where( x => x != player )
			.Where( x => x.HealthComponent.State == LifeState.Alive )
			.Select( x => x.Transform.Position )
			.ToArray();

		foreach ( var spawn in spawns )
		{
			if ( playerPositions.All( x => (x - spawn.Position).LengthSquared > 32f * 32f ) )
			{
				return spawn;
			}
		}

		return spawns[0];
	}
}
