namespace Facepunch;

public partial class PlayerController
{
	[DeveloperCommand( "-10 HP (head)", "Player" )]
	private static void Command_HurtTenHead()
	{
		var player = GameUtils.Viewer;
		if ( player.IsValid() )
			player.HealthComponent.TakeDamage( new DamageInfo( player as Component, 10, Hitbox: HitboxTags.Head ) );
	}

	[DeveloperCommand( "-10 HP (chest)", "Player" )]
	private static void Command_HurtTenChest()
	{
		var player = GameUtils.Viewer;
		if ( player.IsValid() )
			player.HealthComponent.TakeDamage( new DamageInfo( player as Component, 10, Hitbox: HitboxTags.Chest ) );
	}

	[DeveloperCommand( "Heal", "Player" )]
	private static void Command_Heal()
	{
		var player = GameUtils.Viewer;
		if ( player.IsValid() )
		{
			player.HealthComponent.Health = player.HealthComponent.MaxHealth;
		}
	}

	[DeveloperCommand( "Suicide", "Player" ), ConCmd( "kill" )]
	private static void Command_Suicide()
	{
		if ( GameUtils.Viewer is PlayerController player )
		{
			player.Kill();
		}
	}

	[DeveloperCommand( "Give $1k", "Player" )]
	private static void Command_GiveGrand()
	{
		if ( GameUtils.Viewer is PlayerController player )
		{
			player.Inventory.GiveCash(1000);
		}
	}
}
