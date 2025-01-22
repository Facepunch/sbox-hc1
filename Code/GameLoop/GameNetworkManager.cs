using Sandbox.Network;
using System.Threading.Tasks;
using Sandbox.Events;
using System.Threading.Channels;
using Sandbox.Diagnostics;

namespace Facepunch;

public sealed class GameNetworkManager : SingletonComponent<GameNetworkManager>, Component.INetworkListener
{
	/// <summary>
	/// Which player prefab should we spawn?
	/// </summary>
	[Property] public GameObject ClientPrefab { get; set; }

	/// <summary>
	/// Is this game multiplayer? If not, we won't create a lobby.
	/// </summary>
	[Property] public bool IsMultiplayer { get; set; } = true;

	protected override void OnStart()
	{
		if ( !IsMultiplayer )
		{
			OnActive( Connection.Local );
			return;
		}

		//
		// Create a lobby if we're not connected
		//
		if ( !Networking.IsActive )
		{
			Networking.CreateLobby( new LobbyConfig()
			{
				// ?
			} );
		}
	}

	/// <summary>
	/// Tries to recycle a player state owned by this player (if they disconnected) or makes a new one.
	/// </summary>
	/// <param name="channel"></param>
	/// <returns></returns>
	private Client GetOrCreateClient( Connection channel = null )
	{
		var Clients = Scene.GetAllComponents<Client>();

		var possibleClient = Clients.FirstOrDefault( x => {
			// A candidate player state has no owner.
			return x.Connection is null && x.SteamId == channel.SteamId;
		} );

		if ( possibleClient.IsValid() )
		{
			Log.Warning( $"Found existing player state for {channel.SteamId} that we can re-use. {possibleClient}" );
			return possibleClient;
		}

		if ( !ClientPrefab.IsValid() )
		{
			Log.Warning( "Could not spawn player as no ClientPrefab assigned." );
			return null;
		}

		var player = ClientPrefab.Clone();
		player.BreakFromPrefab();
		player.Name = $"Client ({channel.DisplayName})";
		player.Network.SetOrphanedMode( NetworkOrphaned.ClearOwner );

		var Client = player.GetComponent<Client>();
		if ( !Client.IsValid() )
			return null;

		return Client;
	}

	/// <summary>
	/// Called when a network connection becomes active
	/// </summary>
	/// <param name="channel"></param>
	public void OnActive( Connection channel )
	{
		Log.Info( $"Player '{channel.DisplayName}' is becoming active" );

		var Client = GetOrCreateClient( channel );
		if ( !Client.IsValid() )
		{
			throw new Exception( $"Something went wrong when trying to create Client for {channel.DisplayName}" );
		}

		OnPlayerJoined( Client, channel );
	}

	public void OnPlayerJoined( Client Client, Connection channel )
	{
		// Dunno if we need both of these events anymore? But I'll keep them for now.
		Scene.Dispatch( new PlayerConnectedEvent( Client ) );

		// Either spawn over network, or claim ownership
		if ( !Client.Network.Active )
			Client.GameObject.NetworkSpawn( channel );
		else
			Client.Network.AssignOwnership( channel );

		Client.HostInit();
		Client.ClientInit();

		Scene.Dispatch( new PlayerJoinedEvent( Client ) );
	}
}
