using System.Threading;
using System.Threading.Tasks;

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

	private Dictionary<IBotBehavior, Task> _behaviorTasks = new();
	private CancellationTokenSource _behaviorCts;


	internal void UpdateBehaviors( CancellationToken token )
	{
		// Create/update cancellation token source
		if ( _behaviorCts == null || _behaviorCts.IsCancellationRequested )
		{
			_behaviorCts?.Dispose();
			_behaviorCts = CancellationTokenSource.CreateLinkedTokenSource( token );
		}

		// Get behaviors in priority order
		var behaviors = GetComponents<IBotBehavior>()
			.OrderByDescending( x => x.Priority )
			.ToList();

		// Remove any completed or cancelled tasks
		foreach ( var kv in _behaviorTasks.ToList() )
		{
			if ( kv.Value.IsCompleted || kv.Value.IsCanceled )
			{
				_behaviorTasks.Remove( kv.Key );
			}
		}

		// Update or start behavior tasks
		foreach ( var behavior in behaviors )
		{
			// Skip if task is already running
			if ( _behaviorTasks.TryGetValue( behavior, out var existingTask )
				&& !existingTask.IsCompleted )
				continue;

			// Start new behavior task
			var task = behavior.Update( _behaviorCts.Token );
			_behaviorTasks[behavior] = task;
		}
	}

	protected override void OnDisabled()
	{
		_behaviorCts?.Cancel();
		_behaviorCts?.Dispose();
		_behaviorCts = null;
		_behaviorTasks.Clear();
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
