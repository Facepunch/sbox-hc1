using Facepunch;

/// <summary>
/// Place players at spawn points matching their teams.
/// </summary>
public sealed class TeamSpawnAssigner : Component, ISpawnPointAssigner
{
	private readonly Dictionary<Team, int> _nextSpawnIndex = new();

	public Transform GetSpawnTransform( Team team )
	{
		var spawns = new List<Transform>();

		spawns.AddRange( Game.ActiveScene.GetAllComponents<TeamSpawnPoint>()
			.Where( x => x.Team == team )
			.Select( x => x.Transform.World ) );

		if ( team == Team.Unassigned )
		{
			spawns.AddRange( Game.ActiveScene.GetAllComponents<SpawnPoint>()
				.Select( x => x.Transform.World ) );
		}

		var spawnIndex = _nextSpawnIndex.GetValueOrDefault( team );

		_nextSpawnIndex[team] = spawnIndex + 1;

		if ( spawns.Count == 0 )
		{
			throw new Exception( $"No valid spawns for team {team}!" );
		}

		return spawns[spawnIndex % spawns.Count];
	}
}
