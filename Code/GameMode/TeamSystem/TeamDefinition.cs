
namespace Facepunch;

[GameResource( "HC1/Team Definition", "team", "A team", IconBgColor = "#ad4784", Icon = "hot_tub" )]
public partial class TeamDefinition : GameResource
{
	[Property, Group( "Setup" )]
	public string Name { get; set; } = "My Team";

	[Property, Group( "Setup" )]
	public Color Color { get; set; } = Color.White;

	[Property, Group( "Setup" ), ImageAssetPath]
	public string Icon { get; set; }

	[Property, Group( "Setup" ), ImageAssetPath]
	public string Banner { get; set; }

	[Property, Group( "Tags" )]
	public TagSet Tags { get; set; } = new TagSet();

	[Property, Group( "Appearance" )]
	public Model BaseModel { get; set; }

	/// <summary>
	/// Gets a team with a tag
	/// </summary>
	/// <param name="v"></param>
	/// <returns></returns>
	internal static IEnumerable<TeamDefinition> GetWithTag( string v )
	{
		return ResourceLibrary.GetAll<TeamDefinition>()
			.Where( x => x.Tags.Has( v ) );
	}
}
