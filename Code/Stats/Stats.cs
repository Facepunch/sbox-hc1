namespace Facepunch;

/// <summary>
/// Called to get local stats for the player.
/// </summary>
internal static class Stats
{
	internal static double Get( string name, double fallback = 0 )
	{
		var stat = Sandbox.Services.Stats.LocalPlayer.Get( name );
		return stat.Value;
	}

	internal static void Increment( string name, double amount = 1 )
	{
		// Log.Info( $"Trying to increment stats: {name}, amount: {amount}" );
		Sandbox.Services.Stats.Increment( name, amount );
		//Flush( name );
	}

	/// <summary>
	/// Will flush the stats immediately.. don't turn this on unless you're testing something 
	/// </summary>
	/// <param name="name"></param>
	async static void Flush( string name )
	{
		await Sandbox.Services.Stats.FlushAndWaitAsync();
		Log.Info( $"Flushed {name}, new fetched value: {Get( name )}" );
	}
}
