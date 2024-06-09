namespace Facepunch.UI;

/// <summary>
/// Handles all the player score values.
/// </summary>
public sealed class PlayerScore : Component, IKillListener, IBombDefusedListener, IBombDetonatedListener, IBombPlantedListener
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

	public void OnPlayerKilled( DamageEvent damageEvent )
	{
		if ( !damageEvent.Attacker.IsValid() ) return;
		if ( !damageEvent.Victim.IsValid() ) return;

		var thisPlayer = GameUtils.GetPlayerFromComponent( this );
		var killerPlayer = GameUtils.GetPlayerFromComponent( damageEvent.Attacker );
		var victimPlayer = GameUtils.GetPlayerFromComponent( damageEvent.Victim );

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

	public void OnBombPlanted( PlayerController planter, GameObject bomb, BombSite bombSite )
	{
		var thisPlayer = GameUtils.GetPlayerFromComponent( this );
		var planterPlayer = GameUtils.GetPlayerFromComponent( planter );

		if ( planterPlayer == thisPlayer )
		{
			// Planter is the current player
			Score += PlantScore;
		}
	}

	public void OnBombDefused( PlayerController defuser, GameObject bomb, BombSite bombSite )
	{
		var thisPlayer = GameUtils.GetPlayerFromComponent( this );
		var defuserPlayer = GameUtils.GetPlayerFromComponent( defuser );

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

	public void OnBombDetonated( GameObject bomb, BombSite bombSite )
	{
		var thisPlayer = GameUtils.GetPlayerFromComponent( this );

		if ( GameMode.Instance?.Components.Get<BombDefusalScenario>() is not { } scenario )
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
