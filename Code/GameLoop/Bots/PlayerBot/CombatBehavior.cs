using Facepunch;
using System.Threading;
using System.Threading.Tasks;

public class CombatBehavior : BaseBotBehavior, IHotloadManaged
{
	private IBehaviorNode _behavior;

	[Property]
	public BotContext Context { get; set; }

	void IHotloadManaged.Created( IReadOnlyDictionary<string, object> state )
	{
		OnInitialize();
	}

	protected override void OnInitialize()
	{
		// Build behavior tree
		_behavior = new SequenceNode(
			new HasVisibleEnemiesNode(),
			new HasVisibleTeammatesNode(),
			new SelectTargetNode(),
			new ReloadWeaponNode(),

			new ParallelNode(
				new MoveToFiringPositionNode(),
				new AimAtTargetNode(),
				new ShootTargetNode()
			)
		);
	}

	public override async Task<bool> Update( CancellationToken token )
	{
		Context = new BotContext( Controller, Task );
		var result = await _behavior.Evaluate( Context, token );
		return result == NodeResult.Success;
	}
}
