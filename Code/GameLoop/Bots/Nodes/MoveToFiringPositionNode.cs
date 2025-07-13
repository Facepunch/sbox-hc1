using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

public class MoveToFiringPositionNode : BaseBehaviorNode
{
	private const string CURRENT_TARGET_KEY = "current_target";
	private const float MOVE_UPDATE_TIME = 1.0f;
	private const float STRAFE_DISTANCE = 128f;
	private const float CHASE_DISTANCE = 500f;
	private const float TARGET_VISIBLE_MEMORY = 0.1f;

	private Dictionary<PlayerPawn, TimeSince> _lastSeenTargets = new();
	private TimeSince _timeSincePositionUpdate;
	private bool _strafeDirection;

	protected override async Task<NodeResult> OnEvaluate( BotContext context, CancellationToken token )
	{
		if ( !context.HasData( CURRENT_TARGET_KEY ) )
			return NodeResult.Failure;

		var target = context.GetData<PlayerPawn>( CURRENT_TARGET_KEY );
		if ( !target.IsValid() || target.HealthComponent.State != LifeState.Alive )
			return NodeResult.Failure;

		// Start tracking this target if we aren't already
		if ( !_lastSeenTargets.ContainsKey( target ) )
			_lastSeenTargets[target] = 0;

		// Update target visibility
		bool canSeeTarget = CanSeeTarget( context.Pawn, target );
		if ( canSeeTarget )
			_lastSeenTargets[target] = 0;

		while ( !token.IsCancellationRequested )
		{
			// Update movement periodically
			if ( _timeSincePositionUpdate > MOVE_UPDATE_TIME )
			{
				_timeSincePositionUpdate = 0;

				// If we can't see target, chase them
				if ( !canSeeTarget || _lastSeenTargets[target] > TARGET_VISIBLE_MEMORY )
				{
					ChaseTarget( context, target );
				}
				// If we can see them, do combat movement
				else
				{
					DoCombatMovement( context, target );
				}

				// Flip strafe direction occasionally
				if ( Random.Shared.Float( 0, 1 ) < 0.3f )
					_strafeDirection = !_strafeDirection;
			}

			await context.Task.FixedUpdate();
		}

		return NodeResult.Success;
	}

	private bool CanSeeTarget( PlayerPawn bot, PlayerPawn target )
	{
		var trace = bot.Scene.Trace.Ray( bot.EyePosition, target.EyePosition + Vector3.Down * 32f )
			.Run();

		return trace.Hit && trace.GameObject.Root == target.GameObject;
	}

	private void ChaseTarget( BotContext context, PlayerPawn target )
	{
		// Move directly to target's position when chasing
		context.MeshAgent.MoveTo( target.WorldPosition );
	}

	private void DoCombatMovement( BotContext context, PlayerPawn target )
	{
		var bot = context.Pawn;
		var toTarget = target.WorldPosition - bot.WorldPosition;
		var distance = toTarget.Length;
		toTarget = toTarget.Normal;

		// Get perpendicular direction for strafing
		var strafeDir = Vector3.Cross( toTarget, Vector3.Up ).Normal;
		if ( !_strafeDirection )
			strafeDir = -strafeDir;

		// Calculate movement position that maintains distance while strafing
		var desiredDistance = Math.Min( distance, CHASE_DISTANCE );
		var targetPos = target.WorldPosition
			- toTarget * desiredDistance // Maintain distance
			+ strafeDir * STRAFE_DISTANCE; // Add strafe offset

		// Validate position
		var trace = bot.Scene.Trace.Ray( targetPos + Vector3.Up * 64, targetPos - Vector3.Up * 64 )
			.WithoutTags( "player" )
			.Run();

		if ( trace.Hit )
			context.MeshAgent.MoveTo( trace.EndPosition );
	}
}
