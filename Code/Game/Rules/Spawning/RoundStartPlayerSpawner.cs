using Facepunch;

/// <summary>
/// Respawn all players at the start of each round.
/// </summary>
public sealed class RoundStartPlayerSpawner : Component, IRoundStartListener
{
	void IRoundStartListener.PreRoundStart()
	{
		Log.Info( nameof( RoundStartPlayerSpawner ) );

		foreach ( var player in GameUtils.ActivePlayers )
		{
			Log.Info(
				$"Calling Respawn on {player.GameObject.Name} ({player.GameObject.Network.OwnerConnection?.DisplayName})" );

			player.Respawn();
		}
	}
}
