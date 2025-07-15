namespace Facepunch;

/// <summary>
/// Checks if there are visible enemies in the blackboard.
/// </summary>
public class HasVisibleEnemiesNode : BaseBehaviorNode
{
	private const string ENEMIES_KEY = "visible_enemies";

	protected override NodeResult OnEvaluate( BotContext context )
	{
		// Perception system (UpdatePerceptionNode) should have already populated this list
		if ( !context.HasData( ENEMIES_KEY ) )
			return NodeResult.Failure;

		var enemies = context.GetData<List<Pawn>>( ENEMIES_KEY );
		if ( enemies == null || enemies.Count == 0 )
			return NodeResult.Failure;

		// Already stored by UpdatePerceptionNode, but we can set again to be explicit
		context.SetData( ENEMIES_KEY, enemies );
		return NodeResult.Success;
	}
}
