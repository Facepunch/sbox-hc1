using System.Threading;
using System.Threading.Tasks;

namespace Facepunch;

/// <summary>
/// Executes child nodes in sequence until one fails
/// </summary>
public class SequenceNode : BaseBehaviorNode
{
	private readonly IBehaviorNode[] _children;
	private IBehaviorNode _activeChild;

	public SequenceNode( params IBehaviorNode[] children )
	{
		_children = children;
	}

	protected override async Task<NodeResult> OnEvaluate( BotContext context, CancellationToken token )
	{
		foreach ( var child in _children )
		{
			_activeChild = child;
			var result = await child.Evaluate( context, token );
			if ( result != NodeResult.Success )
				return result;
		}

		return NodeResult.Success;
	}

	public override string Name
	{
		get
		{
			if ( _activeChild != null )
				return _activeChild.Name;

			return "Sequence";
		}
	}
}
