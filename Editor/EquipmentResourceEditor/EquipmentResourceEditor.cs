using Editor;
using Sandbox;
using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Label = Editor.Label;

namespace Facepunch.Editor;

public sealed class EquipmentResourceEditor : BaseResourceEditor<EquipmentResource>
{
	private SerializedObject Object { get; set; }

	public EquipmentResourceEditor()
	{
		Layout = Layout.Column();
	}

	protected override void SavedToDisk()
	{
		base.SavedToDisk();

		var prefabFile = Resource?.MainPrefab?.Scene?.Source as PrefabFile;
		if ( prefabFile is null ) return;

		var prefabAsset = AssetSystem.FindByPath( prefabFile.ResourcePath );
		prefabAsset?.SaveToDisk( prefabFile );
	}

	protected override void Initialize( Asset asset, EquipmentResource resource )
	{
		Layout.Clear( true );

		Object = resource.GetSerialized();

		var sheet = new ControlSheet();
		sheet.AddObject( Object );

		Layout.Add( sheet );

		foreach ( var typeDesc in TypeLibrary.GetTypes<EquipmentComponent>() )
		{
			AddPrefabComponentProperties( typeDesc );
		}

		Object.OnPropertyChanged += NoteChanged;
	}

	private void AddPrefabComponentProperties( TypeDescription typeDesc )
	{
		if ( typeDesc.IsAbstract ) return;
		if ( typeDesc.Properties.All( x => !x.HasAttribute<EquipmentResourcePropertyAttribute>() ) )
		{
			return;
		}

		var prefabFile = Resource?.MainPrefab?.Scene?.Source as PrefabFile;
		var prefabJson = prefabFile?.RootObject;
		if ( prefabJson is null ) return;

		var compJson = FindComponentJson( prefabJson, typeDesc );
		if ( compJson is null ) return;

		var comp = typeDesc.Create<Component>();
		comp.DeserializeImmediately( compJson );

		var serialized = comp.GetSerialized();
		var properties = serialized.Where( x => x.HasAttribute<EquipmentResourcePropertyAttribute>() ).ToArray();

		serialized.OnPropertyChanged += prop =>
		{
			var jsonName = prop.TryGetAttribute( out JsonPropertyNameAttribute attrib ) ? attrib.Name : prop.Name;
			compJson[jsonName] = Json.ToNode( prop.GetValue<object>(), prop.PropertyType );

			NoteChanged( prop );
		};

		var sheet = new ControlSheet { Margin = new Sandbox.UI.Margin( 0, 0, 0, 0 ) };
		sheet.AddGroup( typeDesc.Title, properties );

		Layout.Add( sheet );

		if ( comp is Shootable shootWeapon )
		{
			var debugWidget = new ShootWeaponDebugWidget( Resource, shootWeapon );

			Layout.Add( debugWidget );

			Object.OnPropertyChanged += _ => debugWidget.UpdateGrid();
			serialized.OnPropertyChanged += _ => debugWidget.UpdateGrid();
		}
	}

	private JsonObject FindComponentJson( JsonObject obj, TypeDescription typeDesc )
	{
		if ( obj?["Components"] is not JsonArray components )
		{
			return null;
		}

		foreach ( var component in components )
		{
			if ( component?["__type"]?.GetValue<string>() != typeDesc.FullName )
			{
				continue;
			}

			return component!.AsObject();
		}

		if ( obj["Children"] is not JsonArray children )
		{
			return null;
		}

		foreach ( var child in children )
		{
			if ( FindComponentJson( child?.AsObject(), typeDesc ) is { } match )
			{
				return match;
			}
		}

		return null;
	}
}

file sealed class ShootWeaponDebugWidget : Widget
{
	public EquipmentResource Resource { get; }
	public Shootable ShootWeapon { get; }

	public ShootWeaponDebugWidget( EquipmentResource resource, Shootable shootWeapon )
	{
		Resource = resource;
		ShootWeapon = shootWeapon;

		var grid = Layout.Grid();

		grid.VerticalSpacing = 4;
		grid.HorizontalSpacing = 8;

		Layout = grid;
		Layout.Margin = new Sandbox.UI.Margin( 16f, 16f, 16f, 16f );

		UpdateGrid();
	}

	private static (bool ArmorHelmet, HitboxTags HitboxTags)[] Cases { get; } =
	{
		(false, HitboxTags.Head), (true, HitboxTags.Head),
		(false, HitboxTags.Chest), (true, HitboxTags.Chest)
	};

	public void UpdateGrid()
	{
		var grid = (GridLayout)Layout;

		grid.Clear( true );

		grid.AddCell( 0, 0, new Label.Small( "Body Part" ), alignment: TextFlag.Center );
		grid.AddCell( 1, 0, new Label.Small( "Armor" ), alignment: TextFlag.Center );
		grid.AddCell( 2, 0, new Label.Small( "Damage" ), alignment: TextFlag.Center );
		grid.AddCell( 3, 0, new Label.Small( "Shots to Kill" ), alignment: TextFlag.Center );

		var hitboxConifgs = PlayerGlobals.GetDefaultHitboxConfigs();

		for ( var i = 0; i < Cases.Length; i++ )
		{
			var damageCase = Cases[i];
			var row = i + 1;

			var flags = damageCase.ArmorHelmet
				? DamageFlags.Armor | DamageFlags.Helmet
				: 0;

			PlayerGlobals.GetDamageModifications( flags, damageCase.HitboxTags,
				Resource.ArmorReduction ?? PlayerGlobals.DefaultArmorReduction,
				Resource.HelmetReduction ?? PlayerGlobals.DefaultHelmetReduction,
				hitboxConifgs, out var damageScale, out var armorReduction, out _ );

			var damage = ShootWeapon.BaseDamage;

			damage *= damageScale;
			damage *= armorReduction;

			grid.AddCell( 0, row, new Label( damageCase.HitboxTags.ToString() ), alignment: TextFlag.Center );
			grid.AddCell( 1, row, new Label( damageCase.ArmorHelmet ? "\u2611" : "\u2610" ), alignment: TextFlag.Center );
			grid.AddCell( 2, row, new Label( damage.ToString( "F1" ) ), alignment: TextFlag.Center );
			grid.AddCell( 3, row, new Label( Math.Ceiling( 100f / damage ).ToString( "N0" ) ), alignment: TextFlag.Center );
		}
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		Paint.ClearPen();
		Paint.SetBrush( Theme.ControlBackground.Lighten( 0.5f ) );
		Paint.DrawRect( Layout.InnerRect.Grow( 8f ) );
	}
}
