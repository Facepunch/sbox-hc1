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
			if ( item is null ) continue;

			if ( !container.Has( item ) )
				container.Toggle( item );
		}

		// Do we have a hat on?
		var hat = container.Clothing.FirstOrDefault( x => x.Clothing.Category == Clothing.ClothingCategory.Hat && x.Clothing.SubCategory != "Masks" );

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
public partial class PlayerOutfitter : Component, Component.INetworkSpawn, IArmorListener
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

	public PlayerOutfit CurrentOutfit { get; set; }

	[Broadcast( NetPermission.HostOnly )]
	private void UpdateCurrent()
	{
		CurrentOutfit?.Wear( this );
		PlayerController.Body.ReapplyVisibility();
	}

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

		PlayerController.Body.ReapplyVisibility();
	}

	[Sync] public string Avatar { get; set; }

	public void OnNetworkSpawn( Connection owner )
	{
		Avatar = owner.GetUserData( "avatar" );
	}

	[Property] public bool EnableHelmetPhysics { get; set; } = true;

	void IArmorListener.OnHelmetChanged( bool hasHelmet )
	{
		if ( PlayerController.HealthComponent.State != LifeState.Alive )
			return;

		if ( hasHelmet )
		{
			UpdateCurrent();
		}
		else
		{
			if ( !EnableHelmetPhysics )
			{
				UpdateCurrent();
			}
			else
			{
				var player = PlayerController;
				var helmetRenderer = player.Body.Components.GetAll<SkinnedModelRenderer>()
					.FirstOrDefault( x => x.Model.ResourcePath == CurrentOutfit.Helmet.Model );

				if ( !helmetRenderer.IsValid() )
					return;
				
				helmetRenderer.GameObject.SetParent( null );
				helmetRenderer.GameObject.Tags.Add( "no_player" );
				helmetRenderer.BoneMergeTarget = null;

				var phys = helmetRenderer.Components.Create<SphereCollider>();
				phys.Radius = 10f;
				phys.Center = new( 0, 0, 64 );

				var rb = helmetRenderer.Components.Create<Rigidbody>();
				rb.RigidbodyFlags |= RigidbodyFlags.DisableCollisionSounds;

				var destroy = helmetRenderer.Components.Create<TimedDestroyComponent>();
				destroy.Time = 5;

				var rot = player.EyeAngles.ToRotation();

				// Kinda just cheesy random values to make it look good
				rb.Velocity = Vector3.Up * 100f + rot.Left * 25f + rot.Backward * 100f;
				rb.AngularVelocity = rot.Backward * 10f;
			}
		}
	}
}
