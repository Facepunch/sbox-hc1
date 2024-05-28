namespace Facepunch;

public partial class AimWeaponFunction : InputActionWeaponFunction
{
	public bool IsAiming { get; set; }

	[Property] public Vector3 AimOffset { get; set; }
	[Property] public Angles AimAngles { get; set; }

	protected override void OnEnabled()
	{
		BindTag( "aiming", () => IsAiming );
	}

	protected virtual bool CanAim()
	{
		if ( Tags.Has( "reloading" ) ) return false;

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
