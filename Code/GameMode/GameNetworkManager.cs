using Sandbox.Network;
using System.Threading.Tasks;
using Sandbox.Events;

namespace Facepunch;

public sealed class GameNetworkManager : SingletonComponent<GameNetworkManager>, Component.INetworkListener
{
	/// <summary>
	/// Which player prefab should we spawn?
	/// </summary>
	[Property] public GameObject PlayerPrefab { get; set; }

	/// <summary>
	/// Is this game multiplayer? If not, we won't create a lobby.
	/// </summary>
	[Property] public bool IsMultiplayer { get; set; } = true;

	protected override void OnStart()
	{
		PlayerId.Init();

		// TODO: create player state again

		//
		// Create a lobby if we're not connected
		//
		if ( !GameNetworkSystem.IsActive )
		{
			GameNetworkSystem.CreateLobby();
		}
	}

	/// <summary>
	/// Called when a network connection becomes active
	/// </summary>
	/// <param name="channel"></param>
	public void OnActive( Connection channel )
	{
		if ( !IsMultiplayer ) return;

		Log.Info( $"Player '{channel.DisplayName}' is becoming active" );

		var player = PlayerPrefab.Clone();
		player.BreakFromPrefab();
		player.Name = $"PlayerState ({channel.DisplayName})";

		var playerState = player.Components.Get<PlayerState>();
		if ( !playerState.IsValid() )
			return;

		OnPlayerJoined( playerState, channel );
	}

	public void OnPlayerJoined( PlayerState playerState, Connection channel )
	{
		// Dunno if we need both of these events anymore? But I'll keep them for now.
		Scene.Dispatch( new PlayerConnectedEvent( playerState ) );

		playerState.GameObject.NetworkSpawn( channel );

		Scene.Dispatch( new PlayerJoinedEvent( playerState ) );
	}
}
