using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

/// <summary>
/// Finds a random navigable point within specified radius using navmesh
/// </summary>
public class GetRandomPointNode : BaseBehaviorNode
{
	private const string TARGET_POSITION_KEY = "target_position";

	protected override async Task<NodeResult> OnEvaluate( BotContext context, CancellationToken token )
	{
		var randomPoint = Game.ActiveScene.NavMesh.GetRandomPoint();
		if ( !randomPoint.HasValue )
			return NodeResult.Failure;

		context.SetData( TARGET_POSITION_KEY, randomPoint.Value );

		await context.Task.FixedUpdate();

		if ( token.IsCancellationRequested )
			return NodeResult.Failure;

		return NodeResult.Success;
	}
}
