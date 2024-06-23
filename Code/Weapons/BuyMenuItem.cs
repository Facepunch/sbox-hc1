using Facepunch;

public abstract class BuyMenuItem
{
	public string Id { get; protected init; }
	public string Name { get; protected init; }
	public string Icon { get; protected init; }
	public virtual int GetPrice( PlayerState player ) => 0;
	public virtual bool IsOwned( PlayerState player ) => true;
	public virtual bool IsVisible( PlayerState player ) => true;

	protected virtual void OnPurchase( PlayerState player ) { }

	public void Purchase( PlayerState player )
	{
		if ( IsOwned( player ) ) return;

		var price = GetPrice( player );
		player.GiveCash( -price );
		OnPurchase( player );
	}

	public static IEnumerable<BuyMenuItem> GetAll()
	{
		return new List<BuyMenuItem>
		{
			new ArmorEquipment( "kevlar", "Kevlar", "/ui/equipment/armor.png" ),
			new ArmorWithHelmetEquipment( "kevlar_helmet", "Kevlar + Helmet", "/ui/equipment/helmet.png" ),
			new DefuseKitEquipment( "defuse_kit", "Defuse Kit", "/ui/equipment/defusekit.png" )
		};
	}

	public static BuyMenuItem GetById( string id )
	{
		return GetAll().FirstOrDefault( x => x.Id == id );
	}
}

public class ArmorEquipment : BuyMenuItem
{
	public ArmorEquipment( string id, string name, string icon )
	{
		Id = id;
		Name = name;
		Icon = icon;
	}

	public override int GetPrice( PlayerState player ) => 650;

	protected override void OnPurchase( PlayerState player )
	{
		// TODO: tony
		// player.ArmorComponent.Armor = player.ArmorComponent.MaxArmor;
	}

	public override bool IsOwned( PlayerState player ) => /* player.ArmorComponent.Armor == player.ArmorComponent.MaxArmor; */ false;
}

public class ArmorWithHelmetEquipment : BuyMenuItem
{
	public ArmorWithHelmetEquipment( string id, string name, string icon )
	{
		Id = id;
		Name = name;
		Icon = icon;
	}

	public override int GetPrice( PlayerState player )
	{
		//if ( player.ArmorComponent.Armor == player.ArmorComponent.MaxArmor )
		//    return 350;

		return 1000;
	}

	protected override void OnPurchase( PlayerState player )
	{
		//player.ArmorComponent.Armor = player.ArmorComponent.MaxArmor;
		//player.ArmorComponent.HasHelmet = true;

		// Reset the player's outfit
		// player.Outfitter.OnResetState( player );
	}

	public override bool IsOwned( PlayerState player )
	{
		return /*player.ArmorComponent.Armor == player.ArmorComponent.MaxArmor && player.ArmorComponent.HasHelmet;*/ false;
	}
}

public class DefuseKitEquipment : BuyMenuItem
{
	public DefuseKitEquipment( string id, string name, string icon )
	{
		Id = id;
		Name = name;
		Icon = icon;
	}

	public override int GetPrice( PlayerState player ) => 400;

	protected override void OnPurchase( PlayerState player )
	{
		// player.Inventory.HasDefuseKit = true;
	}

	public override bool IsVisible( PlayerState player ) => player.Team == Team.CounterTerrorist;

	public override bool IsOwned( PlayerState player ) => /*player.Inventory.HasDefuseKit;*/ false;
}
