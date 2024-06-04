using Facepunch;

public sealed class RandomSpawnAssigner : Component, ISpawnAssigner
{
	/// <summary>
	/// Use spawns that include at least one of these tags.
	/// </summary>
	[Property, Title( "Tags" )]
	public List<string> SpawnTags { get; private set; } = new();

	/// <summary>
	/// Avoid spawning within this radius of an active player.
	/// </summary>
	[Property]
	public float MinDistance { get; set; } = 512f;

	/// <summary>
	/// Avoid spawning outside of this radius of an active player.
	/// </summary>
	[Property]
	public float MaxDistance { get; set; } = 2048f;

	/// <summary>
	/// How much is the given transform looking at the given position?
	/// </summary>
	private float GetLookAtScore( Transform transform, Vector3 position )
	{
		var aim = Vector3.Dot( transform.Forward, (position - transform.Position).Normal );
		var dist = Math.Max( (position - transform.Position).Length, MinDistance );

		return aim * MinDistance / dist;
	}

	private float ScoreSpawnPoint( Transform spawn, IReadOnlyList<Transform> players )
	{
		if ( players.Count == 0 )
		{
			return 0f;
		}

		var minDist = MathF.Sqrt( players.Min( x => (x.Position - spawn.Position).LengthSquared ) );

		var distScore = minDist < MinDistance ? minDist / MinDistance : minDist > MaxDistance ? MaxDistance / minDist : 100f;
		var lookingAtScore = players.Sum( x => GetLookAtScore( spawn, x.Position ) ) / players.Count;
		var lookedAtScore = 1f - players.Sum( x => GetLookAtScore( x, spawn.Position ) ) / players.Count;

		return distScore * lookingAtScore * lookedAtScore;
	}

	Transform ISpawnAssigner.GetSpawnPoint( PlayerController player )
	{
		var players = GameUtils.ActivePlayers
			.Where( x => x.HealthComponent.State == LifeState.Alive )
			.Select( x => x.Transform.World )
			.ToArray();

		var spawns = GameUtils.GetSpawnPoints( Team.Unassigned, SpawnTags.ToArray() ).ToArray();

		if ( spawns.Length == 0 )
		{
			throw new Exception( "No unassigned spawn points!" );
		}

		return spawns.MaxBy( x => ScoreSpawnPoint( x, players ) + Random.Shared.NextSingle() );
	}
}
