using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// This means a weapon can be lowered
/// </summary>
[Title( "Lowerable" ), Group( "Weapon Components" )]
public partial class Lowerable : EquipmentComponent, IGameEventHandler<EquipmentTagChanged>
{
	TimeSince TimeSinceLowered { get; set; } = 0;

	protected override void OnEnabled()
	{
		TimeSinceLowered = 0;

		BindTag( "no_shooting", () => TimeSinceLowered < 0.2f );
		BindTag( "lowered", () => Equipment.HasTag( "lowered" ) );
	}

	void IGameEventHandler<EquipmentTagChanged>.OnGameEvent( EquipmentTagChanged e )
	{
		if ( !e.Tag.Equals( "lowered" ) )
			return;

		Sound.Play( Equipment.HasTag( "lowered" ) ? "player.jump.gear" : "player.walk.gear", WorldPosition );
		TimeSinceLowered = 0;
	}

	void ToggleLower()
	{
		// Cooldown
		if ( TimeSinceLowered < 0.5f )
			return;

		Equipment.ToggleTag( "lowered" );
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

		if ( Equipment.HasTag( "aiming" ) || Equipment.HasTag( "reloading" ) )
			return;

		if ( Input.Down( "Use" ) && Input.Down( "Reload" ) )
		{
			ToggleLower();
		}
	}
}
