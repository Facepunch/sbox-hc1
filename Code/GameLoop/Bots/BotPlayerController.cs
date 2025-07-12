using Sandbox.Events;

namespace Facepunch;

public class BotPlayerController : Component, IBotController
{
	public StateMachineComponent StateMachine { get; set; }

	public PlayerPawn Player { get; set; }

	protected override void OnAwake()
	{
		Player = GetComponentInParent<PlayerPawn>( true );
		StateMachine = GetComponent<StateMachineComponent>( true );
		Enabled = false;
	}

	void IBotController.OnControl( BotController bot )
	{
		// Turn us on
		StateMachine.Enabled = true;
	}
}
