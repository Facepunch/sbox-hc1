using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// This means a weapon can be lowered
/// </summary>
[Title( "Lowerable" ), Group( "Weapon Components" )]
public partial class Lowerable : EquipmentComponent, IGameEventHandler<EquipmentFlagChanged>
{
	TimeSince TimeSinceLowered { get; set; } = 0;

	protected override void OnEnabled()
	{
		TimeSinceLowered = 0;

		BindTag( "no_shooting", () => TimeSinceLowered < 0.2f );
		BindTag( "lowered", () => Equipment.EquipmentFlags.HasFlag( EquipmentFlags.Lowered ) );
	}

	void IGameEventHandler<EquipmentFlagChanged>.OnGameEvent( EquipmentFlagChanged e )
	{
		if ( !e.Flag.Equals( EquipmentFlags.Lowered ) )
			return;

		Sound.Play( Equipment.HasFlag( EquipmentFlags.Lowered ) ? "player.jump.gear" : "player.walk.gear", WorldPosition );
		TimeSinceLowered = 0;
	}

	void ToggleLower()
	{
		// Cooldown
		if ( TimeSinceLowered < 0.5f )
			return;

		Equipment.ToggleFlag( EquipmentFlags.Lowered );
		TimeSinceLowered = 0;
	}

	protected override void OnFixedUpdate()
	{
		if ( !Equipment.IsValid() )
			return;

		if ( !Equipment.IsDeployed )
			return;

		if ( !Equipment.Owner.IsValid() )
			return;

		if ( !Equipment.Owner.IsLocallyControlled )
			return;

		if ( Equipment.Owner.Hovered.IsValid() )
			return;

		if ( Equipment.HasFlag( EquipmentFlags.Aiming ) || Equipment.HasFlag( EquipmentFlags.Reloading ) )
			return;

		if ( Input.Down( "Use" ) && Input.Down( "Reload" ) )
		{
			ToggleLower();
		}
	}
}
