namespace Facepunch;

/// <summary>
/// Base interface for all behavior tree nodes
/// </summary>
public interface IBehaviorNode
{
	string Name => GetType().Name.Replace( "Node", "" );

	/// <summary>
	/// Evaluate this node's behavior
	/// </summary>
	NodeResult Evaluate( BotContext context );
}

/// <summary>
/// Base class for behavior nodes that handles common functionality
/// </summary>
public abstract class BaseBehaviorNode : IBehaviorNode
{
	public virtual string Name => GetType().Name.Replace( "Node", "" );

	public NodeResult Evaluate( BotContext context )
	{
		// subclasses implement their own synchronous evaluation
		return OnEvaluate( context );
	}

	protected abstract NodeResult OnEvaluate( BotContext context );
}

/// <summary>
/// Result of node evaluation
/// </summary>
public enum NodeResult
{
	Failure,
	Success,
	Running
}

/// <summary>
/// Shared context passed between nodes during behavior tree evaluation
/// </summary>
public class BotContext
{
	public BotPlayerController Controller { get; }
	public PlayerPawn Pawn => Controller.Player;
	public NavMeshAgent MeshAgent => Controller.MeshAgent;
	public TaskSource Task { get; init; }

	// Blackboard for sharing data between nodes
	private Dictionary<string, object> _blackboard = new();

	public IReadOnlyDictionary<string, object> Blackboard => _blackboard.AsReadOnly();

	public BotContext( BotPlayerController controller )
	{
		Controller = controller;
	}

	public void SetData<T>( string key, T value ) => _blackboard[key] = value;
	public T GetData<T>( string key ) => (T)_blackboard[key];
	public bool HasData( string key ) => _blackboard.ContainsKey( key );
}
