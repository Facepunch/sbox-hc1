namespace Facepunch;

public class SequenceNode : IBehaviorNode
{
	private readonly List<IBehaviorNode> _children = new();
	private int _currentIndex = 0;

	public SequenceNode( params IBehaviorNode[] nodes )
	{
		_children.AddRange( nodes );
	}

	public NodeResult Evaluate( BotContext context )
	{
		// run current child until it succeeds or fails
		while ( _currentIndex < _children.Count )
		{
			var result = _children[_currentIndex].Evaluate( context );

			if ( result == NodeResult.Running )
			{
				return NodeResult.Running;
			}

			if ( result == NodeResult.Failure )
			{
				_currentIndex = 0;
				return NodeResult.Failure;
			}

			// success, move to next
			_currentIndex++;
		}

		// all children succeeded
		_currentIndex = 0;
		return NodeResult.Success;
	}

	public void Reset()
	{
		_currentIndex = 0;
	}
}
