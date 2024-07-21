using Sandbox.Events;

namespace Facepunch;

public record GamemodeInitializedEvent( string Title ) : IGameEvent;

/// <summary>
/// Handles the main game loop, using components that listen to state change
/// events to handle game logic.
/// </summary>
public sealed partial class GameMode : SingletonComponent<GameMode>, Component.INetworkListener
{
	/// <summary>
	/// If we're the host, the path in the scene of the selected game mode.
	/// </summary>
	public static string ActivePath { get; private set; }

	/// <summary>
	/// Sets the current gamemode for the next scene load.
	/// </summary>
	/// <param name="gameMode"></param>
	public static void SetCurrent( GameModeInfo gameMode )
	{
		if ( gameMode is null )
		{
			ActivePath = null;
			return;
		}

		ActivePath = gameMode.Path;
	}

	[Property]
	public string Title { get; set; }

	[Property]
	public string Description { get; set; }

	private StateMachineComponent _stateMachine;

	public StateMachineComponent StateMachine => _stateMachine ??= Components.GetInDescendantsOrSelf<StateMachineComponent>();

	private TimeSince _sinceLastSoundHandleLog;

	protected override void OnAwake()
	{
		if ( Networking.IsHost )
		{
			// Only stay enabled if host chose this game mode

			if ( ActivePath is { } path && !path.Equals( GameObject.GetScenePath(), StringComparison.OrdinalIgnoreCase ) )
			{
				GameObject.Enabled = false;
				return;
			}

			// Fallback for testing in editor - just use first active game mode

			if ( Instance is { IsValid: true, Active: true, Scene: { } scene } && scene == Scene )
			{
				Log.Info( $"A GameMode is already active, disabling {GameObject.GetScenePath()}" );
				GameObject.Enabled = false;
				return;
			}
		}

		base.OnAwake();
	}

	protected override void OnStart()
	{
		Scene.Dispatch( new GamemodeInitializedEvent( Title ) );

		base.OnStart();

		GameUtils.LogPlayers();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( _sinceLastSoundHandleLog > 5f )
		{
			_sinceLastSoundHandleLog = 0f;

			var list = new List<SoundHandle>();
			SoundHandle.GetActive( list );

			var mostCommon = list
				.GroupBy( x => x.Name )
				.Select( x => (Name: x.Key, Count: x.Count()) )
				.OrderByDescending( x => x.Count )
				.FirstOrDefault();

			Log.Info( $"Active sound handle count: {list.Count}, most common: {mostCommon.Name} ({mostCommon.Count})" );
		}
	}

	void INetworkListener.OnBecameHost( Connection previousHost )
	{
		Log.Info( "We became the host, taking over the game loop..." );

		GameUtils.LogPlayers();
	}

	private StateComponent _prevState;
	private readonly Dictionary<Type, Component> _componentCache = new();

	/// <summary>
	/// Gets the given component from within the game mode's object hierarchy, or null if not found / enabled.
	/// </summary>
	public T Get<T>( bool required = false )
		where T : class
	{
		if ( _prevState != StateMachine.CurrentState )
		{
			_prevState = StateMachine.CurrentState;
			_componentCache.Clear();
		}

		if ( !_componentCache.TryGetValue( typeof(T), out var component ) || component is { IsValid: false } || component is { Active: false } )
		{
			component = Components.GetInDescendantsOrSelf<T>() as Component;
			_componentCache[typeof(T)] = component;
		}

		if ( required && component is not T )
		{
			throw new Exception( $"Expected a {typeof(T).Name} to be active in the {nameof(GameMode)}!" );
		}

		return component as T;
	}
}
