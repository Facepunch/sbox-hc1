using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

/// <summary>
/// Finds a random navigable point within specified radius using navmesh
/// </summary>
public class GetRandomPointNode : BaseBehaviorNode
{
	private const string TARGET_POSITION_KEY = "target_position";
	private readonly float _radius;
	private readonly int _maxAttempts;

	public GetRandomPointNode( float radius = 8192f, int maxAttempts = 10 )
	{
		_radius = radius;
		_maxAttempts = maxAttempts;
	}

	protected override async Task<NodeResult> OnEvaluate( BotContext context, CancellationToken token )
	{
		var agent = context.MeshAgent;
		if ( !agent.IsValid() )
			return NodeResult.Failure;

		var currentPos = context.Pawn.WorldPosition;

		// Try to find valid position
		for ( int i = 0; i < _maxAttempts; i++ )
		{
			// Get random point within radius
			var randomOffset = Vector3.Random.WithZ( 0 ).Normal * Random.Shared.Float( 0, _radius );
			var targetPos = currentPos + randomOffset;

			// Try to find nearest valid position on navmesh
			if ( agent.Scene.NavMesh.GetClosestPoint( targetPos ) is { } validPos )
			{
				context.SetData( TARGET_POSITION_KEY, validPos );
				return NodeResult.Success;
			}

			await context.Task.FixedUpdate();

			if ( token.IsCancellationRequested )
				return NodeResult.Failure;
		}

		return NodeResult.Failure;
	}
}
