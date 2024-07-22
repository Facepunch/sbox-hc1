namespace Facepunch;

/// <summary>
/// Controlls the spotting behaviour of a player.
/// </summary>
public sealed class Spotter : Component
{
	public static float Interval = 0.5f;

	[Property] PlayerPawn Player { get; set; }

	private TimeSince LastPoll;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !Networking.IsHost )
			return;

		if ( Player.HealthComponent.State != LifeState.Alive || Player.Team == Team.Unassigned )
			return;

		if ( LastPoll < Interval )
			return;

		foreach ( var spottable in Scene.GetAllComponents<Spottable>() )
		{
			if ( spottable.GameObject == this.GameObject )
				continue;

			if ( spottable.Team == Player.Team )
				continue;

			Poll( spottable );
		}

		LastPoll = 0;
	}

	private void Poll( Spottable spottable )
	{
		var playerEyePos = Player.AimRay.Position;

		const float FOV = 85;
		float angle = Vector3.GetAngle( Player.EyeAngles.Forward, spottable.Transform.Position - playerEyePos );
		if ( MathF.Abs( angle ) > (FOV / 2) )
		{
			return;
		}

		// Try the top
		var trace = Scene.Trace.Ray( playerEyePos, spottable.Transform.Position + Vector3.Up * spottable.Height ) // bit of error for funsies
				.IgnoreGameObjectHierarchy( spottable.GameObject )
				.IgnoreGameObjectHierarchy( Player.GameObject )
				.UseHitboxes()
				.WithoutTags( "trigger", "ragdoll", "movement", "player" )
				.Run();

		if ( trace.Hit )
		{
			// Try the bottom
			trace = Scene.Trace.Ray( playerEyePos, spottable.Transform.Position )
				.IgnoreGameObjectHierarchy( spottable.GameObject )
				.IgnoreGameObjectHierarchy( Player.GameObject )
				.UseHitboxes()
				.WithoutTags( "trigger", "ragdoll", "movement", "player" )
				.Run();

			if ( trace.Hit )
				return;
		}

		spottable.Spotted(this);
	}
}
