namespace Facepunch;

/// <summary>
/// Handles certain events and plays radio sounds.
/// </summary>
public partial class RadioManager : Component, 
	IRoundStartListener, 
	IBombPlantedListener, 
	IBombDefusedListener,
	IKillListener
{
	public static RadioManager Instance { get; private set; }

	[Property] public bool PlayEnemyLeftSounds { get; set; } = true;
	[Property] public bool PlayDeathSounds { get; set; } = true;

	protected override void OnStart()
	{
		Instance = this;
	}

	void IRoundStartListener.PostRoundStart()
	{
		RadioSounds.Play( Team.Terrorist, RadioSound.RoundStarted );
		RadioSounds.Play( Team.CounterTerrorist, RadioSound.RoundStarted );
	}

	void IBombPlantedListener.OnBombPlanted( PlayerController planter, GameObject bomb, BombSite bombSite )
	{
		RadioSounds.Play( Team.Terrorist, RadioSound.BombPlanted );
		RadioSounds.Play( Team.CounterTerrorist, RadioSound.BombPlanted );
	}

	void IBombDefusedListener.OnBombDefused( PlayerController planter, GameObject bomb, BombSite bombSite )
	{
		RadioSounds.Play( Team.Terrorist, RadioSound.BombDefused );
		RadioSounds.Play( Team.CounterTerrorist, RadioSound.BombDefused );
	}

	private int GetAliveCount( Team team )
	{
		return GameUtils.GetPlayers( team ).Where( x => x.HealthComponent.State == LifeState.Alive ).Count();
	}

	void IKillListener.OnPlayerKilled( Component killer, Component victim, float damage, Vector3 position, Vector3 force, Component inflictor, string hitbox )
	{
		var victimTeam = victim.GameObject.GetTeam();

		if ( PlayDeathSounds )
			RadioSounds.Play( victimTeam, RadioSound.TeammateDies );

		if ( !PlayEnemyLeftSounds )
			return;

		if ( killer.IsValid() )
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
