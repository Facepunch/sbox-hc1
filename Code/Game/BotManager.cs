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

	[DeveloperCommand( "Add Bot" )]
	private static void Command_Add_Bot()
	{
		Instance.AddBot();
	}


	[DeveloperCommand( "Add 9 Bots" )]
	private static void Command_Add_Bots_Filled()
	{
		for ( int i = 0; i < 9; i++ )
		{
			Instance.AddBot();
		}
	}
}
