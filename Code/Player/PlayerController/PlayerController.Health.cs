using Facepunch.UI;

namespace Facepunch;

public partial class PlayerController
{
	/// <summary>
	/// Called when YOU inflict damage on something
	/// </summary>
	void IDamageListener.OnDamageGiven( DamageEvent damageEvent )
	{
		// Did we cause this damage?
		if ( IsViewer )
		{
			Crosshair.Instance?.Trigger( damageEvent );
		}
	}

	/// <summary>
	/// Called when YOU take damage from something
	/// </summary>
	void IDamageListener.OnDamageTaken( DamageEvent damageEvent )
	{
		AnimationHelper.ProceduralHitReaction( damageEvent.Damage / 100f, damageEvent.Force );

		if ( !damageEvent.Attacker.IsValid() ) 
			return;

		var position = damageEvent.Attacker?.Transform.Position ?? Vector3.Zero;

		// Is this the local player?
		if ( IsViewer )
		{
			DamageIndicator.Current?.OnHit( position );
		}

		Body.DamageTakenPosition = position;
		Body.DamageTakenForce = damageEvent.Force.Normal * damageEvent.Damage * Game.Random.Float( 5f, 20f );

		// Headshot effects
		if ( damageEvent.Hitboxes.Contains( "head" ) )
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
