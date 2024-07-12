namespace Facepunch;

/// <summary>
/// Produces footstep sounds for the player.
/// </summary>
public sealed class PlayerFootsteps : Component
{
	[Property] public PlayerPawn Player { get; set; }
	[Property] SkinnedModelRenderer Source { get; set; }
	[Property] public float FootstepBaseDecibels { get; set; } = 70f;
	[Property] public float FootstepScale { get; set; } = 1f;
	[Property] public float SprintFootstepScale { get; set; } = 2f;

	protected override void OnEnabled()
	{
		if ( Source is null )
			return;

		Source.OnFootstepEvent += OnEvent;
	}

	protected override void OnDisabled()
	{
		if ( Source is null )
			return;

		Source.OnFootstepEvent -= OnEvent;
	}

	TimeSince timeSinceStep;

	private void OnEvent( SceneModel.FootstepEvent e )
	{
		if ( timeSinceStep < 0.2f )
			return;

		if ( Player.CharacterController.Velocity.Length < 20f ) 
			return;

		// Don't make footsteps sometimes
		if ( Player.IsCrouching || Player.IsSlowWalking ) 
			return;

		var tr = Scene.Trace
			.Ray( e.Transform.Position + Vector3.Up * 20, e.Transform.Position + Vector3.Up * -20 )
			.Run();

		if ( !tr.Hit )
			return;

		if ( tr.Surface is null )
			return;

		timeSinceStep = 0;

		var sound = e.FootId == 0 ? tr.Surface.Sounds.FootLeft : tr.Surface.Sounds.FootRight;
		if ( sound is null ) return;

		var scale = ( Player?.IsSprinting ?? false ) ? SprintFootstepScale : FootstepScale;
		var handle = Sound.Play( sound, tr.HitPosition + tr.Normal * 5 );
		if ( !handle.IsValid() ) return;
		
		handle.Volume *= e.Volume;
		handle.Occlusion = false;
		handle.Decibels = FootstepBaseDecibels * scale;
		handle.ListenLocal = Player.IsViewer;
	}
}
