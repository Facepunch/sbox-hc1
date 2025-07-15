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

	protected override NodeResult OnEvaluate( BotContext context )
	{
		if ( !context.HasData( "target_position" ) )
			return NodeResult.Failure;

		var targetPos = context.GetData<Vector3>( "target_position" );
		var pawn = context.Pawn;
		var agent = context.MeshAgent;

		if ( !agent.IsValid() )
			return NodeResult.Failure;

		// Step agent toward target
		agent.MoveTo( targetPos );

		// Check if we've reached target
		float distSqr = pawn.WorldPosition.DistanceSquared( targetPos );
		if ( distSqr < _arrivalDistance * _arrivalDistance )
			return NodeResult.Success;

		// Face movement direction if desired
		if ( _faceDirection && agent.WishVelocity.Length > 0.1f )
		{
			var targetRot = Rotation.LookAt( agent.WishVelocity.Normal );
			pawn.EyeAngles = pawn.EyeAngles.LerpTo( targetRot.Angles(), Time.Delta * 5f );
		}

		return NodeResult.Running;
	}
}

