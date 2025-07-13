using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

/// <summary>
/// Moves the bot to a target position using navmesh pathfinding
/// </summary>
public class MoveToNode : BaseBehaviorNode
{
	private readonly float _arrivalDistance;
	private readonly bool _faceDirection;

	public MoveToNode( float arrivalDistance = 50f, bool faceDirection = false )
	{
		_arrivalDistance = arrivalDistance;
		_faceDirection = faceDirection;
	}

	protected override async Task<NodeResult> OnEvaluate( BotContext context, CancellationToken token )
	{
		if ( !context.HasData( "target_position" ) )
			return NodeResult.Failure;

		var targetPos = context.GetData<Vector3>( "target_position" );
		var pawn = context.Pawn;
		var agent = context.MeshAgent;

		if ( !agent.IsValid() )
			return NodeResult.Failure;

		// Start moving
		agent.MoveTo( targetPos );

		// Keep checking until we arrive or get cancelled
		while ( !token.IsCancellationRequested )
		{
			// Check if we've reached target
			var distance = pawn.WorldPosition.DistanceSquared( targetPos );
			if ( distance < _arrivalDistance * _arrivalDistance )
				return NodeResult.Success;

			// Face movement direction if enabled
			if ( _faceDirection && agent.WishVelocity.Length > 0.1f )
			{
				var targetRot = Rotation.LookAt( agent.WishVelocity.Normal );
				pawn.EyeAngles = pawn.EyeAngles.LerpTo( targetRot.Angles(), Time.Delta * 5f );
			}

			await context.Task.FixedUpdate();
		}

		return NodeResult.Running; // Let parent nodes know we're still working
	}
}
