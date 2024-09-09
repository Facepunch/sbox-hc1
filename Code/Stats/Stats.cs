namespace Facepunch;

/// <summary>
/// Called to get local stats for the player.
/// </summary>
internal static class Stats
{
	private static string ParseName( string name, bool gameModeBased = false )
	{
		if ( gameModeBased && !string.IsNullOrEmpty( GameMode.Instance.Ident ) )
		{
			var gameModeName = System.IO.Path.GetFileNameWithoutExtension( GameMode.Instance.Ident );
			return $"{name}-{gameModeName}";
		}

		return name;
	}

	internal static double Get( string name, double fallback = 0, bool gameModeBased = false )
	{
		name = ParseName( name, gameModeBased );
		var stat = Sandbox.Services.Stats.LocalPlayer.Get( name );
		return stat.Value;
	}

	internal static void Increment( string name, double amount = 1, bool gameModeBased = false )
	{
		name = ParseName( name, gameModeBased );

		Log.Info( $"Trying to send {name} stat" );
		// Log.Info( $"Trying to increment stats: {name}, amount: {amount}" );
		Sandbox.Services.Stats.Increment( name, amount );
		//Flush( name );
	}

	/// <summary>
	/// Will flush the stats immediately.. don't turn this on unless you're testing something 
	/// </summary>
	/// <param name="name"></param>
	/// <param name="gameModeBased"></param>
	async static void Flush( string name, bool gameModeBased = false )
	{
		name = ParseName( name, gameModeBased );
		await Sandbox.Services.Stats.FlushAndWaitAsync();
		Log.Info( $"Flushed {name}, new fetched value: {Get( name )}" );
	}
}
