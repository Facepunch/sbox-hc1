using System.Threading.Tasks;
using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Show a warning message when entering this state, then throw everyone back to the menu when this state ends.
/// </summary>
public sealed class KillGameOnEnd : Component,
	IGameEventHandler<EnterStateEvent>,
	IGameEventHandler<LeaveStateEvent>
{
	[Property, Sync( SyncFlags.FromHost )]
	public string Message { get; set; } = "The game is over - returning to menu";

	[Rpc.Broadcast( NetFlags.HostOnly )]
	private void Disconnect()
	{
		GameUtils.ReturnToMainMenu();
	}

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		if ( !string.IsNullOrEmpty( Message ) )
		{
			GameMode.Instance.ShowToast( Message, duration: eventArgs.State.DefaultDuration );
		}
	}

	void IGameEventHandler<LeaveStateEvent>.OnGameEvent( LeaveStateEvent eventArgs )
	{
		Disconnect();
	}
}
