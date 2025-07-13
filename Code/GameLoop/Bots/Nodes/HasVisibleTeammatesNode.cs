using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

/// <summary>
/// Checks if there are visible teammates within range and stores them in context
/// </summary>
public class HasVisibleTeammatesNode : BaseBehaviorNode
{
	private const string VISIBLE_TEAMMATES_KEY = "visible_teammates";
	private const float DEFAULT_RANGE = 8192f;

	private readonly float _range;

	public HasVisibleTeammatesNode( float range = DEFAULT_RANGE )
	{
		_range = range;
	}

	protected override async Task<NodeResult> OnEvaluate( BotContext context, CancellationToken token )
	{
		var pawn = context.Pawn;
		if ( !pawn.IsValid() )
			return NodeResult.Failure;

		var rangeSqr = _range * _range;

		// Find teammates in range
		var teammates = Game.ActiveScene.GetAll<PlayerPawn>()
			.Where( x =>
				x != pawn &&
				x.HealthComponent.State == LifeState.Alive &&
				x.Team == pawn.Team &&
				x.WorldPosition.DistanceSquared( pawn.WorldPosition ) < rangeSqr
			);

		// Check visibility for each teammate
		var visibleTeammates = new List<PlayerPawn>();

		foreach ( var teammate in teammates )
		{
			// Check if we have line of sight
			var trace = pawn.Scene.Trace.Ray( pawn.EyePosition, teammate.EyePosition + Vector3.Down * 32f )
				.IgnoreGameObjectHierarchy( pawn.GameObject.Root )
				.Run();

			// Can see teammate if trace ends at them
			if ( trace.Hit && trace.GameObject.Root == teammate.GameObject )
			{
				visibleTeammates.Add( teammate );
			}
		}

		// Store visible teammates in context
		if ( visibleTeammates.Count > 0 )
		{
			context.SetData( VISIBLE_TEAMMATES_KEY, visibleTeammates );
			return NodeResult.Success;
		}

		return NodeResult.Success;
	}
}
