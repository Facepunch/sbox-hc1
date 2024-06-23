using Sandbox;
using System.Threading.Channels;

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
		player.Name = $"PlayerState (BOT)";

		var playerState = player.Components.Get<PlayerState>();
		playerState.BotId = CurrentBotId;

		GameNetworkManager.Instance.OnPlayerJoined( playerState, Connection.Host );

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
			if ( player.PlayerState.IsBot )
				player.GameObject.Destroy();
		}
	}

	// TODO: TEMPORARY LOCATION, DELETE WHEN DONE
	[Property] public GameObject Drone { get; set; }
	private Pawn DronePawn { get; set; }

	[DeveloperCommand( "Toggle Drone", "Drone Stuff" )]
	private static void Command_Become_Drone()
	{
		if ( GameUtils.CurrentPawn is Drone )
		{
			// Switch back to player
			(GameUtils.LocalPlayer as Pawn).Possess();
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

			var drone = newInst.Components.Get<Drone>();
			drone.Team = GameUtils.LocalPlayer.Team;

			var transform = GameUtils.LocalPlayer.Transform;
			drone.Transform.Position = transform.Position + transform.Rotation.Forward * 50f + Vector3.Up * 50f;

			Instance.DronePawn = drone;
			
		}
	}
}
