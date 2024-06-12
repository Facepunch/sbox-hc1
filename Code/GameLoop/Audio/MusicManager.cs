using Sandbox.Audio;
using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// Handles certain events and plays radio sounds.
/// </summary>
public partial class MusicManager : Component,
	IGameEventHandler<PostRoundStartEvent>,
	IGameEventHandler<BombPlantedEvent>,
	IGameEventHandler<BombDefusedEvent>
{
	public static MusicManager Instance { get; private set; }

	protected override void OnStart()
	{
		Instance = this;
	}

	[Broadcast]
	private void PlaySound( string snd )
	{
		var x = Sound.Play( snd );
		x.TargetMixer = Mixer.FindMixerByName( "Music" );
	}

	void IGameEventHandler<PostRoundStartEvent>.OnGameEvent( PostRoundStartEvent eventArgs )
	{
		PlaySound( "sounds/music/round_intro/round_start.sound" );
	}

	void IGameEventHandler<BombPlantedEvent>.OnGameEvent( BombPlantedEvent eventArgs )
	{

	}

	void IGameEventHandler<BombDefusedEvent>.OnGameEvent( BombDefusedEvent eventArgs )
	{

	}
}
