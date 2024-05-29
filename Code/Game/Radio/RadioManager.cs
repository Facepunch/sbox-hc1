namespace Facepunch;

/// <summary>
/// Handles certain events and plays radio sounds.
/// </summary>
public partial class RadioManager : Component, IRoundStartListener
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
}
