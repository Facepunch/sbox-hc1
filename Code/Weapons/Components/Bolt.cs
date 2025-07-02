using Sandbox.Events;

namespace Facepunch;

[Title( "Bolt" ), Group( "Weapon Components" )]
public partial class Boltable : WeaponInputAction,
	IGameEventHandler<WeaponShotEvent>
{
	[Property, Group( "Effects" )] public GameObject EjectionPrefab { get; set; }

	[Property, Group( "Effects" )] public SoundEvent BoltReload { get; set; }

	/// <summary>
	/// Fetches the desired model renderer that we'll focus effects on like trail effects, muzzle flashes, etc.
	/// </summary>
	protected WeaponModel Effector
	{
		get
		{
			if ( IsProxy || !Equipment.ViewModel.IsValid() )
				return Equipment.WorldModel;

			return Equipment.ViewModel;
		}
	}

	private bool CanBolt = false;

	void IGameEventHandler<WeaponShotEvent>.OnGameEvent( WeaponShotEvent eventArgs )
	{
		CanBolt = true;
	}

	protected override void OnInputUp()
	{
		if ( !CanBolt ) return;

		Equipment.ViewModel?.ModelRenderer?.Set( "b_reload_bolt", true );

		GameObject.PlaySound( BoltReload );

		// Eject casing using GameObject / prefab
		if ( EjectionPrefab.IsValid() )
		{
			if ( Effector.EjectionPort.IsValid() )
			{
				var x = EjectionPrefab.Clone( new CloneConfig()
				{
					Parent = Effector.EjectionPort,
					Transform = new(),
					StartEnabled = true,
					Name = $"Bullet ejection: {Equipment.GameObject}"
				} );
			}
		}

		CanBolt = false;
	}
}
