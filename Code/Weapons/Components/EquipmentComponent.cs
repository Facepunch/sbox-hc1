namespace Facepunch;

/// <summary>
/// A weapon component. This can be anything that controls a weapon. Aiming, recoil, sway, shooting..
/// </summary>
[Icon( "track_changes" )]
public abstract class EquipmentComponent : Component
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

	protected override void OnAwake()
	{
		// Cache the weapon on awake
		Equipment = Components.Get<Equipment>( FindMode.EverythingInSelfAndAncestors );

		base.OnAwake();
	}
}

/// <summary>
/// Show this property in the equipment resource editor.
/// </summary>
[AttributeUsage( AttributeTargets.Property )]
public sealed class EquipmentResourcePropertyAttribute : Attribute
{

}
