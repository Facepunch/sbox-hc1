
using Facepunch;

partial class GameMode
{
	[Property, HostSync] public bool BuyAnywhere { get; set; } = true;
	[Property, HostSync] public bool UnlimitedMoney { get; set; }
	[Property, HostSync] public int MaxBalance { get; set; } = 16000;

	[DeveloperCommand( "Toggle Buy Anywhere" )]
	private static void Command_ToggleBuyAnywhere()
	{
		Instance.BuyAnywhere = !Instance.BuyAnywhere;
	}

}
