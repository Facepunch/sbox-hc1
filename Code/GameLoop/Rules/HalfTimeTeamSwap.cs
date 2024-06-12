using Facepunch;
using Sandbox.Events;

public class HalfTimeTeamSwap : Component,
	IGameEventHandler<PreRoundStartEvent>,
	IGameEventHandler<PostRoundEndEvent>
{
	[RequireComponent] public RoundCounter RoundCounter { get; private set; }
	[RequireComponent] public RoundLimit RoundLimit { get; private set; }

	public int FirstHalfRoundCount => RoundLimit.MaxRounds / 2;

	void IGameEventHandler<PreRoundStartEvent>.OnGameEvent( PreRoundStartEvent eventArgs )
	{
		if ( RoundCounter.Round == FirstHalfRoundCount - 1 )
		{
			Facepunch.UI.Toast.Instance.Show( "Final round of the first half", Facepunch.UI.ToastType.Generic );
		}
	}

	void IGameEventHandler<PostRoundEndEvent>.OnGameEvent( PostRoundEndEvent eventArgs )
	{
		if ( RoundCounter.Round != FirstHalfRoundCount )
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
