using System.Threading.Tasks;
using Facepunch;
using Facepunch.UI;

public sealed class TeamScoring : Component, IGameStartListener, IGameEndListener
{
	[HostSync]
	public NetDictionary<Team, int> Scores { get; private set; } = new();

	public int MyTeamScore => Scores.GetValueOrDefault( GameUtils.LocalPlayer?.GameObject.GetTeam() ?? Team.Unassigned );
	public int OpposingTeamScore => Scores.GetValueOrDefault( GameUtils.LocalPlayer?.GameObject.GetTeam().GetOpponents() ?? Team.Unassigned );

	void IGameStartListener.PostGameStart()
	{
		Scores.Clear();
	}

	public void IncrementScore( Team team, int amount = 1 )
	{
		Scores[team] = Scores.GetValueOrDefault( team ) + amount;
	}

	Task IGameEndListener.OnGameEnd()
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.IsFrozen = true;
		}

		if ( Scores.GetValueOrDefault( Team.CounterTerrorist ) > Scores.GetValueOrDefault( Team.Terrorist ) )
		{
			GameMode.Instance.ShowToast( "Operators Win!", ToastType.CounterTerroristsWin );
		}
		else if ( Scores.GetValueOrDefault( Team.Terrorist ) > Scores.GetValueOrDefault( Team.CounterTerrorist ) )
		{
			GameMode.Instance.ShowToast( "Anarchists Win!", Facepunch.UI.ToastType.TerroristsWin );
		}
		else
		{
			GameMode.Instance.ShowToast( "Scores are Tied!" );
		}

		return Task.CompletedTask;
	}
}
