namespace Facepunch;

/// <summary>
/// Some network utils, mainly RPC filters.
/// </summary>
public partial class NetworkUtils
{
	/// <summary>
	/// Makes a RPC filter based on your team.
	/// </summary>
	/// <returns></returns>
	public static IDisposable RpcMyTeam()
	{
		return RpcTeam( PlayerState.Local.Team );
	}

	/// <summary>
	/// Makes a RPC filter to a specific team.
	/// </summary>
	/// <param name="team"></param>
	/// <returns></returns>
	public static IDisposable RpcTeam( Team team )
	{
		return Rpc.FilterInclude(
			GameUtils.GetPlayers( team )
			.Select( x => x.Connection ) );
	}
}
