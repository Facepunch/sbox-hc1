namespace Facepunch;

/// <summary>
/// End the game after a fixed number of rounds.
/// </summary>
public sealed class DefuseWinCondition : Component, IGameEndCondition, IGameEndListener
{
	[RequireComponent]
	public RoundLimit RoundLimit { get; private set; }

	[RequireComponent]
	public TeamScoring TeamScoring { get; private set; }

	/// <summary>
	/// Who's the winning team?
	/// </summary>
	[HostSync] public Team WinningTeam { get; set; }

	/// <summary>
	/// How many rounds does one team need to win?
	/// </summary>
	public int RoundsToWin => ( RoundLimit.MaxRounds / 2 ) + 1;

	public bool ShouldGameEnd()
	{
		if ( TeamScoring.RoundWinHistory.Count( x => x == Team.CounterTerrorist ) >= RoundsToWin )
		{
			WinningTeam = Team.CounterTerrorist;
			GameMode.Instance.ShowToast( "Operators Win!", Facepunch.UI.ToastType.CounterTerroristsWin );
			return true;
		}

		if ( TeamScoring.RoundWinHistory.Count( x => x == Team.Terrorist ) >= RoundsToWin )
		{
			WinningTeam = Team.Terrorist;
			GameMode.Instance.ShowToast( "Anarchists Win!", Facepunch.UI.ToastType.TerroristsWin );

			return true;
		}

		return false;
	}
}
