using System.Threading.Tasks;

namespace Facepunch;

/// <summary>
/// A simple game rule that kills the game when the game is over, by pulling everyone into the main menu
/// </summary>
public sealed class KillGameOnEnd : Component, IGameEndListener, Component.INetworkListener
{
	[Broadcast( NetPermission.HostOnly )]
	private void Disconnect()
	{
		GameUtils.ReturnToMainMenu();
	}

	async Task IGameEndListener.OnGameEnd()
	{
		GameMode.Instance.ShowToast( "The game is over - returning to menu", UI.ToastType.Generic );
		await GameTask.DelaySeconds( 5f );
		Disconnect();
	}
}
