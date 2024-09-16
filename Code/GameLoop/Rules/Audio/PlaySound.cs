using Sandbox.Events;
using Sandbox.Audio;

namespace Facepunch;

/// <summary>
/// Play a sound at the start of this state.
/// </summary>
public sealed class PlaySound : Component,
	IGameEventHandler<EnterStateEvent>
{
	[Property]
	public SoundEvent SoundEvent { get; set; }

	[Broadcast]
	private void Play()
	{
		if ( SoundEvent is null )
		{
			return;
		}

		var x = Sound.Play( SoundEvent );
		x.TargetMixer = Mixer.FindMixerByName( "Music" );
	}

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		Play();
	}
}

/// <summary>
/// Play a radio message at the start of this state.
/// </summary>
public sealed class PlayRadio : Component,
	IGameEventHandler<EnterStateEvent>
{
	[Property]
	public bool BothTeams { get; set; }

	[Property, HideIf( nameof(BothTeams), true )]
	public Team Team { get; set; } = Team.Terrorist;

	[Property]
	public RadioSound Sound { get; set; }

	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		if ( BothTeams )
		{
			RadioSounds.Play( Team.Terrorist, Sound );
			RadioSounds.Play( Team.CounterTerrorist, Sound );
		}
		else
		{
			RadioSounds.Play( Team, Sound );
		}
	}
}
