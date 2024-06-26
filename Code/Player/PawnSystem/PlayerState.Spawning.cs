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

	[Property] public TimeSince TimeSinceRespawnStateChanged;

	/// <summary>
	/// Are we ready to respawn?
	/// </summary>
	[HostSync, Change( nameof( OnRespawnStateChanged ))] public RespawnState RespawnState { get; set; }

	public bool IsRespawning => RespawnState is not RespawnState.Not;

	public void Spawn()
	{
		if ( PlayerPawn.IsValid() )
			return;

		Log.Info( $"Spawning player.. ( {GameObject.Name} ({DisplayName}, {Team}) )" );

		var prefab = PlayerPawnPrefab.Clone();
		var pawn = prefab.Components.Get<PlayerPawn>();
		pawn.PlayerState = this;
		prefab.NetworkSpawn( Network.OwnerConnection );

		PlayerPawn = pawn;
		if ( IsBot )
			Pawn = pawn;
				
		RespawnState = RespawnState.Not;
		pawn.Respawn();
	}

	protected void OnRespawnStateChanged( LifeState oldValue, LifeState newValue )
	{
		TimeSinceRespawnStateChanged = 0f;
	}
}
