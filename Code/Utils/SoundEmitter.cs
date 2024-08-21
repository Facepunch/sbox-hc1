namespace Facepunch;

/// <summary>
/// A simple component that plays a sound.
/// </summary>
public sealed class SoundEmitter : Component
{
	SoundHandle handle;

	/// <summary>
	/// How long until we destroy the GameObject.
	/// </summary>
	[Property] public SoundEvent SoundEvent { get; set; }

	/// <summary>
	/// Should we follow the current GameObject?
	/// </summary>
	[Property] public bool Follow { get; set; } = true;

	/// <summary>
	/// Should the GameObject be destroyed when the sound has finished?
	/// </summary>
	[Property] public bool DestroyOnFinish { get; set; } = true;
	
	[Property, ToggleGroup( "VolumeModifier", Label = "Volume Modifier" )]
	public bool VolumeModifier { get; set; } = false;
	
	[Property, ToggleGroup( "VolumeModifier" )]
	public Curve VolumeOverTime { get; set; } = new( new Curve.Frame( 0f, 1f ), new Curve.Frame( 1f, 1f ) );

	[Property, ToggleGroup( "VolumeModifier" )]
	public float LifeTime { get; set; } = 1f;

	private TimeSince TimeSincePlayed { get; set; }

	float initVolume = 1f;
	public void Play()
	{
		handle?.Stop();

		if ( SoundEvent == null ) return;
		TimeSincePlayed = 0f;
		handle = Sound.Play( SoundEvent, Transform.Position );

		initVolume = handle.Volume;
	}

	protected override void OnStart()
	{
		Play();
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
			handle.Position = GameObject.Transform.Position;
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
	/// <param name="follow"></param>
	public static void PlaySound( this GameObject self, SoundEvent sndEvent, bool follow = true )
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
		emitter.SoundEvent = sndEvent;
		emitter.Play();
	}

	/// <inheritdoc cref="PlaySound(GameObject, SoundEvent, bool)"/>
	public static void PlaySound( this GameObject self, string sndPath, bool follow = true )
	{
		if ( ResourceLibrary.TryGet<SoundEvent>( sndPath, out var sndEvent ) )
		{
			self.PlaySound( sndEvent, follow );
		}
	}
}
