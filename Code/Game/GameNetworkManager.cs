using Sandbox.Network;

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
		if ( !IsMultiplayer ) return;

		//
		// Create a lobby if we're not connected
		//
		if ( !GameNetworkSystem.IsActive )
		{
			GameNetworkSystem.CreateLobby();
		}
	}

	[DeveloperCommand( "Add Bot" )]
	private static void Command_Add_Bot()
	{
		var player = Instance.PlayerPrefab.Clone();
		player.NetworkSpawn();
	}

	/// <summary>
	/// Called when a network connection becomes active
	/// </summary>
	/// <param name="channel"></param>
	public void OnActive( Connection channel )
	{
		if ( !IsMultiplayer ) return;

		Log.Info( $"Player '{channel.DisplayName}' is becoming active" );

		var player = PlayerPrefab.Clone( GameMode.Instance.GetSpawnTransform( Team.Unassigned ) );
		player.NetworkSpawn( channel );

		var playerComponent = player.Components.Get<PlayerController>();

		if ( playerComponent.IsValid() )
		{
			playerComponent.NetPossess();
		}
	}
}
