namespace Facepunch;

/// <summary>
/// Respawn players on input, after a delay.
/// </summary>
public sealed class PlayerInputRespawner : Respawner
{
	/// <summary>
	/// Which input action should we listen to?
	/// </summary>
	[Property, InputAction] 
	public string InputAction { get; set; } = "Jump";

	protected override void OnUpdate()
	{
		var player = PlayerState.Local;

		if ( player.PlayerPawn.IsValid() && player.PlayerPawn.HealthComponent.State == LifeState.Alive )
			return;

		if ( Input.Pressed( InputAction ) )
		{
			using ( Rpc.FilterInclude( Connection.Host ) )
			{
				AskToRespawn();
			}
		}
	}

	/// <summary>
	/// A RPC sent to the host to ask them if we can respawn
	/// </summary>
	[Rpc.Broadcast]
	private void AskToRespawn()
	{
		var rpcCaller = Rpc.Caller;
		var player = GameUtils.AllPlayers.FirstOrDefault( x => x.Network.Owner == rpcCaller );

		if ( !player.IsValid() )
			return;

		if ( player.PlayerPawn.IsValid() && player.PlayerPawn.HealthComponent.State == LifeState.Alive )
			return;

		player.RespawnState = RespawnState.Immediate;
	}
}
