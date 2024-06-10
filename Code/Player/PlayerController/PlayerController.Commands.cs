namespace Facepunch;

public partial class PlayerController
{
	[DeveloperCommand( "-50 HP", "Player" )]
	private static void Command_Hurt()
	{
		var player = GameUtils.Viewer;
		if ( player.IsValid() )
		{
			player.HealthComponent.TakeDamage( 50, Vector3.Zero, Vector3.Zero, player.HealthComponent.Id );
		}
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
		var player = GameUtils.Viewer;
		if ( player.IsValid() )
		{
			player.Kill();
		}
	}

	[DeveloperCommand( "Give $1k", "Player" )]
	private static void Command_GiveGrand()
	{
		var player = GameUtils.Viewer;
		if ( player.IsValid() )
		{
			player.Inventory.GiveCash(1000);
		}
	}
}
