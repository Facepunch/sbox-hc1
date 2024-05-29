
using Facepunch;
using Facepunch.UI;

public sealed class BombDefusalScenario : Component,
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

	void IBombPlantedListener.OnBombPlanted( PlayerController planter, GameObject bomb, BombSite bombSite )
	{
		IsBombPlanted = true;
		BombHasDetonated = false;
		BombWasDefused = false;

		StartAfterPlant();
	}

	[Broadcast( NetPermission.HostOnly )]
	private void StartAfterPlant()
	{
		RoundTimeLimit.Enabled = false;
		TeamEliminated.IgnoreTeam = Team.Terrorist;
	}

	[Broadcast( NetPermission.HostOnly )]
	private void PostRoundCleanup()
	{
		RoundTimeLimit.Enabled = true;
		TeamEliminated.IgnoreTeam = Team.Unassigned;
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

		PostRoundCleanup();
	}

	protected override void OnUpdate()
	{
		if ( GameMode.Instance.State != GameState.DuringRound ) return;
		if ( GameUtils.GetHudPanel<RoundStateDisplay>() is not { } display ) return;

		var team = GameUtils.LocalPlayer?.TeamComponent.Team ?? Team.Unassigned;

		if ( IsBombPlanted )
		{
			display.Status = team switch
			{
				Team.Terrorist => "Defend",
				Team.CounterTerrorist => "Defuse the Bomb",
				_ => null
			};
			display.Time = null;
		}
		else
		{
			display.Status = team switch
			{
				Team.Terrorist => "Plant the Bomb",
				Team.CounterTerrorist => "Defend",
				_ => null
			};
		}
	}

	public bool ShouldRoundEnd()
	{
		return IsBombPlanted && (BombWasDefused || BombHasDetonated);
	}
}
