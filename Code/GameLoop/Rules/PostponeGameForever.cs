using Facepunch;
using System.Threading.Tasks;
using Facepunch.UI;

/// <summary>
/// Never start the game, useful for testing weapons.
/// </summary>
public sealed class PostponeGameForever : Component, IGameStartListener
{
	async Task IGameStartListener.OnGameStart()
	{
		GameMode.Instance.ShowStatusText( "Testing Mode" );

		while ( true )
		{
			await Task.DelaySeconds( 1f );
		}
	}
}
