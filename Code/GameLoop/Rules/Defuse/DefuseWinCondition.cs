namespace Facepunch;

/// <summary>
/// End the game after a fixed number of rounds.
/// </summary>
public sealed class DefuseWinCondition : Component, IGameEndCondition, IGameEndListener, IRoundStartListener
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

	void IRoundStartListener.PostRoundStart()
	{
		if ( GetWonRounds( Team.Terrorist ) == RoundsToMatchPoint || GetWonRounds( Team.CounterTerrorist ) == RoundsToMatchPoint )
		{
			Facepunch.UI.Toast.Instance.Show( "Match Point", Facepunch.UI.ToastType.Generic );
		}
	}

	public bool ShouldGameEnd()
	{
		if ( GetWonRounds( Team.CounterTerrorist ) >= RoundsToWin )
		{
			return true;
		}

		if ( GetWonRounds( Team.Terrorist ) >= RoundsToWin )
		{
			return true;
		}

		return false;
	}
}
