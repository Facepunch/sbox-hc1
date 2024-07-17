
using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Event dispatched on the host when <see cref="RoundCounter"/> should be reset.
/// </summary>
public record RoundCounterResetEvent : IGameEvent;

/// <summary>
/// Event dispatched on the host when <see cref="RoundCounter"/> should be incremented.
/// </summary>
public record RoundCounterIncrementedEvent : IGameEvent;

/// <summary>
/// Keep track of how many rounds have been played.
/// </summary>
public sealed class RoundCounter : Component,
	IGameEventHandler<RoundCounterResetEvent>,
	IGameEventHandler<RoundCounterIncrementedEvent>
{
	/// <summary>
	/// Current round number, starting at 1.
	/// </summary>
	[HostSync, Change( nameof(OnRoundChanged) )]
	public int Round { get; set; } = 1;

	[Early]
	void IGameEventHandler<RoundCounterResetEvent>.OnGameEvent( RoundCounterResetEvent eventArgs )
	{
		Round = 1;
	}

	[Early]
	void IGameEventHandler<RoundCounterIncrementedEvent>.OnGameEvent( RoundCounterIncrementedEvent eventArgs )
	{
		Round += 1;
	}

	private void OnRoundChanged( int oldValue, int newValue )
	{
		Log.Info( $"### Round {newValue}" );

		GameUtils.LogPlayers();
	}
}

/// <summary>
/// Resets <see cref="RoundCounter"/> when this state is entered.
/// </summary>
public sealed class ResetRoundCounter : Component,
	IGameEventHandler<EnterStateEvent>
{
	public void OnGameEvent( EnterStateEvent eventArgs )
	{
		Scene.Dispatch( new RoundCounterResetEvent() );
	}
}

/// <summary>
/// Increments <see cref="RoundCounter"/> when this state is entered.
/// </summary>
public sealed class IncrementRoundCounter : Component,
	IGameEventHandler<EnterStateEvent>
{
	public void OnGameEvent( EnterStateEvent eventArgs )
	{
		Scene.Dispatch( new RoundCounterIncrementedEvent() );
	}
}
