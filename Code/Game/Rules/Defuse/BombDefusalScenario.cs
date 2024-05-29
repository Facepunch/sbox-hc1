
using Facepunch;

public sealed class BombDefusalScenario : Component,
	IRoundStartListener,
	IBombPlantedListener,
	IBombDetonatedListener,
	IBombDefusedListener,
	IRoundEndListener,
	IRoundEndCondition
{
	[RequireComponent]
	public RoundTimeLimit RoundTimeLimit { get; private set; }
	[RequireComponent]
	public TeamEliminated TeamEliminated { get; private set; }

	[HostSync] public bool IsBombPlanted { get; private set; }
	[HostSync] public bool BombHasDetonated { get; private set; }
	[HostSync] public bool BombWasDefused { get; private set; }

	void IRoundStartListener.PostRoundStart()
	{
		GameMode.Instance.ShowStatusText( Team.Terrorist, "Plant the Bomb" );
		GameMode.Instance.ShowStatusText( Team.CounterTerrorist, "Defend" );
	}

	void IBombPlantedListener.OnBombPlanted( PlayerController planter, GameObject bomb, BombSite bombSite )
	{
		IsBombPlanted = true;
		BombHasDetonated = false;
		BombWasDefused = false;

		RoundTimeLimit.Enabled = false;
		TeamEliminated.IgnoreTeam = Team.Terrorist;

		GameMode.Instance.ShowStatusText( Team.Terrorist, "Defend" );
		GameMode.Instance.ShowStatusText( Team.CounterTerrorist, "Defuse the Bomb" );
		GameMode.Instance.HideTimer();
	}

	void IBombDetonatedListener.OnBombDetonated( GameObject bomb, BombSite bombSite )
	{
		BombHasDetonated = true;
	}

	void IBombDefusedListener.OnBombDefused( PlayerController defuser, GameObject bomb, BombSite bombSite )
	{
		BombWasDefused = true;
	}

	void IRoundEndListener.PreRoundEnd()
	{
		IsBombPlanted = false;

		RoundTimeLimit.Enabled = true;
		TeamEliminated.IgnoreTeam = Team.Unassigned;
	}

	public bool ShouldRoundEnd()
	{
		return IsBombPlanted && (BombWasDefused || BombHasDetonated);
	}
}
