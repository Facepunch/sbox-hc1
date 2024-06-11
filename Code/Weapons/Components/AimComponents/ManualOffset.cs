namespace Facepunch;

public interface IViewModelOffset
{
	public Vector3 PositionOffset { get; }
	public Angles AngleOffset { get; }
}

[Title( "Aim Offset (Aim Component)" ), Group( "Weapon Components" )]
public partial class ManualOffset : AimWeaponComponent, IViewModelOffset
{
	[Property] public Vector3 AimOffset { get; set; }
	[Property] public Angles AimAngleOffset { get; set; }

	Vector3 IViewModelOffset.PositionOffset => IsAiming ? AimOffset : Vector3.Zero;
	Angles IViewModelOffset.AngleOffset => IsAiming ? AimAngleOffset : Angles.Zero;
}
