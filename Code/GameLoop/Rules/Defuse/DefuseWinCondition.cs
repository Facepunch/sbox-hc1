using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// End the game after a fixed number of rounds.
/// </summary>
public sealed class DefuseWinCondition : Component,
	IGameEventHandler<PostRoundStartEvent>,
	IGameEventHandler<PostRoundEndEvent>
{
	[RequireComponent]
	public RoundLimit RoundLimit { get; private set; }

	[RequireComponent]
	public TeamScoring TeamScoring { get; private set; }

	/// <summary>
	/// How many rounds does one team need to win?
	/// </summary>
	public int RoundsToWin => ( RoundLimit.MaxRounds / 2 ) + 1;
	public int RoundsToMatchPoint => ( RoundLimit.MaxRounds / 2 );

	private int GetWonRounds( Team team )
	{
		return TeamScoring.Scores.GetValueOrDefault( team );
	}

	void IGameEventHandler<PostRoundStartEvent>.OnGameEvent( PostRoundStartEvent eventArgs )
	{
		if ( GetWonRounds( Team.Terrorist ) == RoundsToMatchPoint || GetWonRounds( Team.CounterTerrorist ) == RoundsToMatchPoint )
		{
			Facepunch.UI.Toast.Instance.Show( "Match Point", Facepunch.UI.ToastType.Generic );
		}
	}

	void IGameEventHandler<PostRoundEndEvent>.OnGameEvent( PostRoundEndEvent eventArgs )
	{
		if ( GetWonRounds( Team.CounterTerrorist ) >= RoundsToWin )
		{
			GameMode.Instance.EndGame();
		}

		if ( GetWonRounds( Team.Terrorist ) >= RoundsToWin )
		{
			GameMode.Instance.EndGame();
		}
	}
}
