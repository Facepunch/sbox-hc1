namespace Facepunch;

public class BotPlayerController : Component, IBotController
{
	[Property]
	public NavMeshAgent MeshAgent { get; set; }

	SimpleBotBehavior SimpleBot { get; set; }

	public PlayerPawn Player { get; set; }

	protected override void OnAwake()
	{
		Player = GetComponentInParent<PlayerPawn>( true );
		MeshAgent = GetOrAddComponent<NavMeshAgent>();
		SimpleBot = GetOrAddComponent<SimpleBotBehavior>();
		Enabled = false;

		// We want to handle this ourselves
		MeshAgent.UpdateRotation = false;
		MeshAgent.UpdatePosition = false;
		MeshAgent.Radius = 48f;
		MeshAgent.Height = 64f;
	}

	void IBotController.OnControl( BotController bot )
	{
		SimpleBot.Tick();
	}
}
