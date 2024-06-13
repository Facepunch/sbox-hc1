namespace Facepunch;

public partial class PlayerController
{
	[DeveloperCommand( "-50 HP", "Player" )]
	private static void Command_Hurt()
	{
		var player = GameUtils.Viewer;
		if ( player.IsValid() )
			player.HealthComponent.TakeDamage( new DamageInfo( player as Component, 50, Hitbox: HitboxTags.Head ) );
	}

	[DeveloperCommand( "-10 HP", "Player" )]
	private static void Command_HurtTen()
	{
		var player = GameUtils.Viewer;
		if ( player.IsValid() )
			player.HealthComponent.TakeDamage( new DamageInfo( player as Component, 10, Hitbox: HitboxTags.Head ) );
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
