using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Handles certain events and plays radio sounds.
/// </summary>
public sealed class PlayRadioOnKill : Component,
	IGameEventHandler<KillEvent>
{
	[Property] public bool PlayEnemyLeftSounds { get; set; } = true;
	[Property] public bool PlayDeathSounds { get; set; } = true;

	private int GetAliveCount( Team team )
	{
		return GameUtils.GetPlayers( team ).Where( x => x.HealthComponent.State == LifeState.Alive ).Count();
	}

	void IGameEventHandler<KillEvent>.OnGameEvent( KillEvent eventArgs )
	{
		var damageInfo = eventArgs.DamageInfo;
		var victimTeam = damageInfo.Victim.GameObject.GetTeam();

		if ( PlayDeathSounds && GameUtils.GetPlayerFromComponent( damageInfo.Victim ) is { } player )
			RadioSounds.Play( victimTeam, RadioSound.TeammateDies );

		if ( !PlayEnemyLeftSounds )
			return;

		if ( damageInfo.Attacker.IsValid() )
		{
			if ( GetAliveCount( victimTeam ) == 2 )
			{
				RadioSounds.Play( victimTeam.GetOpponents(), RadioSound.TwoEnemiesLeft );
			}
			else if ( GetAliveCount( victimTeam ) == 1 )
			{
				RadioSounds.Play( victimTeam.GetOpponents(), RadioSound.OneEnemyLeft );
			}
		}
	}
}
