
using Facepunch;

partial class GameMode
{

	[Property, HostSync]
	public bool BuyAnywhere { get; private set; } = true;

	[DeveloperCommand( "Toggle Buy Anywhere" )]
	private static void Command_ToggleBuyAnywhere()
	{
		Instance.BuyAnywhere = !Instance.BuyAnywhere;
	}

}
