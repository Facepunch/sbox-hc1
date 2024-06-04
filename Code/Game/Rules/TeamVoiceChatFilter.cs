namespace Facepunch;

public partial class TeamVoiceChatFilter : Component, IVoiceFilter
{
	bool IVoiceFilter.ShouldExclude( Connection listener )
	{
		if ( GameNetworkManager.PlayerConnections.TryGetValue( listener, out var player ) )
		{
			return !player.IsFriendly( GameUtils.LocalPlayer );
		}
		return false;
	}
}
