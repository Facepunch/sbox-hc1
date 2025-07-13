using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

/// <summary>
/// Checks if there are visible enemies within range
/// </summary>
public class HasVisibleEnemiesNode : BaseBehaviorNode
{
	private readonly float _range;
	private const string ENEMIES_KEY = "visible_enemies";

	public HasVisibleEnemiesNode( float range = 2048f )
	{
		_range = range;
	}

	protected override async Task<NodeResult> OnEvaluate( BotContext context, CancellationToken token )
	{
		var rangeSqr = _range * _range;
		var pawn = context.Pawn;

		var enemies = context.Pawn.Scene.GetAllComponents<Pawn>()
			.Where( x =>
				x != pawn &&
				x.HealthComponent.IsValid() &&
				x.HealthComponent.Health > 0 &&
				x.WorldPosition.DistanceSquared( pawn.WorldPosition ) < rangeSqr &&
				x.Team != pawn.Team &&
				IsInLineOfSight( pawn, x ) );

		if ( !enemies.Any() )
			return NodeResult.Failure;

		context.SetData( ENEMIES_KEY, enemies.ToList() );
		return NodeResult.Success;
	}

	private bool IsInLineOfSight( Pawn from, Pawn to )
	{
		var trace = from.Scene.Trace.Ray( from.EyePosition, to.EyePosition + Vector3.Down * 32f )
			.IgnoreGameObjectHierarchy( from.GameObject.Root )
			.Run();

		return trace.Hit && trace.GameObject.Root == to.GameObject;
	}
}
