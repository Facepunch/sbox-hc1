using Sandbox;

namespace Facepunch;

public sealed class BotManager : SingletonComponent<BotManager>
{
	private static readonly string[] BotNames =
	{
		"Gordon",
		"Jamie",
		"Nigella",
		"Heston",
		"Anthony",
		"Ainsley",
		"Delia",
		"Loyd",
		"Paul",
		"Marco"
	};
	public string[] Names;

	[HostSync] private int CurrentBotId { get; set; } = 0;

	protected override void OnAwake()
	{
		base.OnAwake();

		Names = BotNames.Shuffle().ToArray();
	}

	public void AddBot()
	{
		var player = GameNetworkManager.Instance.PlayerPrefab.Clone();

		PlayerController playerController = player.Components.Get<PlayerController>();
		playerController.BotId = CurrentBotId;

		GameNetworkManager.Instance.OnPlayerJoined( playerController, Connection.Host );

		CurrentBotId++;
	}

	public string GetName(int id)
	{
		return Names[id % Names.Length];
	}

	[DeveloperCommand( "Add Bot", "Game Loop" )]
	private static void Command_Add_Bot()
	{
		Instance.AddBot();
	}


	[DeveloperCommand( "Add 9 Bots", "Game Loop" )]
	private static void Command_Add_Bots_Filled()
	{
		for ( int i = 0; i < 9; i++ )
		{
			Instance.AddBot();
		}
	}

	[DeveloperCommand( "Kick all Bots", "Game Loop" )]
	private static void Command_Kick_Bots()
	{
		foreach ( var player in GameUtils.AllPlayers )
		{
			if ( player.IsBot )
				player.GameObject.Destroy();
		}
	}

	// TODO: TEMPORARY LOCATION, DELETE WHEN DONE
	[Property] public GameObject Drone { get; set; }
	private IPawn DronePawn { get; set; }

	[DeveloperCommand( "Toggle Drone", "Drone Stuff" )]
	private static void Command_Become_Drone()
	{
		if ( GameUtils.Viewer is Drone )
		{
			// Switch back to player
			(GameUtils.LocalPlayer as IPawn).Possess();
		}
		else
		{
			if ( Instance.DronePawn.IsValid() )
			{
				Instance.DronePawn.Possess();
				return;
			}

			var newInst = Instance.Drone.Clone();
			newInst.NetworkSpawn();
			Instance.DronePawn = newInst.Components.Get<IPawn>();
		}
	}
}
