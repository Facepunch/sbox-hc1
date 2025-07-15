namespace Facepunch;

public class RoamBehavior : BaseBotBehavior
{
	private IBehaviorNode _behavior;

	public override float Score( BotContext ctx )
	{
		return 10f;
	}

	protected override void OnInitialize()
	{
		// Build behavior tree
		_behavior = new SequenceNode(
			new GetRandomPointNode(),
			new MoveToNode( 50, true )
		);
	}

	public override NodeResult Update( BotContext ctx )
	{
		return _behavior.Evaluate( ctx );
	}
}
