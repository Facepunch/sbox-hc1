namespace Facepunch;

public sealed class KillSound : Component, IKillListener, IRoundStartListener
{
	[Property] public SoundEvent KillSoundEvent { get; set; }
	[Property] public float BaseSoundPitch { get; set; } = 0.7f;
	[Property] public float SoundPitchPerCount { get; set; } = 0.1f;
	[Property] public int MaxCount { get; set; } = 5;

	int count = 0;

	void IRoundStartListener.PostRoundStart()
	{
		count = 0;
	}

	void AddCount()
	{
		count++;
		if ( count >= MaxCount ) count = 0;
	}

	void IKillListener.OnPlayerKilled( DamageEvent damageEvent )
	{
		if ( damageEvent.Attacker == GameUtils.Viewer )
		{
			var snd = Sound.Play( KillSoundEvent );
			snd.Pitch = BaseSoundPitch + (count * SoundPitchPerCount);
			AddCount();
		}
	}
}
