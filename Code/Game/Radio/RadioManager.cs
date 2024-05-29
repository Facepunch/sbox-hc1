namespace Facepunch;

/// <summary>
/// Handles certain events and plays radio sounds.
/// </summary>
public partial class RadioManager : Component, IRoundStartListener, IBombPlantedListener, IBombDefusedListener
{
	public static RadioManager Instance { get; private set; }

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
}
