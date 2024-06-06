namespace Facepunch;

public class PlayerOutfit
{
	public List<Clothing> Items { get; set; } = new();

	bool IsKeptCategory( ClothingContainer.ClothingEntry entry )
	{
		if ( entry.Clothing.Category == Clothing.ClothingCategory.Skin ) return true;
		if ( entry.Clothing.Category == Clothing.ClothingCategory.Facial ) return true;
		return false;
	}

	[Property] public Clothing Helmet { get; set; }

	public void Wear( PlayerOutfitter outfitter )
	{
		var clothing = new ClothingContainer();
		clothing.Deserialize( outfitter.Avatar );

		for ( int i = clothing.Clothing.Count() - 1; i >= 0; i-- )
		{
			if ( !IsKeptCategory( clothing.Clothing[i] ) )
			{
				clothing.Clothing.RemoveAt( i );
			}
		}

		// Outfit items
		foreach ( var item in Items )
		{
			if ( !clothing.Has( item ) )
				clothing.Toggle( item );
		}

		// Do we have a hat on?
		var hat = clothing.Clothing.FirstOrDefault( x => x.Clothing.Category == Clothing.ClothingCategory.Hat );

		// Take off the hat
		if ( hat is not null )
			clothing.Toggle( hat.Clothing );

		// Do we have a helmet?
		if ( outfitter.PlayerController.HealthComponent.HasHelmet )
		{
			// Turn on our helmet override
			clothing.Toggle( Helmet );
		}

		// Apply to player
		clothing.Apply( outfitter.Body.Renderer );
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
