namespace Facepunch;

/// <summary>
/// Executes all child nodes in parallel. Succeeds when all children succeed.
/// Can optionally fail fast when any child fails.
/// </summary>
public class ParallelNode : BaseBehaviorNode
{
	private readonly IBehaviorNode[] _children;
	private readonly bool _failFast = true;

	public ParallelNode( params IBehaviorNode[] children )
	{
		_children = children;
	}

	protected override NodeResult OnEvaluate( BotContext context )
	{
		bool anyRunning = false;

		foreach ( var child in _children )
		{
			var result = child.Evaluate( context );

			if ( result == NodeResult.Failure && _failFast )
				return NodeResult.Failure;

			if ( result == NodeResult.Running )
				anyRunning = true;
		}

		if ( anyRunning )
			return NodeResult.Running;

		// if none running and none failed, all succeeded
		return NodeResult.Success;
	}

	public override string ToString()
	{
		return string.Join( "+", _children.Select( c => c.ToString() ) );
	}
}
