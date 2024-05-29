namespace Facepunch.UI;

public sealed class PlayerScore : Component, IKillListener
{
	[HostSync( Query = true ), Property, ReadOnly] 
	public int Kills { get; set; } = 0;

	[HostSync( Query = true ), Property, ReadOnly] 
	public int Deaths { get; set; } = 0;

	[HostSync( Query = true ), Property, ReadOnly] 
	public int Experience { get; set; } = 0;

	public void OnPlayerKilled( Component killer, Component victim, float damage, Vector3 position, Vector3 force, Component inflictor = null )
	{
		Log.Info( $"{killer} killed {victim} with {inflictor} for {damage} damage at {position} with {force} force" );

		var thisPlayer = GameUtils.GetPlayerFromComponent( this );
		var killerPlayer = GameUtils.GetPlayerFromComponent( killer );
		var victimPlayer = GameUtils.GetPlayerFromComponent( victim );

		if ( killerPlayer == thisPlayer )
		{
			Kills++;
			Experience += 100;
		}
		else if ( victimPlayer == thisPlayer )
		{
			Deaths++;
		}
	}
}
