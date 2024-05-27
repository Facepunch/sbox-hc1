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
	/// Shoulud we follow the current GameObject?
	/// </summary>
	[Property] public bool Follow { get; set; } = true;

	public void Play()
	{
		handle?.Stop( 0.0f );

		if ( SoundEvent == null ) return;

		handle = Sound.Play( SoundEvent, Transform.Position );
	}

	protected override void OnStart()
	{
		Play();
	}

	protected override void OnUpdate()
	{
		if ( handle is null ) return;

		// If we stopped playing, kill the game object
		if ( handle.IsStopped )
		{
			GameObject.Destroy();
		}
		// Otherwise, let's keep updating the position
		else if ( Follow )
		{
			handle.Position = GameObject.Transform.Position;
		}
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
		var gameObject = self.Scene.CreateObject();
		gameObject.Name = sndEvent.ResourceName;
		if ( follow ) gameObject.Parent = self;
		else gameObject.Transform.World = self.Transform.World;

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
