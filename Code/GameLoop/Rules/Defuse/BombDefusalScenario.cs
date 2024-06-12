using Facepunch;
using Sandbox.Events;

public sealed class BombDefusalScenario : Component,
	IGameEventHandler<PostGameStartEvent>,
	IGameEventHandler<PostRoundStartEvent>,
	IGameEventHandler<BombDroppedEvent>,
	IGameEventHandler<BombPickedUpEvent>,
	IGameEventHandler<BombPlantedEvent>,
	IGameEventHandler<BombDetonatedEvent>,
	IGameEventHandler<BombDefusedEvent>,
	IGameEventHandler<PreRoundEndEvent>,
	IGameEventHandler<DuringRoundEvent>,
	IGameEventHandler<TeamAssignedEvent>,
	IGameEventHandler<TeamsSwappedEvent>
{
	[RequireComponent] public RoundTimeLimit RoundTimeLimit { get; private set; }
	[RequireComponent] public TeamEliminated TeamEliminated { get; private set; }
	[RequireComponent] public RoundBasedTeamScoring Scoring { get; private set; }

	[Property, HostSync, Category( "Economy" )]
	public int StartMoney { get; set; } = 800;

	[Property, HostSync, Category( "Economy" )]
	public int DefaultWinTeamIncome { get; set; } = 3250;

	[Property, HostSync, Category( "Economy" )]
	public int BombDefusedTeamIncome { get; set; } = 3500;

	[Property, HostSync, Category( "Economy" )]
	public int BombDetonatedTeamIncome { get; set; } = 3500;

	/// <summary>
	/// Give the bomb planter this bonus.
	/// </summary>
	public int BombPlantedPlayerBonus { get; set; } = 300;

	/// <summary>
	/// Give the bomb defuser this bonus.
	/// </summary>
	public int BombDefusedPlayerBonus { get; set; } = 300;

	/// <summary>
	/// If the terrorists plant, but lose the round, give them this bonus.
	/// </summary>
	public int BombPlantedTeamBonus { get; set; } = 800;

	[Property, HostSync, Category( "Economy" )]
	public int BaseLossTeamIncome { get; set; } = 1400;

	/// <summary>
	/// How much each team's loss income increases per loss streak level.
	/// </summary>
	[Property, HostSync, Category( "Economy" )]
	public int LossBonusIncrement { get; set; } = 500;

	[Property, HostSync, Category( "Economy" )]
	public int MaxLossStreakLevel { get; set; } = 4;

	[HostSync]
	public NetDictionary<Team, int> LossStreakLevel { get; private set; } = new();

	[HostSync] public bool IsBombPlanted { get; private set; }
	[HostSync] public bool BombHasDetonated { get; private set; }
	[HostSync] public bool BombWasDefused { get; private set; }

	[HostSync] private Guid BombPlanterId { get; set; }

	public PlayerController BombPlanter
	{
		get => Scene.Directory.FindComponentByGuid( BombPlanterId ) as PlayerController;
		private set
		{
			BombPlanterId = value?.Id ?? Guid.Empty;
		}
	}

	private int GetLossStreakBonus( Team team )
	{
		if ( !LossStreakLevel.TryGetValue( team, out var level ) )
		{
			level = 0;
		}

		return BaseLossTeamIncome + level * LossBonusIncrement;
	}

	private void IncrementLossStreak( Team team, int sign )
	{
		LossStreakLevel[team] = Math.Clamp( LossStreakLevel.GetValueOrDefault( team ) + sign, 0, MaxLossStreakLevel );
	}

	void IGameEventHandler<PostGameStartEvent>.OnGameEvent( PostGameStartEvent eventArgs )
	{
		LossStreakLevel.Clear();
	}

	void IGameEventHandler<TeamAssignedEvent>.OnGameEvent( TeamAssignedEvent eventArgs )
	{
		eventArgs.Player.Inventory.Clear();
		eventArgs.Player.Inventory.SetCash( StartMoney );
	}

	void IGameEventHandler<TeamsSwappedEvent>.OnGameEvent( TeamsSwappedEvent eventArgs )
	{
		LossStreakLevel.Clear();
	}

	void IGameEventHandler<PostRoundStartEvent>.OnGameEvent( PostRoundStartEvent eventArgs )
	{
		GameMode.Instance.ShowStatusText( Team.Terrorist, "Plant the Bomb" );
		GameMode.Instance.ShowStatusText( Team.CounterTerrorist, "Defend" );
	}

	void IGameEventHandler<BombDroppedEvent>.OnGameEvent( BombDroppedEvent eventArgs )
	{
		if ( GameMode.Instance.State == GameState.DuringRound )
		{
			GameMode.Instance.ShowStatusText( Team.Terrorist, "Recover the Bomb" );
		}
	}

	void IGameEventHandler<BombPickedUpEvent>.OnGameEvent( BombPickedUpEvent eventArgs )
	{
		if ( GameMode.Instance.State == GameState.DuringRound )
		{
			GameMode.Instance.ShowStatusText( Team.Terrorist, "Plant the Bomb" );
		}
	}

	void IGameEventHandler<BombPlantedEvent>.OnGameEvent( BombPlantedEvent eventArgs )
	{
		IsBombPlanted = true;
		BombHasDetonated = false;
		BombWasDefused = false;

		BombPlanter = eventArgs.Planter;

		RoundTimeLimit.Enabled = false;
		TeamEliminated.IgnoreTeam = Team.Terrorist;

		eventArgs.Planter?.Inventory.GiveCash( BombPlantedPlayerBonus );

		GameMode.Instance.ShowToast( "Bomb has been planted" );

		GameMode.Instance.ShowStatusText( Team.Terrorist, "Defend the Bomb" );
		GameMode.Instance.ShowStatusText( Team.CounterTerrorist, "Defuse the Bomb" );
		GameMode.Instance.HideTimer();
	}

	void IGameEventHandler<BombDetonatedEvent>.OnGameEvent( BombDetonatedEvent eventArgs )
	{
		BombHasDetonated = true;
	}

	void IGameEventHandler<BombDefusedEvent>.OnGameEvent( BombDefusedEvent eventArgs )
	{
		BombWasDefused = true;

		eventArgs.Defuser?.Inventory.GiveCash( BombDefusedPlayerBonus );
	}

	void IGameEventHandler<PreRoundEndEvent>.OnGameEvent( PreRoundEndEvent eventArgs )
	{
		if ( Scoring.RoundWinner == Team.Terrorist )
		{
			GameUtils.GiveTeamIncome( Team.Terrorist, IsBombPlanted ? BombDetonatedTeamIncome : DefaultWinTeamIncome );
			GameUtils.GiveTeamIncome( Team.CounterTerrorist, GetLossStreakBonus( Team.CounterTerrorist ) );

			IncrementLossStreak( Team.Terrorist, -1 );
			IncrementLossStreak( Team.CounterTerrorist, 1 );
		}
		else if ( Scoring.RoundWinner == Team.CounterTerrorist )
		{
			GameUtils.GiveTeamIncome( Team.Terrorist, GetLossStreakBonus( Team.Terrorist ) + (BombWasDefused ? BombPlantedTeamBonus : 0) );
			GameUtils.GiveTeamIncome( Team.CounterTerrorist, BombWasDefused ? BombDefusedTeamIncome : DefaultWinTeamIncome );

			IncrementLossStreak( Team.Terrorist, 1 );
			IncrementLossStreak( Team.CounterTerrorist, -1 );
		}

		IsBombPlanted = false;
		BombPlanter = null;

		RoundTimeLimit.Enabled = true;
		TeamEliminated.IgnoreTeam = Team.Unassigned;
	}

	void IGameEventHandler<DuringRoundEvent>.OnGameEvent( DuringRoundEvent eventArgs )
	{
		if ( !IsBombPlanted )
		{
			return;
		}

		if ( BombWasDefused )
		{
			Scoring.RoundWinner = Team.CounterTerrorist;
			GameMode.Instance.EndRound();
		}
		else if ( BombHasDetonated )
		{
			Scoring.RoundWinner = Team.Terrorist;
			GameMode.Instance.EndRound();
		}
	}
}
