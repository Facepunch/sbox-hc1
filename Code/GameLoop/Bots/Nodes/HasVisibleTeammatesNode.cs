namespace Facepunch;

/// <summary>
/// Checks if there are visible teammates in the blackboard.
/// </summary>
public class HasVisibleTeammatesNode : BaseBehaviorNode
{
	private const string TEAMMATES_KEY = "visible_teammates";

	protected override NodeResult OnEvaluate( BotContext context )
	{
		// Perception system (UpdatePerceptionNode) should have already populated this list
		if ( !context.HasData( TEAMMATES_KEY ) )
			return NodeResult.Failure;

		var teammates = context.GetData<List<Pawn>>( TEAMMATES_KEY );
		if ( teammates == null || teammates.Count == 0 )
			return NodeResult.Failure;

		context.SetData( TEAMMATES_KEY, teammates );
		return NodeResult.Success;
	}
}
