namespace Facepunch;

public partial class TeamVoiceChatFilter : Component, IVoiceFilter
{
	private bool DontSendFilter( PlayerPawn player )
	{
		// Send voices to spectators
		if ( player.Team == Team.Unassigned ) return false;

		// Don't send to enemies
		return !player.IsFriendly( GameUtils.LocalPlayer );
	}

	IEnumerable<Connection> IVoiceFilter.GetExcludeFilter()
	{
		return Scene.GetAllComponents<PlayerPawn>()
			.Where( DontSendFilter )
			.Select( x => x.Network.OwnerConnection );
	}
}
