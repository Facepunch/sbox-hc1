namespace Facepunch;

public partial class AimWeaponFunction : InputWeaponComponent
{
	[Sync] public bool IsAiming { get; set; }

	[Property] public Vector3 AimOffset { get; set; }
	[Property] public Angles AimAngles { get; set; }

	protected override void OnEnabled()
	{
		BindTag( "aiming", () => IsAiming );
	}

	protected virtual bool CanAim()
	{
		if ( !Weapon.PlayerController.IsGrounded ) return false;
		if ( Weapon.PlayerController.IsSprinting ) return false;
		if ( Tags.Has( "no_aiming" ) ) return false;
		if ( Tags.Has( "reloading" ) ) return false;
		if ( Weapon.PlayerController.Tags.Has( "no_aiming" ) ) return false;

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
