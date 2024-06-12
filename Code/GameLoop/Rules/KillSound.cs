using Sandbox.Events;

namespace Facepunch;

public sealed class KillSound : Component, IGameEventHandler<KillEvent>,
	IGameEventHandler<PostRoundStartEvent>
{
	[Property] public SoundEvent KillSoundEvent { get; set; }
	[Property] public float BaseSoundPitch { get; set; } = 0.7f;
	[Property] public float SoundPitchPerCount { get; set; } = 0.1f;
	[Property] public int MaxCount { get; set; } = 5;

	int count = 0;

	void IGameEventHandler<PostRoundStartEvent>.OnGameEvent( PostRoundStartEvent eventArgs )
	{
		count = 0;
	}

	void AddCount()
	{
		count++;
		if ( count >= MaxCount ) count = 0;
	}

	void IGameEventHandler<KillEvent>.OnGameEvent( KillEvent eventArgs )
	{
		if ( eventArgs.DamageInfo.Attacker == GameUtils.Viewer )
		{
			var snd = Sound.Play( KillSoundEvent );
			snd.Pitch = BaseSoundPitch + (count * SoundPitchPerCount);
			AddCount();
		}
	}
}
