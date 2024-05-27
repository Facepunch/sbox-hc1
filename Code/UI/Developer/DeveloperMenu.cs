namespace Facepunch;

public partial class Developer
{
	internal static PlayerController CurrentPlayer;

	/// <summary>
	/// Quick and dirty way to find the player we're controlling right now
	/// </summary>
	/// <returns></returns>
	private static PlayerController FindPlayer()
	{
		var connection = Connection.Local;

		foreach ( var player in Game.ActiveScene.GetAllComponents<PlayerController>() )
		{
			if ( player.Network.OwnerConnection is null ) continue;
			// Found our guy
			if ( player.Network.OwnerConnection.SteamId == connection.SteamId ) return player;
		}

		return null;
	}

	internal static void InvokeMethod( MethodDescription method )
	{
		Developer.CurrentPlayer = Game.ActiveScene.GetSystem<PawnSystem>().Viewer as PlayerController;

		method.Invoke( null );

		Developer.CurrentPlayer = null;
	}
}

public class DeveloperCommandAttribute : Attribute
{
	public string Name { get; set; }
	public string Description { get; set; }

	public DeveloperCommandAttribute( string name, string desc = null )
	{
		Name = name;
		Description = desc;
	}
}
