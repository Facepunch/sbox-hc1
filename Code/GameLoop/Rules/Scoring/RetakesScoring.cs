
using Facepunch.UI;
using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Give each player a <see cref="RetakesScoring"/> component.
/// </summary>
public sealed class EnableRetakesScoring
	: GivePlayerStateComponent<RetakesScoring>
{

}

/// <summary>
/// Per-player win tracker for retakes.
/// </summary>
public sealed class RetakesScoring : Component, IScore,
	IGameEventHandler<ResetScoresEvent>
{
	/// <summary>
	/// How many rounds has this player been on the winning team.
	/// </summary>
	[Score( "Wins" ), Order( -2 ), Sync( SyncFlags.FromHost )] public int Wins { get; set; }

	void IGameEventHandler<ResetScoresEvent>.OnGameEvent( ResetScoresEvent eventArgs )
	{
		Wins = 0;
	}
}

/// <summary>
/// Give points to all players on the winning team.
/// </summary>
public sealed class IncreaseRetakesWins : Component,
	IGameEventHandler<EnterStateEvent>
{
	/// <summary>
	/// Which team won?
	/// </summary>
	[Property]
	public Team Team { get; set; }

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.GetPlayers( Team ) )
		{
			if ( player.GetComponentInChildren<RetakesScoring>( true ) is { } scoring )
			{
				scoring.Wins += 1;
			}
		}
	}
}
