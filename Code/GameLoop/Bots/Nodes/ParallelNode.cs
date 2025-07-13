using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

/// <summary>
/// Executes all child nodes in parallel. Succeeds when all children succeed.
/// Can optionally fail fast when any child fails.
/// </summary>
public class ParallelNode : BaseBehaviorNode
{
	private readonly IBehaviorNode[] _children;
	private readonly bool _failFast = true;
	private List<IBehaviorNode> _activeChildren = new();

	public ParallelNode( params IBehaviorNode[] children )
	{
		_children = children;
	}

	protected override async Task<NodeResult> OnEvaluate( BotContext context, CancellationToken token )
	{
		_activeChildren.Clear();

		var tasks = _children.Select( x =>
		{
			_activeChildren.Add( x );
			return x.Evaluate( context, token );
		} ).ToList();

		var results = new List<NodeResult>();

		while ( tasks.Any() )
		{
			var completedTask = await context.Task.WhenAny( tasks );
			var result = await completedTask;
			tasks.Remove( completedTask );

			results.Add( result );

			if ( _failFast && result == NodeResult.Failure )
				return NodeResult.Failure;
		}

		return results.All( x => x == NodeResult.Success )
			? NodeResult.Success
			: NodeResult.Failure;
	}

	public override string Name
	{
		get
		{
			if ( _activeChildren.Count > 0 )
				return string.Join( "+", _activeChildren.Select( x => x.Name ) );

			return "Parallel";
		}
	}
}
