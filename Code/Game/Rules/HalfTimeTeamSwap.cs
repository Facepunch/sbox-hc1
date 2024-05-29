using Facepunch;

public class HalfTimeTeamSwap : Component, IRoundEndListener
{
	[RequireComponent] public RoundCounter RoundCounter { get; private set; }
	[RequireComponent] public RoundLimit RoundLimit { get; private set; }

	void IRoundEndListener.PostRoundEnd()
	{
		if ( RoundCounter.Round != RoundLimit.MaxRounds / 2 )
		{
			return;
		}

		var ts = GameUtils.GetPlayers( Team.Terrorist ).ToArray();
		var cts = GameUtils.GetPlayers( Team.CounterTerrorist ).ToArray();

		foreach ( var player in ts )
		{
			player.AssignTeam( Team.CounterTerrorist );
		}

		foreach ( var player in cts )
		{
			player.AssignTeam( Team.Terrorist );
		}

		foreach ( var listener in Scene.GetAllComponents<ITeamSwapListener>() )
		{
			listener.OnTeamSwap();
		}
	}
}
