namespace Facepunch.UI;

public sealed class PlayerScore : Component, IKillListener
{
	[HostSync, Property, ReadOnly] 
	public int Kills { get; set; } = 0;

	[HostSync, Property, ReadOnly] 
	public int Deaths { get; set; } = 0;

	[HostSync, Property, ReadOnly] 
	public int Experience { get; set; } = 0;

	private const int FriendlyKillScore = -200;
	private const int KillScore = 100;

	public void OnPlayerKilled( Component killer, Component victim, float damage, Vector3 position, Vector3 force, Component inflictor = null )
	{
		Log.Info( $"{killer} killed {victim} with {inflictor} for {damage} damage at {position} with {force} force" );

		var thisPlayer = GameUtils.GetPlayerFromComponent( this );
		var killerPlayer = GameUtils.GetPlayerFromComponent( killer );
		var victimPlayer = GameUtils.GetPlayerFromComponent( victim );

		bool isFriendly = killerPlayer.TeamComponent.Team == victimPlayer.TeamComponent.Team;
		bool isSuicide = killerPlayer == victimPlayer;

		if ( isFriendly || isSuicide )
		{
			if ( killerPlayer == thisPlayer )
			{
				Kills--;
				Experience += FriendlyKillScore;
			}
		}
		else
		{
			// Valid kill, add score
			if ( killerPlayer == thisPlayer )
			{
				Kills++;
				Experience += KillScore;
			}
			else if ( victimPlayer == thisPlayer )
			{
				Deaths++;
			}
		}
	}
}
