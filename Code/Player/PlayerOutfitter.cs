namespace Facepunch;

public class PlayerOutfit
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
	/// Handle the helmet separately, this'll get applied only when the player has a helmet purchased.
	/// </summary>
	[Property] public Clothing Helmet { get; set; }

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
			if ( !container.Has( item ) )
				container.Toggle( item );
		}

		// Do we have a hat on?
		var hat = container.Clothing.FirstOrDefault( x => x.Clothing.Category == Clothing.ClothingCategory.Hat );

		// Take off the hat
		if ( hat is not null )
			container.Toggle( hat.Clothing );

		// Do we have a helmet?
		if ( outfitter.PlayerController.HealthComponent.HasHelmet )
		{
			// Turn on our helmet override
			container.Toggle( Helmet );
		}

		// Apply to player
		container.Apply( outfitter.Body.Renderer );
	}

	public override string ToString()
	{
		return $"Outfit, {Items.Count} item(s)";
	}
}

public class TeamOutfits
{
	[KeyProperty] public Team Team { get; set; }

	public List<PlayerOutfit> Outfits { get; set; } = new();

	public PlayerOutfit TakeRandom()
	{
		return Game.Random.FromList( Outfits );
	}

	public override string ToString()
	{
		return $"{Team}, {Outfits.Count} outfits";
	}
}

/// <summary>
/// An extremely basic outfitter, which'll swap between two gameobjects (which are holding an outfit)
/// </summary>
public partial class PlayerOutfitter : Component, Component.INetworkSpawn
{
	[RequireComponent] public PlayerBody Body { get; set; }

	[Property] public List<TeamOutfits> Outfits { get; set; }
	[Property] public TeamComponent TeamComponent { get; set; }
	[Property] public PlayerController PlayerController { get; set; }
	[Property] public Team FallbackTeam { get; set; }

	public void OnResetState( PlayerController player )
	{
		UpdateFromTeam( player.TeamComponent.Team );
	}

	private void OnTeamChanged( Team oldTeam, Team newTeam )
	{
		if ( PlayerController.IsSpectating )
			return;
		
		UpdateFromTeam( newTeam );
	}

	protected override void OnEnabled()
	{
		TeamComponent.OnTeamChanged += OnTeamChanged;
		base.OnEnabled();
	}

	protected override void OnDisabled()
	{
		TeamComponent.OnTeamChanged -= OnTeamChanged;
		base.OnDisabled();
	}

	[Broadcast( NetPermission.HostOnly )]
	private void UpdateFromTeam( Team team )
	{
		var teamOutfits = Outfits.FirstOrDefault( x => x.Team == team );

		if ( teamOutfits is null )
			teamOutfits = Outfits.FirstOrDefault( x => x.Team == FallbackTeam );

		if ( teamOutfits is null )
			return;

		var outfit = teamOutfits.TakeRandom();
		outfit.Wear( this );

		PlayerController.Body.ReapplyVisibility();
	}

	[Sync] public string Avatar { get; set; }

	public void OnNetworkSpawn( Connection owner )
	{
		Avatar = owner.GetUserData( "avatar" );
	}
}
