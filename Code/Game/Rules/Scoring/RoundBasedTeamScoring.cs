using Facepunch;
using System.Threading.Tasks;

public sealed class RoundBasedTeamScoring : Component, IGameStartListener, IRoundStartListener, IRoundEndListener
{
	[RequireComponent] public TeamScoring TeamScoring { get; private set; }

	[HostSync] public Team RoundWinner { get; set; }

	[HostSync] public NetList<Team> RoundWinHistory { get; private set; } = new();

	void IGameStartListener.PreGameStart()
	{
		RoundWinHistory.Clear();
	}

	void IRoundStartListener.PreRoundStart()
	{
		RoundWinner = Team.Unassigned;
	}

	Task IRoundEndListener.OnRoundEnd()
	{
		RoundWinHistory.Add( RoundWinner );
		TeamScoring.IncrementScore( RoundWinner );

		GameMode.Instance.ShowStatusText( Team.Unassigned, "ROUND OVER" );

		switch ( RoundWinner )
		{
			case Team.Terrorist:
				GameMode.Instance.ShowToast( "Anarchists Win!", Facepunch.UI.ToastType.TerroristsWin );
				GameMode.Instance.ShowStatusText( Team.CounterTerrorist, "ROUND LOST" );
				GameMode.Instance.ShowStatusText( Team.Terrorist, "ROUND WON" );
				RadioSounds.Play( Team.CounterTerrorist, RadioSound.RoundLost );
				RadioSounds.Play( Team.Terrorist, RadioSound.RoundWon );
				break;

			case Team.CounterTerrorist:
				GameMode.Instance.ShowToast( "Operators Win!", Facepunch.UI.ToastType.CounterTerroristsWin );
				GameMode.Instance.ShowStatusText( Team.CounterTerrorist, "ROUND WON" );
				GameMode.Instance.ShowStatusText( Team.Terrorist, "ROUND LOST" );
				RadioSounds.Play( Team.CounterTerrorist, RadioSound.RoundWon );
				RadioSounds.Play( Team.Terrorist, RadioSound.RoundLost );
				break;
		}

		return Task.CompletedTask;
	}
}
