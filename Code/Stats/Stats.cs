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
	}
}
