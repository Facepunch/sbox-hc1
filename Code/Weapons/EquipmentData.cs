using Facepunch;

public abstract class EquipmentData
{
	public string Id { get; protected init; }
	public string Name { get; protected init; }
	public string Icon { get; protected init; }
	public virtual int GetPrice( PlayerController player ) => 0;
	public virtual bool IsOwned( PlayerController player ) => true;
	public virtual bool IsVisible( IPawn player ) => true;

	protected virtual void OnPurchase( PlayerController player ) { }

	public void Purchase( PlayerController player )
	{
		if ( IsOwned( player ) ) return;

		var price = GetPrice( player );
		player.Inventory.GiveCash( -price );
		OnPurchase( player );
	}

	public static IEnumerable<EquipmentData> GetAll()
	{
		return new List<EquipmentData>
		{
			new ArmorEquipment( "kevlar", "Kevlar", "/ui/equipment/armor.png" ),
			new ArmorWithHelmetEquipment( "kevlar_helmet", "Kevlar + Helmet", "/ui/equipment/helmet.png" ),
			new DefuseKitEquipment( "defuse_kit", "Defuse Kit", "/ui/equipment/defusekit.png" )
		};
	}

	public static EquipmentData GetById( string id )
	{
		return GetAll().FirstOrDefault( x => x.Id == id );
	}
}

public class ArmorEquipment : EquipmentData
{
	public ArmorEquipment( string id, string name, string icon )
	{
		Id = id;
		Name = name;
		Icon = icon;
	}

	public override int GetPrice( PlayerController player ) => 650;

	protected override void OnPurchase( PlayerController player )
	{
		player.ArmorComponent.Armor = player.ArmorComponent.MaxArmor;
	}

	public override bool IsOwned( PlayerController player ) => player.ArmorComponent.Armor == player.ArmorComponent.MaxArmor;
}

public class ArmorWithHelmetEquipment : EquipmentData
{
	public ArmorWithHelmetEquipment( string id, string name, string icon )
	{
		Id = id;
		Name = name;
		Icon = icon;
	}

	public override int GetPrice( PlayerController player )
	{
		if ( player.ArmorComponent.Armor == player.ArmorComponent.MaxArmor )
			return 350;

		return 1000;
	}

	protected override void OnPurchase( PlayerController player )
	{
		player.ArmorComponent.Armor = player.ArmorComponent.MaxArmor;
		player.ArmorComponent.HasHelmet = true;

		// Reset the player's outfit
		player.Outfitter.OnResetState( player );
	}

	public override bool IsOwned( PlayerController player )
	{
		return player.ArmorComponent.Armor == player.ArmorComponent.MaxArmor && player.ArmorComponent.HasHelmet;
	}
}

public class DefuseKitEquipment : EquipmentData
{
	public DefuseKitEquipment( string id, string name, string icon )
	{
		Id = id;
		Name = name;
		Icon = icon;
	}

	public override int GetPrice( PlayerController player ) => 400;

	protected override void OnPurchase( PlayerController player )
	{
		player.Inventory.HasDefuseKit = true;
	}

	public override bool IsVisible( IPawn player ) => player.GameObject.GetTeam() == Team.CounterTerrorist;

	public override bool IsOwned( PlayerController player ) => player.Inventory.HasDefuseKit;
}
