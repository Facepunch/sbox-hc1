using System.Threading.Tasks;
using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// A simple game rule that kills the game when the game is over, by pulling everyone into the main menu
/// </summary>
public sealed class KillGameOnEnd : Component,
	IGameEventHandler<PreGameEndEvent>,
	IGameEventHandler<PostGameEndEvent>,
	Component.INetworkListener
{
	/// <summary>
	/// How long after the game ends until we show the shutdown warning message?
	/// </summary>
	[Property, HostSync]
	public float ShowDelaySeconds { get; set; } = 5f;

	/// <summary>
	/// How to show the shutdown warning message before actually shutting down?
	/// </summary>
	[Property, HostSync]
	public float DurationSeconds { get; set; } = 5f;

	[Broadcast( NetPermission.HostOnly )]
	private void Disconnect()
	{
		GameUtils.ReturnToMainMenu();
	}

	void IGameEventHandler<PreGameEndEvent>.OnGameEvent( PreGameEndEvent eventArgs )
	{
		GameMode.Instance.Transition( GameState.None, ShowDelaySeconds + DurationSeconds );

		_ = ShowWarningAsync();
	}

	private async Task ShowWarningAsync()
	{
		await GameTask.DelaySeconds( ShowDelaySeconds );

		GameMode.Instance.ShowToast( "The game is over - returning to menu" );
	}

	void IGameEventHandler<PostGameEndEvent>.OnGameEvent( PostGameEndEvent eventArgs )
	{
		Disconnect();
	}
}
