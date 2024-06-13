namespace Facepunch;

public partial class TeamVoiceChatFilter : Component, IVoiceFilter
{
	private bool DontSendFilter( PlayerController player )
	{
		// Send voices to spectators
		if ( player.TeamComponent.Team == Team.Unassigned ) return false;

		// Don't send to enemies
		return !player.IsFriendly( GameUtils.LocalPlayer );
	}

	IEnumerable<Connection> IVoiceFilter.GetExcludeFilter()
	{
		return Scene.GetAllComponents<PlayerController>()
			.Where( DontSendFilter )
			.Select( x => x.Network.OwnerConnection );
	}
}
