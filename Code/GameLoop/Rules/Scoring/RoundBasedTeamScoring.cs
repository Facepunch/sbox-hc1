using Facepunch;
using Sandbox.Events;

public sealed class RoundBasedTeamScoring : Component,
	IGameEventHandler<PreGameStartEvent>,
	IGameEventHandler<PreRoundStartEvent>,
	IGameEventHandler<PreRoundEndEvent>
{
	[RequireComponent] public TeamScoring TeamScoring { get; private set; }

	[HostSync] public Team RoundWinner { get; set; }

	[HostSync] public NetList<Team> RoundWinHistory { get; private set; } = new();

	void IGameEventHandler<PreGameStartEvent>.OnGameEvent( PreGameStartEvent eventArgs )
	{
		RoundWinHistory.Clear();
	}

	void IGameEventHandler<PreRoundStartEvent>.OnGameEvent( PreRoundStartEvent eventArgs )
	{
		RoundWinner = Team.Unassigned;
	}

	void IGameEventHandler<PreRoundEndEvent>.OnGameEvent( PreRoundEndEvent eventArgs )
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
	}
}
