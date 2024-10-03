namespace Facepunch;

public partial class DroneSounds : Component
{
	[Property] public Drone Drone { get; set; }
	[Property] public SoundEvent DroneSound { get; set; }

	SoundHandle snd;

	void Play()
	{
		// Shitty loop
		if ( !snd.IsValid() || snd.Finished )
			snd = Sound.Play( DroneSound );

		snd.Position = WorldPosition;
		var vel = Drone.Rigidbody.Velocity.Length.LerpInverse( 0, 300, false );
		var angVel = Drone.Rigidbody.AngularVelocity.Length.LerpInverse( 0, 1.5f, false );

		var added = MathF.Max( vel, angVel );

		snd.Volume = 0.2f;
		snd.Pitch = 0.5f + added;
	}

	protected override void OnUpdate()
	{
		Play();
	}
}
