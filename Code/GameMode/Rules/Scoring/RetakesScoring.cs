
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
	/// How many rounds has this player successfully defended.
	/// </summary>
	[Score( "Wins" ), Order( -2 ), HostSync] public int Wins { get; set; }

	void IGameEventHandler<ResetScoresEvent>.OnGameEvent( ResetScoresEvent eventArgs )
	{
		Wins = 0;
	}
}

/// <summary>
/// Give points to all the defenders for winning the round.
/// </summary>
public sealed class IncreaseRetakesWins : Component,
	IGameEventHandler<EnterStateEvent>
{
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		Log.Info( $"Giving points!" );

		foreach ( var player in GameUtils.GetPlayers( Team.Terrorist ) )
		{
			Log.Info( $"Player: {player.DisplayName}" );

			if ( player.Components.Get<RetakesScoring>( FindMode.EverythingInChildren ) is { } scoring )
			{
				Log.Info( $"Found RetakesScoring: {scoring.Wins}" );
				scoring.Wins += 1;
			}
		}
	}
}
