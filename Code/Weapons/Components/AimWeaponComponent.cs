namespace Facepunch;

[Title( "ADS (Aim Down Sight)" ), Group( "Weapon Components" )]
public partial class AimWeaponComponent : InputWeaponComponent
{
	[Sync, Change( nameof( OnIsAimingChanged ))] public bool IsAiming { get; set; }

	protected void OnIsAimingChanged( bool before, bool after )
	{
		if ( !Equipment.IsValid() ) 
			return;

		if ( after )
		{
			Equipment.EquipmentFlags |= EquipmentFlags.Aiming;
		}
		else
		{
			Equipment.EquipmentFlags &= ~EquipmentFlags.Aiming;
		}
	}

	protected override void OnEnabled()
	{
		BindTag( "no_sprint", IsDown );
	}

	protected virtual bool CanAim()
	{
		// Self checks first
		if ( Tags.Has( "no_aiming" ) ) return false;
		// if ( Tags.Has( "reloading" ) ) return false;

		if ( !Player.IsValid() )
			return false;

		// Player controller
		if ( !Player.IsGrounded ) return false;
		if ( Player.IsSprinting ) return false;
		if ( Player.Tags.Has( "no_aiming" ) ) return false;

		return true;
	}

	protected override void OnUpdate()
	{
		if ( !Player.IsValid() )
			return;

		if ( !Player.IsLocallyControlled ) 
			return;

		if ( !CanAim() )
		{
			IsAiming = false;
			return;
		}

		IsAiming = IsDown();
	}
}
