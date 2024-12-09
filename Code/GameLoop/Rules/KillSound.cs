using Sandbox.Events;

namespace Facepunch;

public sealed class KillSound : Component,
	IGameEventHandler<KillEvent>,
	IGameEventHandler<RoundCounterIncrementedEvent>
{
	[Property] public SoundEvent KillSoundEvent { get; set; }
	[Property] public float BaseSoundPitch { get; set; } = 0.7f;
	[Property] public float SoundPitchPerCount { get; set; } = 0.1f;
	[Property] public int MaxCount { get; set; } = 5;

	int count = 0;

	void IGameEventHandler<RoundCounterIncrementedEvent>.OnGameEvent( RoundCounterIncrementedEvent eventArgs )
	{
		ResetCount();
	}

	[Rpc.Owner]
	private void ResetCount()
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
		var attacker = GameUtils.GetPlayerFromComponent( eventArgs.DamageInfo.Attacker );
		var victim = GameUtils.GetPlayerFromComponent( eventArgs.DamageInfo.Victim );

		if ( attacker != PlayerState.Viewer.Pawn || !attacker.IsValid() || !victim.IsValid() )
			return;

		if ( attacker.IsFriendly( victim ) )
			return;

		var snd = Sound.Play( KillSoundEvent );
		snd.Pitch = BaseSoundPitch + (count * SoundPitchPerCount);
		AddCount();
	}
}
