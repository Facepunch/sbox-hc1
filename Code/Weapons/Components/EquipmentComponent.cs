namespace Facepunch;

/// <summary>
/// A weapon component. This can be anything that controls a weapon. Aiming, recoil, sway, shooting..
/// </summary>
[Icon( "track_changes" )]
public abstract class EquipmentComponent : Component, IEquipmentEvents
{
	/// <summary>
	/// The weapon.
	/// </summary>
	protected Equipment Equipment { get; set; }

	/// <summary>
	/// The player.
	/// </summary>
	protected PlayerPawn Player => Equipment.Owner;

	protected void BindTag( string tag, Func<bool> predicate ) => Equipment.BindTag( tag, predicate );

	protected void SetTagFor( string tag, float time )
	{
		Equipment.Tags.Set( tag, true );

		Invoke( time, () =>
		{
			Equipment.Tags.Set( tag, false );
		} );
	}

	protected override void OnAwake()
	{
		// Cache the weapon on awake
		Equipment = GetComponentInParent<Equipment>();

		base.OnAwake();
	}

	protected virtual void OnEquipmentDeployed()
	{
		//
	}

	protected virtual void OnEquipmentHolstered()
	{
		//
	}

	protected virtual void OnEquipmentDestroyed()
	{
		//
	}

	protected virtual void OnEquipmentTagChanged( string tag, bool value )
	{
		//
	}

	void IEquipmentEvents.OnTagChanged( Equipment e, string tag, bool value )
	{
		OnEquipmentTagChanged( tag, value );
	}

	void IEquipmentEvents.OnDestroyed( Equipment e )
	{
		OnEquipmentDestroyed();
	}

	void IEquipmentEvents.OnHolstered( Equipment e )
	{
		OnEquipmentHolstered();
	}

	void IEquipmentEvents.OnDeployed( Equipment e )
	{
		OnEquipmentDeployed();
	}
}

/// <summary>
/// Show this property in the equipment resource editor.
/// </summary>
[AttributeUsage( AttributeTargets.Property )]
public sealed class EquipmentResourcePropertyAttribute : Attribute
{

}
