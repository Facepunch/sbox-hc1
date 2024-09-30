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
		var player = GameNetworkManager.Instance.PlayerStatePrefab.Clone();
		player.Name = $"PlayerState (BOT)";

		var playerState = player.GetComponent<PlayerState>();
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
			if ( player.IsBot )
			{
				player.Kick();
			}
		}
	}
}
