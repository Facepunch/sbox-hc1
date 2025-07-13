using Facepunch;
using System.Threading;
using System.Threading.Tasks;

public class RoamBehavior : BaseBotBehavior
{
	private IBehaviorNode _behavior;

	[Property]
	public BotContext Context { get; set; }

	protected override void OnInitialize()
	{
		// Build behavior tree
		_behavior = new SequenceNode(
			new GetRandomPointNode(),
			new MoveToNode( 50, true )
		);
	}

	public override async Task<bool> Update( CancellationToken token )
	{
		Context = new BotContext( Controller, Task );
		var result = await _behavior.Evaluate( Context, token );
		return result == NodeResult.Success;
	}
}
