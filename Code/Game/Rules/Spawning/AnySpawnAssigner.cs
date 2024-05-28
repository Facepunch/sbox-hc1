using Facepunch;

/// <summary>
/// Place players at any spawn point, ignoring teams.
/// </summary>
public sealed class AnySpawnAssigner : Component, ISpawnPointAssigner
{
	public Transform GetSpawnTransform( Team team )
	{
		var spawns = Game.ActiveScene.GetAllComponents<TeamSpawnPoint>()
			.Select( x => x.Transform.World )
			.Concat( Game.ActiveScene.GetAllComponents<SpawnPoint>()
				.Select( x => x.Transform.World ) )
			.ToArray();

		return Random.Shared.FromArray( spawns );
	}
}
