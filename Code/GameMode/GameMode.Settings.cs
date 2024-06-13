
namespace Facepunch;

partial class GameMode
{
	[Property, HostSync] public bool UnlimitedMoney { get; set; }
	[Property, HostSync] public int MaxBalance { get; set; } = 16000;

	[DeveloperCommand( "Toggle Buy Anywhere", "Game Loop" )]
	private static void Command_ToggleBuyAnywhere()
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.CanBuyAnywhere = !player.CanBuyAnywhere;
		}
	}
}
