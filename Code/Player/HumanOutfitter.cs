using Sandbox.Events;

namespace Facepunch;

public sealed class HumanOutfitter : Component,
	IGameEventHandler<TeamChangedEvent>
{
	public class TeamModelEntry 
	{
		[KeyProperty] public TeamDefinition Team { get; set; }
		[KeyProperty] public List<Model> Models { get; set; }
	}

	[Property] 
	public PlayerPawn PlayerPawn { get; set; }

	[Property] 
	public SkinnedModelRenderer Renderer { get; set; }

	[Property, InlineEditor] 
	public List<TeamModelEntry> TeamBaseModels { get; set; } = new();

	void IGameEventHandler<TeamChangedEvent>.OnGameEvent( TeamChangedEvent eventArgs )
	{
		UpdateFromTeam( eventArgs.After );
	}

	/// <summary>
	/// Called to wear an outfit based off a team.
	/// </summary>
	/// <param name="team"></param>
	[Broadcast( NetPermission.HostOnly )]
	public void UpdateFromTeam( TeamDefinition team )
	{
		var entry = TeamBaseModels.FirstOrDefault( x => x.Team == team );

		Renderer.Model = Game.Random.FromList( entry.Models );
		PlayerPawn.Body.Refresh();
	}
}
