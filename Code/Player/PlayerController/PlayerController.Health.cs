using Facepunch.UI;

namespace Facepunch;

public partial class PlayerController
{
	/// <summary>
	/// Called when YOU inflict damage on something
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="position"></param>
	/// <param name="force"></param>
	/// <param name="target"></param>
	/// <param name="hitbox"></param>
	void IDamageListener.OnDamageGiven( float damage, Vector3 position, Vector3 force, Component target, string hitbox )
	{
		Log.Info( $"{this} damaged {target} for {damage}" );

		// Did we cause this damage?
		if ( this == GameUtils.Viewer )
		{
			Crosshair.Instance?.Trigger( damage, target, position, hitbox );
		}
	}

	/// <summary>
	/// Called when YOU take damage from something
	/// </summary>
	/// <param name="damage"></param>
	/// <param name="position"></param>
	/// <param name="force"></param>
	/// <param name="attacker"></param>
	/// <param name="hitbox"></param>
	void IDamageListener.OnDamageTaken( float damage, Vector3 position, Vector3 force, Component attacker, string hitbox )
	{
		Log.Info( $"{this} took {damage} damage!" );

		AnimationHelper.ProceduralHitReaction( damage / 100f, force );

		// Is this the local player?
		if ( IsViewer )
		{
			DamageIndicator.Current?.OnHit( position );
		}

		Body.DamageTakenPosition = position;
		Body.DamageTakenForce = force.Normal * damage * Game.Random.Float( 5f, 20f );

		// Headshot effects
		if ( hitbox.Contains( "head" ) )
		{
			var hasHelmet = HealthComponent.HasHelmet;

			// Non-local viewer
			if ( !IsViewer )
			{
				var go = hasHelmet ? HeadshotWithHelmetEffect?.Clone( position ) : HeadshotEffect?.Clone( position );

				if ( go.IsValid() )
				{
					// we did it
				}
			}

			var headshotSound = hasHelmet ? HeadshotWithHelmetSound : HeadshotSound;
			if ( headshotSound is not null )
			{
				Sound.Play( headshotSound, position );
			}
		}
		else
		{
			if ( BloodEffect.IsValid() )
			{
				BloodEffect?.Clone( new CloneConfig()
				{
					StartEnabled = true,
					Transform = new( position ),
					Name = $"Blood effect from ({GameObject})"
				} );
			}

			if ( BloodImpactSound is not null )
			{
				var snd = Sound.Play( BloodImpactSound, position );
				snd.ListenLocal = IsViewer;
			}
		}
	}
}
