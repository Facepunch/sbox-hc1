using Facepunch;

public sealed class TeamSpawnAssigner : Component, ISpawnPointAssigner
{
	private readonly Dictionary<Team, int> _nextSpawnIndex = new();

	public Transform GetSpawnTransform( Team team )
	{
		var spawns = Game.ActiveScene.GetAllComponents<TeamSpawnPoint>()
			.Where( x => x.Team == team )
			.Select( x => x.Transform.World )
			.Union( Game.ActiveScene.GetAllComponents<SpawnPoint>()
				.Select( x => x.Transform.World ) )
			.ToArray();

		var spawnIndex = _nextSpawnIndex.GetValueOrDefault( team );

		_nextSpawnIndex[team] = spawnIndex + 1;

		if (spawns.Length == 0)
		{
			throw new Exception( $"No valid spawns for team {team}!" );
		}

		return spawns[spawnIndex % spawns.Length];
	}
}
