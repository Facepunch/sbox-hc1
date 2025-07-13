using System.Threading;

namespace Facepunch;

/// <summary>
/// The bot for players
/// </summary>
public class BotPlayerController : Component, IBotController
{
	[Property]
	public NavMeshAgent MeshAgent { get; set; }

	public PlayerPawn Player { get; set; }

	protected override void OnAwake()
	{
		Player = GetComponentInParent<PlayerPawn>( true );
		MeshAgent = GetOrAddComponent<NavMeshAgent>();

		// We want to handle this ourselves
		MeshAgent.UpdateRotation = false;
		MeshAgent.UpdatePosition = false;
		MeshAgent.Radius = 48f;
		MeshAgent.Height = 64f;

		foreach ( var behavior in GetComponents<IBotBehavior>( true ) )
		{
			behavior.Initialize( this );
		}

		Enabled = false;
	}

	internal async void UpdateBehaviors( CancellationToken token )
	{
		foreach ( var behavior in GetComponents<IBotBehavior>() )
		{
			if ( await behavior.Update( token ) )
				break;
		}
	}

	/// <summary>
	/// Sometimes we need to synchronize the NavMeshAgent with the physics system.
	/// </summary>
	protected void SyncNavAgentWithPhysics()
	{
		MeshAgent.MaxSpeed = Player.GetWishSpeed();
		Player.WishVelocity = MeshAgent.WishVelocity;

		// Handle desync between agent and physics
		if ( WorldPosition.WithZ( 0 ).DistanceSquared( MeshAgent.AgentPosition.WithZ( 0 ) ) > MeshAgent.Radius * MeshAgent.Radius )
		{
			MeshAgent.SetAgentPosition( WorldPosition );
		}
		if ( MathF.Abs( WorldPosition.z - MeshAgent.AgentPosition.z ) > MeshAgent.Height * MeshAgent.Height )
		{
			MeshAgent.SetAgentPosition( WorldPosition );
		}
	}

	void IBotController.OnControl( BotController bot )
	{
		SyncNavAgentWithPhysics();

		UpdateBehaviors( bot.GameObject.EnabledToken );
	}
}
