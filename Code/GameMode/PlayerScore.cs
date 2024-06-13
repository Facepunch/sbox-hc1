using Sandbox.Events;

namespace Facepunch.UI;

/// <summary>
/// Handles all the player score values.
/// </summary>
public sealed class PlayerScore : Component,
	IGameEventHandler<KillEvent>,
	IGameEventHandler<BombDefusedEvent>,
	IGameEventHandler<BombDetonatedEvent>,
	IGameEventHandler<BombPlantedEvent>
{
	[HostSync, Property, ReadOnly] 
	public int Kills { get; set; } = 0;

	[HostSync, Property, ReadOnly] 
	public int Deaths { get; set; } = 0;

	[HostSync, Property, ReadOnly] 
	public int Score { get; set; } = 0;

	private const int KillScore = 2;
	private const int AssistScore = 1;
	private const int TeamKillScore = -1;
	private const int SuicideScore = -1;

	// Planting the C4 explosive
	private const int PlantScore = 2;

	// Bomb planter alive when the bomb explodes
	private const int BombExplodePlanterAliveScore = 2;

	// Bomb planter dead when the bomb explodes
	private const int BombExplodePlanterDeadScore = 1;

	// Other Ts alive when the bomb explodes
	private const int BombExplodeTeamAliveScore = 1;

	// Defusing bomb
	private const int DefuserScore = 2;

	// Other CTs alive when the bomb is defused
	private const int DefuseTeamAliveScore = 1;

	void IGameEventHandler<KillEvent>.OnGameEvent( KillEvent eventArgs )
	{
		var damageInfo = eventArgs.DamageInfo;

		if ( !damageInfo.Attacker.IsValid() ) return;
		if ( !damageInfo.Victim.IsValid() ) return;

		var thisPlayer = GameUtils.GetPlayerFromComponent( this );

		var killerPlayer = GameUtils.GetPlayerFromComponent( damageInfo.Attacker );
		var victimPlayer = GameUtils.GetPlayerFromComponent( damageInfo.Victim );

		if ( !victimPlayer.IsValid() ) return;

		bool isFriendly = killerPlayer.TeamComponent.Team == victimPlayer.TeamComponent.Team;
		bool isSuicide = killerPlayer == victimPlayer;

		if ( killerPlayer == thisPlayer )
		{
			if ( isFriendly )
			{
				// Killed by friendly/teammate
				Kills--;
				Score += TeamKillScore;
			}
			else if ( isSuicide )
			{
				// Killed by suicide
				Kills--;
				Score += SuicideScore;
			}
			else
			{
				// Valid kill, add score
				Kills++;
				Score += KillScore;
			}
		}
		else if ( victimPlayer == thisPlayer )
		{
			// Only count as death if this wasn't a team kill
			if ( !isFriendly )
			{
				Deaths++;
			}
		}
	}

	void IGameEventHandler<BombPlantedEvent>.OnGameEvent( BombPlantedEvent eventArgs )
	{
		var thisPlayer = GameUtils.GetPlayerFromComponent( this );
		var planterPlayer = eventArgs.Planter;

		if ( planterPlayer == thisPlayer )
		{
			// Planter is the current player
			Score += PlantScore;
		}
	}

	void IGameEventHandler<BombDefusedEvent>.OnGameEvent( BombDefusedEvent eventArgs )
	{
		var thisPlayer = GameUtils.GetPlayerFromComponent( this );
		var defuserPlayer = eventArgs.Defuser;

		if ( defuserPlayer == thisPlayer )
		{
			// Defuser is the current player
			Score += DefuserScore;
		}
		else
		{
			// Defuser is a teammate
			if ( defuserPlayer.TeamComponent.Team == thisPlayer.TeamComponent.Team && thisPlayer.HealthComponent.State == LifeState.Alive )
			{
				Score += DefuseTeamAliveScore;
			}
		}
	}

	void IGameEventHandler<BombDetonatedEvent>.OnGameEvent( BombDetonatedEvent eventArgs )
	{
		var thisPlayer = GameUtils.GetPlayerFromComponent( this );

		if ( GameMode.Instance?.Get<BombDefusalScenario>() is not { } scenario )
			return;

		var planterPlayer = scenario.BombPlanter;
		var planterPlayerComponent = GameUtils.GetPlayerFromComponent( planterPlayer );

		if ( planterPlayerComponent == thisPlayer )
		{
			if ( planterPlayer.HealthComponent.State == LifeState.Alive )
			{
				// Planter is alive when the bomb explodes
				Score += BombExplodePlanterAliveScore;
			}
			else
			{
				// Planter is dead when the bomb explodes
				Score += BombExplodePlanterDeadScore;
			}
		}
		else if ( planterPlayerComponent.TeamComponent.Team == thisPlayer.TeamComponent.Team && thisPlayer.HealthComponent.State == LifeState.Alive )
		{
			// Teammate is alive when the bomb explodes
			Score += BombExplodeTeamAliveScore;
		}
	}
}
