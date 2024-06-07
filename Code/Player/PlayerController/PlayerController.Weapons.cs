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
