namespace Facepunch.UI;

public sealed class PlayerScore : Component, IKillListener
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

	public void OnPlayerKilled( Component killer, Component victim, float damage, Vector3 position, Vector3 force, Component inflictor = null )
	{
		Log.Info( $"{killer} killed {victim} with {inflictor} for {damage} damage at {position} with {force} force" );

		var thisPlayer = GameUtils.GetPlayerFromComponent( this );
		var killerPlayer = GameUtils.GetPlayerFromComponent( killer );
		var victimPlayer = GameUtils.GetPlayerFromComponent( victim );

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
}
