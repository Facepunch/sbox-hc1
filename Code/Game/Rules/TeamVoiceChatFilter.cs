namespace Facepunch;

public partial class TeamVoiceChatFilter : Component, IVoiceFilter
{
	IEnumerable<Connection> IVoiceFilter.GetExcludeFilter()
	{
		return Scene.GetAllComponents<PlayerController>()
			.Where( x => !x.IsFriendly( GameUtils.LocalPlayer ) )
			.Select( x => x.Network.OwnerConnection );
	}
}
