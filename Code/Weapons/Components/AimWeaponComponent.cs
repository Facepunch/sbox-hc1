namespace Facepunch;

[Title( "ADS (Aim Down Sight)" ), Group( "Weapon Components" )]
public partial class AimWeaponComponent : InputWeaponComponent
{
	[Sync] public bool IsAiming { get; set; }

	protected override void OnEnabled()
	{
		BindTag( "aiming", () => IsAiming );
	}

	protected virtual bool CanAim()
	{
		// Self checks first
		if ( Tags.Has( "no_aiming" ) ) return false;
		if ( Tags.Has( "reloading" ) ) return false;

		// Player controller
		if ( !Player?.IsGrounded ?? false ) return false;
		if ( Player?.IsSprinting ?? false ) return false;
		if ( Player?.Tags.Has( "no_aiming" ) ?? false ) return false;

		return true;
	}

	protected override void OnUpdate()
	{
		if ( !CanAim() )
		{
			IsAiming = false;
			return;
		}

		IsAiming = IsDown();
	}
}
