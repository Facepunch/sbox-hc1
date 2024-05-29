
using Facepunch;
using Sandbox.Utility;

public sealed class TeamScoring : Component, IGameStartListener, IRoundStartListener, IRoundEndListener
{
	[Property, HostSync] public int TerroristScore { get; private set; }
	[Property, HostSync] public int CounterTerroristScore { get; private set; }
	[Property, HostSync] public Team RoundWinner { get; set; }

	void IGameStartListener.PreGameStart()
	{
		TerroristScore = 0;
		CounterTerroristScore = 0;
	}

	void IRoundStartListener.PreRoundStart()
	{
		RoundWinner = Team.Unassigned;
	}

	void IRoundEndListener.PreRoundEnd()
	{
		switch ( RoundWinner )
		{
			case Team.Terrorist:
				TerroristScore += 1;
				break;

			case Team.CounterTerrorist:
				CounterTerroristScore += 1;
				break;
		}
	}
}
