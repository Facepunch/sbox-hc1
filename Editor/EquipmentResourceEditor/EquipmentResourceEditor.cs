using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Editor;
using Sandbox;

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
