using Sandbox.Events;

namespace Facepunch;

public sealed class BombDefusalScenario : Component,
	IGameEventHandler<ResetScoresEvent>,
	IGameEventHandler<BombDroppedEvent>,
	IGameEventHandler<BombPickedUpEvent>,
	IGameEventHandler<BombPlantedEvent>,
	IGameEventHandler<BombDetonatedEvent>,
	IGameEventHandler<BombDefusedEvent>,
	IGameEventHandler<TeamAssignedEvent>,
	IGameEventHandler<TeamsSwappedEvent>
{
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

	void IGameEventHandler<ResetScoresEvent>.OnGameEvent( ResetScoresEvent eventArgs )
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

	void IGameEventHandler<BombDroppedEvent>.OnGameEvent( BombDroppedEvent eventArgs )
	{
		//if ( GameMode.Instance.State == GameState.DuringRound )
		//{
		//	GameMode.Instance.ShowStatusText( Team.Terrorist, "Recover the Bomb" );
		//}
	}

	void IGameEventHandler<BombPickedUpEvent>.OnGameEvent( BombPickedUpEvent eventArgs )
	{
		//if ( GameMode.Instance.State == GameState.DuringRound )
		//{
		//	GameMode.Instance.ShowStatusText( Team.Terrorist, "Plant the Bomb" );
		//}
	}

	void IGameEventHandler<BombPlantedEvent>.OnGameEvent( BombPlantedEvent eventArgs )
	{
		BombPlanter = eventArgs.Planter;

		eventArgs.Planter?.Inventory.GiveCash( BombPlantedPlayerBonus );
	}

	void IGameEventHandler<BombDetonatedEvent>.OnGameEvent( BombDetonatedEvent eventArgs )
	{

	}

	void IGameEventHandler<BombDefusedEvent>.OnGameEvent( BombDefusedEvent eventArgs )
	{
		eventArgs.Defuser?.Inventory.GiveCash( BombDefusedPlayerBonus );
	}
}
