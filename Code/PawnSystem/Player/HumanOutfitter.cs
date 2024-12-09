using Sandbox.Events;

namespace Facepunch;

public sealed class HumanOutfitter : Component,
	IGameEventHandler<TeamChangedEvent>
{
	public struct TeamModelEntry 
	{
		[KeyProperty] public List<Model> Models { get; set; }
	}

	[Property] public PlayerPawn PlayerPawn { get; set; }
	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property, InlineEditor] public Dictionary<Team, TeamModelEntry> TeamBaseModels { get; set; } = new();

	void IGameEventHandler<TeamChangedEvent>.OnGameEvent( TeamChangedEvent eventArgs )
	{
		UpdateFromTeam( eventArgs.After );
	}

	/// <summary>
	/// Called to wear an outfit based off a team.
	/// </summary>
	/// <param name="team"></param>
	[Rpc.Broadcast( NetFlags.HostOnly )]
	public void UpdateFromTeam( Team team )
	{
		if ( !TeamBaseModels.TryGetValue( team, out var model ) )
		{
			model = TeamBaseModels[Team.Terrorist];
		}

		Renderer.Model = Game.Random.FromList( model.Models );
		PlayerPawn.Body.Refresh();
	}
}
