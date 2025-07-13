using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

public class MoveToFiringPositionNode : BaseBehaviorNode
{
	private const string CURRENT_TARGET_KEY = "current_target";
	private const float UPDATE_INTERVAL = 1.0f;
	private const float TARGET_VISIBLE_MEMORY = 0.1f;
	private const float RANDOM_OFFSET = 500f;

	private Dictionary<PlayerPawn, TimeSince> _lastSeenTargets = new();
	private TimeSince _timeSinceMovementUpdate;

	protected override async Task<NodeResult> OnEvaluate( BotContext context, CancellationToken token )
	{
		if ( !context.HasData( CURRENT_TARGET_KEY ) )
			return NodeResult.Failure;

		var target = context.GetData<PlayerPawn>( CURRENT_TARGET_KEY );
		if ( !target.IsValid() || target.HealthComponent.State != LifeState.Alive )
			return NodeResult.Failure;

		if ( !_lastSeenTargets.ContainsKey( target ) )
			_lastSeenTargets[target] = 0;

		bool canSeeTarget = CanSeeTarget( context.Pawn, target );
		if ( canSeeTarget )
			_lastSeenTargets[target] = 0;

		if ( _timeSinceMovementUpdate > UPDATE_INTERVAL )
		{
			_timeSinceMovementUpdate = 0;

			if ( !canSeeTarget || _lastSeenTargets[target] > TARGET_VISIBLE_MEMORY )
			{
				context.SetData( "target_position", target.WorldPosition );
				context.MeshAgent.MoveTo( target.WorldPosition );
			}
			else
			{
				context.SetData( "target_position", target.WorldPosition + Vector3.Random * RANDOM_OFFSET );
				context.MeshAgent.MoveTo( target.WorldPosition + Vector3.Random * RANDOM_OFFSET );
			}
		}

		return NodeResult.Success;
	}

	private bool CanSeeTarget( PlayerPawn bot, PlayerPawn target )
	{
		var trace = bot.Scene.Trace.Ray( bot.EyePosition, target.EyePosition + Vector3.Down * 32f )
			.IgnoreGameObjectHierarchy( bot.GameObject )
			.Run();

		return trace.Hit && trace.GameObject.Root == target.GameObject;
	}
}
