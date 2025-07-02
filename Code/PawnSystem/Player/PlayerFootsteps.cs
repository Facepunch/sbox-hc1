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

	public TimeSince TimeSinceStep { get; private set; }

	bool flipFlop = false;

	public float GetStepFrequency()
	{
		if ( Player.IsSprinting ) return 0.25f;
		return 0.35f;
	}

	private void Footstep()
	{
		// Don't make footsteps sometimes
		if ( Player.IsCrouching || Player.IsSlowWalking )
			return;

		var tr = Scene.Trace
			.Ray( Player.WorldPosition + Vector3.Up * 20, Player.WorldPosition + Vector3.Up * -20 )
			.IgnoreGameObjectHierarchy( Player.GameObject )
			.Run();

		if ( !tr.Hit )
			return;

		if ( tr.Surface is null )
			return;

		TimeSinceStep = 0;

		flipFlop = !flipFlop;

		var sound = flipFlop ? tr.Surface.SoundCollection.FootLeft : tr.Surface.SoundCollection.FootRight;
		if ( sound is null ) return;

		var handle = Sound.Play( sound, tr.HitPosition + tr.Normal * 5 );
		if ( !handle.IsValid() ) return;

		handle.Occlusion = false;
		handle.SpacialBlend = Player.IsViewer ? 0 : handle.SpacialBlend;
	}

	protected override void OnFixedUpdate()
	{
		if ( !Player.IsValid() )
			return;

		if ( Player.HealthComponent.State != LifeState.Alive ) return;

		if ( TimeSinceStep < GetStepFrequency() )
			return;

		if ( Player.CharacterController.Velocity.Length > 50f )
		{
			Footstep();
		}
	}
}
