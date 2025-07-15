using System.Text.Json.Serialization;

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

	private IBotBehavior _currentBehavior;
	private BotContext _frameContext;

	private TimeSince _timeSincePerception = 0;
	private const float _perceptionInterval = 0.5f;

	[Property, JsonIgnore, ReadOnly]
	public BotContext Context => _frameContext;

	internal void UpdateBehaviors()
	{
		using var _ = Sandbox.Diagnostics.Performance.Scope( "HC1::UpdateBehaviors" );

		// Build or reuse a context for this frame
		if ( _frameContext == null || _frameContext.Controller != this )
			_frameContext = new BotContext( this );

		// Run perception only if interval has passed
		if ( _timeSincePerception > _perceptionInterval )
		{
			_timeSincePerception = 0;
			var perceptionNode = new UpdatePerceptionNode();
			perceptionNode.Evaluate( _frameContext );
		}

		// --- Score all behaviors using the same context ---
		var scored = GetComponents<IBotBehavior>()
			.Select( b => new { Behavior = b, Score = b.Score( _frameContext ) } )
			.OrderByDescending( x => x.Score )
			.ToList();

		if ( scored.Count == 0 || scored[0].Score <= 0f )
		{
			_currentBehavior = null;
			return;
		}

		var topBehavior = scored[0].Behavior;
		var topScore = scored[0].Score;

		// If a different behavior is running and a higher-scoring one appears, switch
		if ( _currentBehavior != null && _currentBehavior != topBehavior )
		{
			var currentScore = _currentBehavior.Score( _frameContext );
			if ( topScore > currentScore )
			{
				_currentBehavior = topBehavior;
			}
		}

		// If no current behavior, pick the top-scoring one
		if ( _currentBehavior == null )
		{
			_currentBehavior = topBehavior;
		}

		// --- Tick the chosen behavior using the same context ---
		if ( _currentBehavior != null )
		{
			var result = _currentBehavior.Update( _frameContext );
			if ( result != NodeResult.Running && result != NodeResult.Success )
			{
				_currentBehavior = null;
			}
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
		UpdateBehaviors();
	}
}
