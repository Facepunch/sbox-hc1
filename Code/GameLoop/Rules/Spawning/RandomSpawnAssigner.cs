
namespace Facepunch;

public sealed class RandomSpawnAssigner : Component, ISpawnAssigner
{
	/// <summary>
	/// Use spawns that include at least one of these tags.
	/// </summary>
	[Property, Title( "Tags" )]
	public TagSet SpawnTags { get; private set; } = new();

	/// <summary>
	/// Avoid spawning within this radius of an enemy player.
	/// </summary>
	[Property]
	public float MinEnemyDistance { get; set; } = 512f;

	/// <summary>
	/// Assume an enemy player is out of line-of-sight if at least this far away.
	/// </summary>
	[Property]
	public float SkipLineOfSightTestDistance { get; set; } = 2048;

	private bool IsValidSpawnPoint( SpawnPointInfo spawn, IReadOnlyList<Transform> allPlayers, IReadOnlyList<Transform> enemyPlayers )
	{
		// Don't spawn inside another player

		foreach ( var player in allPlayers )
		{
			if ( (player.Position - spawn.Position).LengthSquared < 64f * 64f )
			{
				return false;
			}
		}

		// Don't spawn close to an enemy

		foreach ( var player in enemyPlayers )
		{
			if ( (player.Position - spawn.Position).LengthSquared < MinEnemyDistance * MinEnemyDistance )
			{
				return false;
			}
		}

		// Don't spawn in line of sight of an enemy
		// TODO: is this too expensive? will a ray not be a good enough test? is there still baked PVS in the map for us to use?

		foreach ( var player in enemyPlayers )
		{
			if ( (player.Position - spawn.Position).LengthSquared > SkipLineOfSightTestDistance * SkipLineOfSightTestDistance )
			{
				continue;
			}

			var tr = Scene.Trace
				.FromTo( spawn.Position + Vector3.Up * 64f, player.Position + Vector3.Up )
				.Size( 0.01f )
				.WithoutTags( "player", "ragdoll", "glass", "np_player" )
				.Run();

			if ( !tr.Hit )
			{
				return false;
			}
		}

		return true;
	}

	SpawnPointInfo ISpawnAssigner.GetSpawnPoint( PlayerState player )
	{
		var allPlayers = GameUtils.PlayerPawns
			.Where( x => x.PlayerState != player )
			.Where( x => x.HealthComponent.State == LifeState.Alive )
			.Select( x => x.Transform.World )
			.ToArray();

		var enemyPlayers = GameUtils.PlayerPawns
			.Where( x => x.PlayerState != player )
			.Where( x => !x.PlayerState.IsFriendly( player ) )
			.Where( x => x.HealthComponent.State == LifeState.Alive )
			.Select( x => x.Transform.World )
			.ToArray();

		var spawns = GameUtils.GetSpawnPoints( Team.Unassigned, SpawnTags.ToArray() )
			.Shuffle();

		foreach ( var spawn in spawns )
		{
			if ( IsValidSpawnPoint( spawn, allPlayers, enemyPlayers ) )
			{
				return spawn;
			}
		}

		// Fallback

		Log.Warning( $"Used fallback spawn for {player.DisplayName}" );

		return spawns[0];
	}
}
