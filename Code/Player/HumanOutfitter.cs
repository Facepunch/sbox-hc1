using Sandbox.Events;

namespace Facepunch;

public sealed class HumanOutfitter : Component,
	IGameEventHandler<TeamChangedEvent>
{
	[Property] public PlayerPawn PlayerPawn { get; set; }
	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public Dictionary<Team, Model> TeamBaseModels { get; set; } = new();

	void IGameEventHandler<TeamChangedEvent>.OnGameEvent( TeamChangedEvent eventArgs )
	{
		UpdateFromTeam( eventArgs.After );
	}

	/// <summary>
	/// Called to wear an outfit based off a team.
	/// </summary>
	/// <param name="team"></param>
	[Broadcast( NetPermission.HostOnly )]
	public void UpdateFromTeam( Team team )
	{
		if ( !TeamBaseModels.TryGetValue( team, out var model ) )
		{
			model = TeamBaseModels[Team.Terrorist];
		}

		Renderer.Model = model;
		PlayerPawn.Body.Refresh();
	}
}
