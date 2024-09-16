namespace Facepunch;

public partial class TeamVoiceChatFilter : Component, IVoiceFilter
{
	private bool DontSendFilter( PlayerState player )
	{
		// Send voices to spectators
		if ( player.Team == Team.Unassigned ) return false;

		// Don't send to enemies
		return !player.IsFriendly( PlayerState.Local );
	}

	IEnumerable<Connection> IVoiceFilter.GetExcludeFilter()
	{
		return Scene.GetAllComponents<PlayerState>()
			.Where( DontSendFilter )
			.Select( x => x.Connection );
	}
}
