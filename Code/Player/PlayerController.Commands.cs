namespace Facepunch;

public partial class PlayerController
{
	[DeveloperCommand( "Hurt Self" )]
	private static void Command_Hurt()
	{
		var player = Developer.CurrentPlayer;
		if ( player.IsValid() )
		{
			player.HealthComponent.TakeDamage( 50, Vector3.Zero, Vector3.Zero, player.HealthComponent.Id );
		}
	}

	[DeveloperCommand( "Heal" )]
	private static void Command_Heal()
	{
		var player = Developer.CurrentPlayer;
		if ( player.IsValid() )
		{
			player.HealthComponent.Health = 100f;
		}
	}

	[DeveloperCommand( "Suicide" )]
	private static void Command_Suicide()
	{
		var player = Developer.CurrentPlayer;
		if ( player.IsValid() )
		{
			player.Kill();
		}
	}

	[DeveloperCommand( "Depossess Pawn" )]
	private static void Command_Depossess()
	{
		var player = Developer.CurrentPlayer;
		if ( player.IsValid() )
		{
			player.TryDePossess();
		}
	}

	[DeveloperCommand( "Possess Pawn" )]
	private static void Command_PossessSomething()
	{
		foreach ( var player in GameUtils.AllPlayers.OrderBy( x => x.Network.OwnerId ) )
		{
			if ( player == GameUtils.Viewer ) continue;
			(player as IPawn).Possess();
			return;
		}
	}

	[DeveloperCommand("Give $1k")]
	private static void Command_GiveGrand()
	{
		var player = Developer.CurrentPlayer;
		if ( player.IsValid() )
		{
			player.Inventory.GiveCash(1000);
		}
	}
}
