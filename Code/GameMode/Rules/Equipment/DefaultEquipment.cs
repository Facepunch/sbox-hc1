using Sandbox.Events;

namespace Facepunch;

public sealed class DefaultEquipment : Component,
	IGameEventHandler<PlayerSpawnedEvent>
{
	[Property]
	public bool BothTeams { get; set; } = true;

	[Property, ShowIf( nameof(BothTeams), false )]
	public Team Team { get; set; } = Team.Terrorist;

	/// <summary>
	/// A weapon set that we'll give the player when they spawn.
	/// </summary>
	[Property] public List<EquipmentResource> Weapons { get; set; }

	[Property] public int Armor { get; set; }
	[Property] public bool Helmet { get; set; }
	[Property] public bool DefuseKit { get; set; }
	[Property] public bool RefillAmmo { get; set; } = true;

	void IGameEventHandler<PlayerSpawnedEvent>.OnGameEvent( PlayerSpawnedEvent eventArgs )
	{
		if ( Weapons is null ) return;

		var player = eventArgs.Player;

		if ( !BothTeams && player.TeamComponent.Team != Team ) return;

		foreach ( var weapon in Weapons )
		{
			if ( !player.Inventory.HasWeaponInSlot( weapon.Slot ) )
				player.Inventory.GiveWeapon( weapon, false );
		}

		player.Inventory.SwitchToBestWeapon();

		player.ArmorComponent.Armor = Math.Max( player.ArmorComponent.Armor, Armor );
		player.ArmorComponent.HasHelmet |= Helmet;

		if ( DefuseKit && player.TeamComponent.Team == Team.CounterTerrorist )
		{
			player.Inventory.HasDefuseKit = true;
		}

		if ( RefillAmmo )
		{
			player.Inventory.RefillAmmo();
		}
	}
}

/// <summary>
/// Clear all player equipment when entering this state.
/// </summary>
public sealed class ClearEquipment : Component,
	IGameEventHandler<EnterStateEvent>
{
	void IGameEventHandler<EnterStateEvent>.OnGameEvent( EnterStateEvent eventArgs )
	{
		foreach ( var player in GameUtils.ActivePlayers )
		{
			player.Inventory.Clear();
		}
	}
}
