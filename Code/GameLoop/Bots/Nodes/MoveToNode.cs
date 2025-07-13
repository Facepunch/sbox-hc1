using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

/// <summary>
/// Moves the bot to a target position using navmesh pathfinding
/// </summary>
public class MoveToNode : BaseBehaviorNode
{
	private const string TARGET_POSITION_KEY = "target_position";
	private readonly float _arrivalDistance;
	private readonly bool _faceDirection;
	private readonly float _stuckTime;
	private TimeSince _timeSinceLastMove;
	private Vector3 _lastPosition;

	public MoveToNode( float arrivalDistance = 50f, bool faceDirection = false, float stuckTime = 1.0f )
	{
		_arrivalDistance = arrivalDistance;
		_faceDirection = faceDirection;
		_stuckTime = stuckTime;
	}

	protected override async Task<NodeResult> OnEvaluate( BotContext context, CancellationToken token )
	{
		if ( !context.HasData( TARGET_POSITION_KEY ) )
			return NodeResult.Failure;

		var targetPos = context.GetData<Vector3>( TARGET_POSITION_KEY );
		var pawn = context.Pawn;
		var agent = context.MeshAgent;

		if ( !agent.IsValid() )
			return NodeResult.Failure;

		_lastPosition = pawn.WorldPosition;
		_timeSinceLastMove = 0;

		while ( !token.IsCancellationRequested )
		{
			// Check if we've reached target
			var distance = pawn.WorldPosition.DistanceSquared( targetPos );
			if ( distance < _arrivalDistance * _arrivalDistance )
				return NodeResult.Success;

			// Update movement
			agent.MoveTo( targetPos );

			// Face movement direction if enabled
			if ( _faceDirection && agent.WishVelocity.Length > 0.1f )
			{
				var targetRot = Rotation.LookAt( agent.WishVelocity.Normal );
				pawn.EyeAngles = pawn.EyeAngles.LerpTo( targetRot.Angles(), Time.Delta * 5f );
			}

			// Check if we're stuck
			if ( pawn.WorldPosition.DistanceSquared( _lastPosition ) < 1f )
			{
				if ( _timeSinceLastMove > _stuckTime )
					return NodeResult.Failure;
			}
			else
			{
				_lastPosition = pawn.WorldPosition;
				_timeSinceLastMove = 0;
			}

			await context.Task.FixedUpdate();
		}

		return NodeResult.Failure;
	}
}
