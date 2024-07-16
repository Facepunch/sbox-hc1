using Sandbox.Events;

namespace Facepunch;

/// <summary>
/// An outfit for the player.
/// </summary>
public sealed class PlayerOutfit
{
	/// <summary>
	/// A list of items we'll apply on top of the player's avatar.
	/// </summary>
	public List<Clothing> Items { get; set; } = new();

	/// <summary>
	/// A list of categories we'll discard from the player's avatar.
	/// </summary>
	public List<Clothing.ClothingCategory> DiscardCategories { get; set; } = new();

	/// <summary>
	/// A list of subcategories we'll discard from the player's avatar.
	/// </summary>
	public List<string> DiscardSubcategories { get; set; } = new();

	/// <summary>
	/// Wear the outfit
	/// </summary>
	/// <param name="outfitter"></param>
	public void Wear( PlayerOutfitter outfitter )
	{
		var container = new ClothingContainer();
		container.Deserialize( outfitter.Avatar );

		for ( int i = container.Clothing.Count() - 1; i >= 0; i-- )
		{
			var item = container.Clothing[i];
			if ( DiscardCategories.Contains( item.Clothing.Category ) 
				|| DiscardSubcategories.Contains( item.Clothing.SubCategory ) )
			{
				container.Clothing.RemoveAt( i );
			}
		}

		// Outfit items
		foreach ( var item in Items )
		{
			if ( item is null ) continue;

			if ( !container.Has( item ) )
				container.Toggle( item );
		}

		// Do we have a hat on?
		var hat = container.Clothing.FirstOrDefault( x => x.Clothing.Category == Clothing.ClothingCategory.Hat && x.Clothing.SubCategory != "Masks" );

		// Take off the hat
		if ( hat is not null )
			container.Toggle( hat.Clothing );

		// Apply to player
		container.Apply( outfitter.Body.Renderer );
	}

	public override string ToString()
	{
		return $"Outfit, {Items.Count} item(s)";
	}
}

/// <summary>
/// A list of outfits for a team.
/// </summary>
public sealed class TeamOutfits
{
	/// <summary>
	/// What team is this for?
	/// </summary>
	[KeyProperty] public Team Team { get; set; }

	/// <summary>
	/// A list of outfits for a team.
	/// </summary>
	public List<PlayerOutfit> Outfits { get; set; } = new();

	/// <summary>
	/// Take a random outfit from <see cref="Outfits"/>
	/// </summary>
	/// <returns></returns>
	public PlayerOutfit TakeRandom()
	{
		return Game.Random.FromList( Outfits );
	}
}

/// <summary>
/// A component that handles what a player wears.
/// </summary>
public partial class PlayerOutfitter : Component, Component.INetworkSpawn, 
	IGameEventHandler<TeamChangedEvent>
{
	/// <summary>
	/// The player's body component.
	/// </summary>
	[RequireComponent] public PlayerBody Body { get; set; }

	/// <summary>
	/// A list of outfits per-team.
	/// </summary>
	[Property] public List<TeamOutfits> Outfits { get; set; }

	/// <summary>
	/// The player controller.
	/// </summary>
	[Property] public PlayerPawn PlayerPawn { get; set; }

	/// <summary>
	/// When we're not in a team that has any outfits, what team should we fall back to (preferably one with some outfits!)
	/// </summary>
	[Property] public Team FallbackTeam { get; set; }
	
	/// <summary>
	/// We store the player's avatar over the network so everyone knows what everyone looks like.
	/// </summary>
	[Sync] public string Avatar { get; set; }

	/// <summary>
	/// The stored outfit for this player. We'll use it as their preferred outfit until they change team.
	/// </summary>
	public PlayerOutfit CurrentOutfit { get; set; }

	/// <summary>
	/// Resets the state of the player, using its team.
	/// </summary>
	/// <param name="player"></param>
	public void OnResetState( PlayerPawn player )
	{
		UpdateFromTeam( player.Team );
	}

	/// <summary>
	/// Called when the player's team changes.
	/// </summary>
	/// <param name="oldTeam"></param>
	/// <param name="newTeam"></param>
	private void OnTeamChanged( Team oldTeam, Team newTeam )
	{		
		UpdateFromTeam( newTeam );
	}

	/// <summary>
	/// Assuming we don't want to change the player's outfit, just re-apply the current set of clothing.
	/// Useful when we lose our helmet.
	/// </summary>
	[Broadcast( NetPermission.HostOnly )]
	private void UpdateCurrent()
	{
		CurrentOutfit?.Wear( this );
		PlayerPawn.Body.Refresh();
	}

	/// <summary>
	/// Called to wear an outfit based off a team.
	/// </summary>
	/// <param name="team"></param>
	/// <param name="replace"></param>
	[Broadcast( NetPermission.HostOnly )]
	private void UpdateFromTeam( Team team, bool replace = true )
	{
		var outfit = CurrentOutfit;

		var teamOutfits = Outfits.FirstOrDefault( x => x.Team == team );

		if ( teamOutfits is null )
			teamOutfits = Outfits.FirstOrDefault( x => x.Team == FallbackTeam );

		if ( teamOutfits is null )
			return;

		if ( outfit is null || replace ) outfit = teamOutfits.TakeRandom();

		CurrentOutfit = outfit;
		CurrentOutfit?.Wear( this );

		PlayerPawn.Body.Refresh();
	}

	/// <summary>
	/// Grab the player's avatar data.
	/// </summary>
	/// <param name="owner"></param>
	public void OnNetworkSpawn( Connection owner )
	{
		Avatar = owner.GetUserData( "avatar" );
	}

	void IGameEventHandler<TeamChangedEvent>.OnGameEvent( TeamChangedEvent eventArgs )
	{
		OnTeamChanged( eventArgs.Before, eventArgs.After );
	}
}
