using Facepunch;
using System.Threading;
using System.Threading.Tasks;

public class RoamBehavior : BaseBotBehavior
{
	private IBehaviorNode _behavior;

	protected override void OnInitialize()
	{
		// Build behavior tree
		_behavior = new SequenceNode(
			new GetRandomPointNode(),
			new MoveToNode()
		);
	}

	public override async Task<bool> Update( CancellationToken token )
	{
		var context = new BotContext( Controller, Task );
		var result = await _behavior.Evaluate( context, token );
		return result == NodeResult.Success;
	}
}
