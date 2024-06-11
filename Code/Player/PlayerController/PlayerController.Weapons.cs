namespace Facepunch;

public partial class PlayerController
{
	/// <summary>
	/// What weapon are we using?
	/// </summary>
	[Property, ReadOnly] public Weapon CurrentWeapon { get; private set; }

	/// <summary>
	/// A <see cref="GameObject"/> that will hold our ViewModel.
	/// </summary>
	[Property] public GameObject ViewModelGameObject { get; set; }

	/// <summary>
	/// How much spread should we add based on how fast the player is moving
	/// </summary>
	[Property, Group( "Spread" )] public float VelocitySpreadScale { get; set; } = 0.1f;
	[Property, Group( "Spread" )] public float AimSpreadScale { get; set; } = 0.5f;

	/// <summary>
	/// How inaccurate are things like gunshots?
	/// </summary>
	public float Spread { get; set; }

	private void UpdateRecoilAndSpread()
	{
		var recoil = CurrentWeapon?.Components.Get<RecoilWeaponComponent>( FindMode.EnabledInSelfAndDescendants );

		var spread = 0f;
		var velLimit = 350f;
		var velSpreadScale = VelocitySpreadScale;
		if ( CurrentWeapon?.Tags.Has( "aiming" ) ?? false ) velSpreadScale *= AimSpreadScale;
		var airSpreadMult = 2f;

		var velLen = CharacterController.Velocity.Length;
		spread += velLen.Remap( 0, velLimit, 0, 1, true ) * velSpreadScale;

		if ( recoil.IsValid() )
		{
			var recoilScale = 0.5f;
			if ( IsCrouching ) recoilScale *= 0.5f;

			spread += recoil.Current.AsVector3().Length * recoilScale;
		}

		if ( !IsGrounded ) spread *= airSpreadMult;

		Spread = spread;
	}

	void Weapon.IDeploymentListener.OnDeployed( Weapon weapon )
	{
		CurrentWeapon = weapon;
	}

	void Weapon.IDeploymentListener.OnHolstered( Weapon weapon )
	{
		if ( weapon == CurrentWeapon )
			CurrentWeapon = null;
	}

	[Authority]
	private void SetCurrentWeapon( Guid weaponId )
	{
		var weapon = Scene.Directory.FindComponentByGuid( weaponId ) as Weapon;
		SetCurrentWeapon( weapon );
	}

	[Authority]
	private void ClearCurrentWeapon()
	{
		CurrentWeapon?.Holster();
	}

	public void Holster()
	{
		if ( IsProxy )
		{
			if ( Networking.IsHost )
				ClearCurrentWeapon();

			return;
		}

		CurrentWeapon?.Holster();
	}

	public void SetCurrentWeapon( Weapon weapon )
	{
		if ( IsProxy )
		{
			if ( Networking.IsHost )
				SetCurrentWeapon( weapon.Id );

			return;
		}

		weapon.Deploy();
	}

	public void ClearViewModel()
	{
		foreach ( var weapon in Inventory.Weapons )
		{
			weapon.ClearViewModel();
		}
	}

	public void CreateViewModel( bool playDeployEffects = true )
	{
		if ( CameraController.Mode != CameraMode.FirstPerson )
			return;

		var weapon = CurrentWeapon;
		if ( weapon.IsValid() )
			weapon.CreateViewModel( playDeployEffects );
	}
}
