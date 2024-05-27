using Facepunch;

public sealed class TeamSpawnAssigner : Component, ISpawnPointAssigner
{
	private readonly Dictionary<Team, int> _nextSpawnIndex = new();

	public Transform GetSpawnTransform( Team team )
	{
		var spawns = Game.ActiveScene.GetAllComponents<SpawnPoint>()
			.Where( x => x.Team == team )
			.ToArray();

		var spawnIndex = _nextSpawnIndex.GetValueOrDefault( team );

		_nextSpawnIndex[team] = spawnIndex + 1;

		if (spawns.Length == 0)
		{
			throw new Exception( $"No valid spawns for team {team}!" );
		}

		return spawns[spawnIndex % spawns.Length].Transform.World;
	}
}
