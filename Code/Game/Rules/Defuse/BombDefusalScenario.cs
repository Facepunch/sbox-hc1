
using Facepunch;

public sealed class BombDefusalScenario : Component,
	IGameStartListener,
	IRoundStartListener,
	IBombPlantedListener,
	IBombDetonatedListener,
	IBombDefusedListener,
	IRoundEndListener,
	IRoundEndCondition,
	ITeamAssignedListener
{
	[RequireComponent] public RoundTimeLimit RoundTimeLimit { get; private set; }
	[RequireComponent] public TeamEliminated TeamEliminated { get; private set; }
	[RequireComponent] public TeamScoring TeamScoring { get; private set; }

	[Property, HostSync, Category( "Economy" )]
	public int StartMoney { get; set; } = 800;

	[Property, HostSync, Category( "Economy" )]
	public int DefaultWinTeamIncome { get; set; } = 3250;

	[Property, HostSync, Category( "Economy" )]
	public int BombDefusedTeamIncome { get; set; } = 3500;

	[Property, HostSync, Category( "Economy" )]
	public int BombDetonatedTeamIncome { get; set; } = 3500;

	[Property, HostSync, Category( "Economy" )]
	public int BaseLossTeamIncome { get; set; } = 1400;

	[Property, HostSync, Category( "Economy" )]
	public int LossStreakBonus { get; set; } = 500;

	[Property, HostSync, Category( "Economy" )]
	public int MaxLossStreakBonus { get; set; } = 2000;

	private int GetLossStreakBonus( Team team )
	{
		var roundsLost = 2; // TODO

		return BaseLossTeamIncome + Math.Min( (roundsLost - 1) * LossStreakBonus, MaxLossStreakBonus );
	}

	[HostSync] public bool IsBombPlanted { get; private set; }
	[HostSync] public bool BombHasDetonated { get; private set; }
	[HostSync] public bool BombWasDefused { get; private set; }

	void ITeamAssignedListener.OnTeamAssigned( PlayerController player, Team team )
	{
		player.Inventory.SetCash( StartMoney );
	}

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
		if ( TeamScoring.RoundWinner == Team.Terrorist )
		{
			GameUtils.GiveTeamIncome( Team.Terrorist, IsBombPlanted ? BombDetonatedTeamIncome : DefaultWinTeamIncome );
			GameUtils.GiveTeamIncome( Team.CounterTerrorist, GetLossStreakBonus( Team.CounterTerrorist ) );
		}
		else if ( TeamScoring.RoundWinner == Team.CounterTerrorist )
		{
			GameUtils.GiveTeamIncome( Team.Terrorist, GetLossStreakBonus( Team.Terrorist ) );
			GameUtils.GiveTeamIncome( Team.CounterTerrorist, BombWasDefused ? BombDefusedTeamIncome : DefaultWinTeamIncome );
		}

		IsBombPlanted = false;

		RoundTimeLimit.Enabled = true;
		TeamEliminated.IgnoreTeam = Team.Unassigned;
	}

	public bool ShouldRoundEnd()
	{
		if ( !IsBombPlanted )
		{
			return false;
		}

		if ( BombWasDefused )
		{
			TeamScoring.RoundWinner = Team.CounterTerrorist;
			return true;
		}

		if ( BombHasDetonated )
		{
			TeamScoring.RoundWinner = Team.Terrorist;
			return true;
		}

		return false;
	}
}
