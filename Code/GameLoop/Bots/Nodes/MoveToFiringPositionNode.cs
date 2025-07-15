namespace Facepunch;

/// <summary>
/// Moves toward the current target until within firing range.
/// Relies on perception data for visibility (no extra trace).
/// </summary>
public class MoveToFiringPositionNode : BaseBehaviorNode
{
	private const string CURRENT_TARGET_KEY = "current_target";
	private const float FIRE_RANGE = 2048f;
	private const float UPDATE_INTERVAL = 0.25f;

	private TimeSince _timeSinceMovementUpdate = 0;

	protected override NodeResult OnEvaluate( BotContext context )
	{
		// Must have a target
		if ( !context.HasData( CURRENT_TARGET_KEY ) )
			return NodeResult.Failure;

		var target = context.GetData<Pawn>( CURRENT_TARGET_KEY );
		if ( target == null || !target.IsValid() )
			return NodeResult.Failure;

		// Use perception system's visibility instead of tracing
		if ( !IsTargetVisibleFromPerception( context, target ) )
			return NodeResult.Failure;

		var distanceSqr = context.Pawn.WorldPosition.DistanceSquared( target.WorldPosition );

		// If already in range, no movement needed
		if ( distanceSqr <= FIRE_RANGE * FIRE_RANGE )
			return NodeResult.Success;

		// Throttle path updates
		if ( _timeSinceMovementUpdate > UPDATE_INTERVAL )
		{
			_timeSinceMovementUpdate = 0;
			context.MeshAgent.MoveTo( target.WorldPosition );
		}

		return NodeResult.Running;
	}

	private bool IsTargetVisibleFromPerception( BotContext context, Pawn target )
	{
		if ( !context.HasData( "visible_enemies" ) )
			return false;

		var visibleEnemies = context.GetData<List<Pawn>>( "visible_enemies" );
		if ( visibleEnemies == null )
			return false;

		return visibleEnemies.Contains( target );
	}
}
