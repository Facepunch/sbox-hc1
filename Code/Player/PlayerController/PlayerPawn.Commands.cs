namespace Facepunch;

public partial class PlayerPawn
{
	/// <summary>
	/// Development: should bots follow the player's input?
	/// </summary>
	[ConVar( "hc1_bot_follow" )] public static bool BotFollowHostInput { get; set; }

	[DeveloperCommand( "-10 HP (head)", "Player" )]
	private static void Command_HurtTenHead()
	{
		var player = PlayerState.Local.PlayerPawn;
		if ( player is null ) return;
		player.HealthComponent.TakeDamage( new DamageInfo( player as Component, 10, Hitbox: HitboxTags.Head ) );
	}

	[DeveloperCommand( "-10 HP (chest)", "Player" )]
	private static void Command_HurtTenChest()
	{
		var player = PlayerState.Local.PlayerPawn;
		if ( player is null ) return;
		player.HealthComponent.TakeDamage( new DamageInfo( player as Component, 10, Hitbox: HitboxTags.Chest ) );
	}

	[DeveloperCommand( "Heal", "Player" )]
	private static void Command_Heal()
	{
		var player = PlayerState.Local.PlayerPawn;
		if ( player is null ) return;
		player.HealthComponent.Health = player.HealthComponent.MaxHealth;
	}

	[DeveloperCommand( "Suicide", "Player" ), ConCmd( "kill" )]
	private static void Command_Suicide()
	{
		var player = PlayerState.Local.PlayerPawn;
		if ( player is null ) return;
		player.HealthComponent.TakeDamage( new DamageInfo(player, float.MaxValue ) ); // this is brutal
	}

	[DeveloperCommand( "Give $1k", "Player" )]
	private static void Command_GiveGrand()
	{
		var player = PlayerState.Local.PlayerPawn;
		if ( player is null ) return;
		player.PlayerState.GiveCash(1000);
	}
}
