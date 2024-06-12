using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Handles certain events and plays radio sounds.
/// </summary>
public partial class RadioManager : Component,
	IGameEventHandler<PostRoundStartEvent>,
	IGameEventHandler<BombPlantedEvent>,
	IGameEventHandler<BombDefusedEvent>,
	IGameEventHandler<KillEvent>
{
	public static RadioManager Instance { get; private set; }

	[Property] public bool PlayEnemyLeftSounds { get; set; } = true;
	[Property] public bool PlayDeathSounds { get; set; } = true;

	protected override void OnStart()
	{
		Instance = this;
	}

	void IGameEventHandler<PostRoundStartEvent>.OnGameEvent( PostRoundStartEvent eventArgs )
	{
		RadioSounds.Play( Team.Terrorist, RadioSound.RoundStarted );
		RadioSounds.Play( Team.CounterTerrorist, RadioSound.RoundStarted );
	}

	void IGameEventHandler<BombPlantedEvent>.OnGameEvent( BombPlantedEvent eventArgs )
	{
		RadioSounds.Play( Team.Terrorist, RadioSound.BombPlanted );
		RadioSounds.Play( Team.CounterTerrorist, RadioSound.BombPlanted );
	}

	void IGameEventHandler<BombDefusedEvent>.OnGameEvent( BombDefusedEvent eventArgs )
	{
		RadioSounds.Play( Team.Terrorist, RadioSound.BombDefused );
		RadioSounds.Play( Team.CounterTerrorist, RadioSound.BombDefused );
	}

	private int GetAliveCount( Team team )
	{
		return GameUtils.GetPlayers( team ).Where( x => x.HealthComponent.State == LifeState.Alive ).Count();
	}

	void IGameEventHandler<KillEvent>.OnGameEvent( KillEvent eventArgs )
	{
		var damageInfo = eventArgs.DamageInfo;
		var victimTeam = damageInfo.Victim.GameObject.GetTeam();

		if ( PlayDeathSounds )
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
