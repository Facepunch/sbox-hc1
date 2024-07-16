namespace Facepunch;

/// <summary>
/// Produces footstep sounds for the player.
/// </summary>
public sealed class PlayerFootsteps : Component
{
	[Property] public PlayerPawn Player { get; set; }
	[Property] public float FootstepBaseDecibels { get; set; } = 70f;
	[Property] public float FootstepScale { get; set; } = 1f;
	[Property] public float SprintFootstepScale { get; set; } = 2f;

	TimeSince timeSinceStep;

	bool flipFlop = false;

	private float GetStepFrequency()
	{
		if ( Player.IsSprinting ) return 0.25f;
		return 0.34f;
	}

	private void Footstep()
	{
		// Don't make footsteps sometimes
		if ( Player.IsCrouching || Player.IsSlowWalking )
			return;

		var tr = Scene.Trace
			.Ray( Player.Transform.Position + Vector3.Up * 20, Player.Transform.Position + Vector3.Up * -20 )
			.Run();

		if ( !tr.Hit )
			return;

		if ( tr.Surface is null )
			return;

		timeSinceStep = 0;

		flipFlop = !flipFlop;

		var sound = flipFlop ? tr.Surface.Sounds.FootLeft : tr.Surface.Sounds.FootRight;
		if ( sound is null ) return;

		var scale = (Player?.IsSprinting ?? false) ? SprintFootstepScale : FootstepScale;
		var handle = Sound.Play( sound, tr.HitPosition + tr.Normal * 5 );
		if ( !handle.IsValid() ) return;

		handle.Occlusion = false;
		handle.Decibels = FootstepBaseDecibels * scale;
		handle.ListenLocal = Player.IsViewer;
	}

	protected override void OnFixedUpdate()
	{
		if ( !Player.IsValid() ) 
			return;

		if ( timeSinceStep < GetStepFrequency() )
			return;

		if ( Player.CharacterController.Velocity.Length > 50f )
		{
			Footstep();
		}
	}
}
