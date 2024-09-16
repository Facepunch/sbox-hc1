using Sandbox.Events;

namespace Facepunch;

public enum RespawnState
{
	Not,
	Requested,
	Delayed,
	Immediate
}

public partial class PlayerState
{
	/// <summary>
	/// The prefab to spawn when we want to make a player pawn for the player.
	/// </summary>
	[Property] public GameObject PlayerPawnPrefab { get; set; }

	public TimeSince TimeSinceRespawnStateChanged { get; private set; }
	public DamageInfo LastDamageInfo { get; private set; }

	/// <summary>
	/// Are we ready to respawn?
	/// </summary>
	[HostSync, Change( nameof( OnRespawnStateChanged ))] public RespawnState RespawnState { get; set; }

	public bool IsRespawning => RespawnState is RespawnState.Delayed;

	private void Spawn( SpawnPointInfo spawnPoint )
	{
		var prefab = PlayerPawnPrefab.Clone( spawnPoint.Transform );
		var pawn = prefab.Components.Get<PlayerPawn>();

		pawn.PlayerState = this;

		pawn.SetSpawnPoint( spawnPoint );

		prefab.NetworkSpawn( Network.OwnerConnection );

		PlayerPawn = pawn;
		if ( IsBot )
			Pawn = pawn;
				
		RespawnState = RespawnState.Not;
		pawn.OnRespawn();
	}

	public void Respawn( bool forceNew )
	{
		var spawnPoint = GameMode.Instance.Get<ISpawnAssigner>() is { } spawnAssigner
			? spawnAssigner.GetSpawnPoint( this )
			: GameUtils.GetRandomSpawnPoint( Team );

		Log.Info( $"Spawning player.. ( {GameObject.Name} ({DisplayName}, {Team}), {spawnPoint.Position}, [{string.Join( ", ", spawnPoint.Tags )}] )" );

		if ( forceNew || !PlayerPawn.IsValid() || PlayerPawn.HealthComponent.State == LifeState.Dead )
		{
			PlayerPawn?.GameObject?.Destroy();
			PlayerPawn = null;

			Spawn( spawnPoint );
		}
		else
		{
			PlayerPawn.SetSpawnPoint( spawnPoint );
			PlayerPawn.OnRespawn();
		}
	}

	public void OnKill( DamageInfo damageInfo )
	{
		LastDamageInfo = damageInfo;
	}

	protected void OnRespawnStateChanged( LifeState oldValue, LifeState newValue )
	{
		TimeSinceRespawnStateChanged = 0f;
	}

	public PlayerPawn GetLastKiller()
	{
		return GameUtils.GetPlayerFromComponent( LastDamageInfo?.Attacker );
	}
}
