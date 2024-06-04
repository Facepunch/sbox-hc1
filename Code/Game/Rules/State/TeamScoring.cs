using Facepunch;

public sealed class TeamScoring : Component, IGameStartListener
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
}
