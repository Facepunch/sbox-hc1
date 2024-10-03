namespace Sandbox.Audio;

/// <summary>
/// A simple component that plays a sound.
/// </summary>
public sealed class SoundEmitter : Component
{
	/// <summary>
	/// How long until we destroy the GameObject.
	/// </summary>
	[Property]
	public SoundEvent SoundEvent { get; set; }

	/// <summary>
	/// Should we follow the current GameObject?
	/// </summary>
	[Property]
	public bool Follow { get; set; } = true;

	/// <summary>
	/// Should the GameObject be destroyed when the sound has finished?
	/// </summary>
	[Property]
	public bool DestroyOnFinish { get; set; } = true;

	/// <summary>
	/// Which mixer should this sound play on?
	/// </summary>
	[Property]
	public string MixerName { get; set; } = "Game";

	/// <summary>
	/// Should we automatically play the sound when creating the sound emitter, or should we trigger it manually?
	/// </summary>
	[Property]
	public bool AutoStart { get; set; } = true;

	/// <summary>
	/// How much should we scale this sound's volume by?
	/// </summary>
	[Property, ToggleGroup( "VolumeModifier", Label = "Volume Modifier" )]
	public bool VolumeModifier { get; set; } = false;
	
	/// <summary>
	/// Should we change the volume over time? Good for fading in/out a sound.
	/// </summary>
	[Property, ToggleGroup( "VolumeModifier" )]
	public Curve VolumeOverTime { get; set; } = new( new Curve.Frame( 0f, 1f ), new Curve.Frame( 1f, 1f ) );

	/// <summary>
	/// How long should this sound last until being destroyed, since we're creating a GameObject per-sound, it's useful.
	/// </summary>
	[Property, ToggleGroup( "VolumeModifier" )]
	public float LifeTime { get; set; } = 1f;

	/// <summary>
	/// How long until we started playing the sound?
	/// </summary>
	private TimeSince TimeSincePlayed { get; set; }

	// Cache the initial volume so we can scale it.
	float initVolume = 1f;

	// Cache the SoundHandle so we can update its position per-frame.
	SoundHandle handle;

	/// <summary>
	/// Play the sound
	/// </summary>
	public void Play()
	{
		handle?.Stop();

		if ( SoundEvent == null ) return;
		TimeSincePlayed = 0f;
		handle = Sound.Play( SoundEvent, WorldPosition );
		handle.TargetMixer = Mixer.FindMixerByName( MixerName );

		initVolume = handle.Volume;
	}

	protected override void OnStart()
	{
		if ( AutoStart )
		{
			Play();
		}
	}

	protected override void OnUpdate()
	{
		if ( handle is null ) return;

		// If we stopped playing, kill the game object (maybe)
		if ( handle.IsStopped )
		{
			if ( DestroyOnFinish )
				GameObject.Destroy();
		}
		// Otherwise, let's keep updating the position
		else if ( Follow )
		{
			handle.Position = GameObject.WorldPosition;
		}

		if ( VolumeModifier )
		{
			handle.Volume = initVolume * VolumeOverTime.Evaluate( TimeSincePlayed / LifeTime );
		}
	}

	protected override void OnDestroy()
	{
		handle?.Stop();
		handle = null;
	}
}

public static partial class GameObjectExtensions
{
	/// <summary>
	/// Creates a GameObject that plays a sound.
	/// </summary>
	/// <param name="self"></param>
	/// <param name="sndEvent"></param>
	/// <param name="follow">Should this sound follow the GameObject?</param>
	/// <param name="mixerName">The audio mixer, look at the Mixer window in the Editor</param>
	public static void PlaySound( this GameObject self, SoundEvent sndEvent, bool follow = true, string mixerName = null )
	{
		if ( !self.IsValid() )
			return;

		if ( sndEvent is null )
			return;

		var gameObject = self.Scene.CreateObject();
		gameObject.Name = sndEvent.ResourceName;
		
		if ( follow )
			gameObject.Parent = self;
		else
			gameObject.Transform.World = self.Transform.World;

		var emitter = gameObject.Components.Create<SoundEmitter>();

		// Point to an emitter if we chose one
		if ( !string.IsNullOrEmpty( mixerName ) )
		{
			emitter.MixerName = mixerName;
		}

		emitter.SoundEvent = sndEvent;
		emitter.Play();
	}

	/// <inheritdoc cref="PlaySound(GameObject, SoundEvent, bool, string)"/>
	public static void PlaySound( this GameObject self, string sndPath, bool follow = true, string mixerName = null )
	{
		if ( ResourceLibrary.TryGet<SoundEvent>( sndPath, out var sndEvent ) )
		{
			self.PlaySound( sndEvent, follow, mixerName );
		}
	}
}
