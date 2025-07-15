namespace Facepunch;

/// <summary>
/// Finds a random navigable point within specified radius using navmesh
/// </summary>
public class GetRandomPointNode : BaseBehaviorNode
{
	private const string TARGET_POSITION_KEY = "target_position";
	private bool _hasRun;

	protected override NodeResult OnEvaluate( BotContext context )
	{
		// Only pick a random point once per sequence run
		if ( _hasRun )
			return NodeResult.Success;

		var randomPoint = Game.ActiveScene.NavMesh.GetRandomPoint();
		if ( !randomPoint.HasValue )
			return NodeResult.Failure;

		context.SetData( TARGET_POSITION_KEY, randomPoint.Value );
		_hasRun = true;
		return NodeResult.Success;
	}

	// Optional: reset if needed when sequence resets
	public void Reset()
	{
		_hasRun = false;
	}
}
