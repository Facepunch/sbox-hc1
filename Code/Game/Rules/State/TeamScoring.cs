using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Facepunch;

public sealed class TeamScoring : Component, IGameStartListener, IRoundStartListener, IRoundEndListener, ITeamSwapListener
{
	[Property, JsonIgnore] public int TerroristScore => GetTeamScore( Team.Terrorist );
	[Property, JsonIgnore] public int CounterTerroristScore => GetTeamScore( Team.CounterTerrorist );

	[HostSync] public Team RoundWinner { get; set; }

	[HostSync] public NetList<Team> RoundWinHistory { get; private set; } = new();

	public int GetTeamScore( Team team )
	{
		return RoundWinHistory.Count( x => x == team );
	}

	void IGameStartListener.PreGameStart()
	{
		RoundWinHistory.Clear();
	}

	void IRoundStartListener.PreRoundStart()
	{
		RoundWinner = Team.Unassigned;
	}

	async Task IRoundEndListener.OnRoundEnd()
	{
		RoundWinHistory.Add( RoundWinner );

		await Task.DelaySeconds( 1f );

		switch ( RoundWinner )
		{
			case Team.Terrorist:
				GameMode.Instance.ShowToast( "Anarchists Win!" );
				GameMode.Instance.ShowStatusText( "Anarchists Win!" );
				RadioSounds.Play( Team.CounterTerrorist, RadioSound.RoundLost );
				RadioSounds.Play( Team.Terrorist, RadioSound.RoundWon );
				break;

			case Team.CounterTerrorist:
				GameMode.Instance.ShowToast( "Operators Win!" );
				GameMode.Instance.ShowStatusText( "Operators Win!" );
				RadioSounds.Play( Team.CounterTerrorist, RadioSound.RoundWon );
				RadioSounds.Play( Team.Terrorist, RadioSound.RoundLost );
				break;
		}
	}

	void ITeamSwapListener.OnTeamSwap()
	{
		// Maybe there's a nicer way of handling this...

		for ( var i = 0; i < RoundWinHistory.Count; ++i )
		{
			RoundWinHistory[i] = RoundWinHistory[i].GetOpponents();
		}
	}
}
