using Sandbox.Network;
using System.Threading.Tasks;
using Sandbox.Events;
using System.Threading.Channels;

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
	/// Tries to recycle a player state owned by this player (if they disconnected) or makes a new one.
	/// </summary>
	/// <param name="channel"></param>
	/// <returns></returns>
	private PlayerState GetOrCreatePlayerState( Connection channel = null )
	{
		var playerStates = Scene.GetAllComponents<PlayerState>();

		var possiblePlayerState = playerStates.FirstOrDefault( x => {
			// A candidate player state has no owner.
			return x.Network.OwnerConnection is null && x.SteamId == channel.SteamId;
		} );

		if ( possiblePlayerState.IsValid() )
		{
			Log.Warning( $"Found existing player state for {channel.SteamId} that we can re-use. {possiblePlayerState}" );
			return possiblePlayerState;
		}

		var player = PlayerPrefab.Clone();
		player.BreakFromPrefab();
		player.Name = $"PlayerState ({channel.DisplayName})";

		var playerState = player.Components.Get<PlayerState>();
		if ( !playerState.IsValid() )
			return null;

		return playerState;
	}

	/// <summary>
	/// Called when a network connection becomes active
	/// </summary>
	/// <param name="channel"></param>
	public void OnActive( Connection channel )
	{
		if ( !IsMultiplayer ) return;

		Log.Info( $"Player '{channel.DisplayName}' is becoming active" );

		var playerState = GetOrCreatePlayerState( channel );
		if ( !playerState.IsValid() )
		{
			throw new Exception( $"Something went wrong when trying to create PlayerState for {channel.DisplayName}" );
		}

		OnPlayerJoined( playerState, channel );
	}

	public void OnPlayerJoined( PlayerState playerState, Connection channel )
	{
		playerState.SteamId = channel.SteamId;

		// Dunno if we need both of these events anymore? But I'll keep them for now.
		Scene.Dispatch( new PlayerConnectedEvent( playerState ) );

		// Either spawn over network, or claim ownership
		if ( playerState.GameObject.NetworkMode != NetworkMode.Object )
			playerState.GameObject.NetworkSpawn( channel );
		else
			playerState.Network.AssignOwnership( channel );

		Scene.Dispatch( new PlayerJoinedEvent( playerState ) );
	}
}
