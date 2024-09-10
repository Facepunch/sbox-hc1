using Sandbox.Events;

namespace Facepunch;

public class Loadout
{
	[KeyProperty] public TeamDefinition Team { get; set; }
	[KeyProperty] public List<EquipmentResource> Equipment { get; set; }
}

public sealed class DefaultEquipment : Component,
	IGameEventHandler<PlayerSpawnedEvent>
{
	/// <summary>
	/// A weapon set that we'll give the player when they spawn.
	/// </summary>
	[Property] public List<Loadout> TeamLoadouts { get; set; }

	[Property] public int Armor { get; set; }
	[Property] public bool Helmet { get; set; }
	[Property] public bool DefuseKit { get; set; }
	[Property] public bool RefillAmmo { get; set; } = true;
	[Property] public bool LoadoutsEnabled { get; set; } = true;

	public Loadout GetLoadout( TeamDefinition team )
	{
		if ( TeamLoadouts.FirstOrDefault( x => x.Team == team ) is { } loadout )
		{
			return loadout;
		}

		return TeamLoadouts.FirstOrDefault();
	}

	public bool Contains( TeamDefinition team, EquipmentResource resource )
	{
		var loadout = GetLoadout( team );
		if ( loadout is null ) return false;

		return loadout.Equipment.Contains( resource );
	}

	void IGameEventHandler<PlayerSpawnedEvent>.OnGameEvent( PlayerSpawnedEvent eventArgs )
	{
		var loadout = GetLoadout( eventArgs.Player.Team );
		if ( loadout is null )
			return;

		var player = eventArgs.Player;

		if ( LoadoutsEnabled )
		{
			foreach ( var resource in player.PlayerState.Loadout.Equipment )
			{
				if ( !player.Inventory.HasInSlot( resource.Slot ) )
				{
					player.Inventory.Give( resource, false );
				}
			}
		}

		foreach ( var weapon in loadout.Equipment )
		{
			if ( !player.Inventory.HasInSlot( weapon.Slot ) )
				player.Inventory.Give( weapon, false );
		}

		player.Inventory.SwitchToBest();

		player.ArmorComponent.Armor = Math.Max( player.ArmorComponent.Armor, Armor );
		player.ArmorComponent.HasHelmet |= Helmet;

		if ( DefuseKit && player.Team is not null && player.Team.Tags.Has( "ct" ) )
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
		foreach ( var player in GameUtils.PlayerPawns )
		{
			player.Inventory.Clear();
		}
	}
}
