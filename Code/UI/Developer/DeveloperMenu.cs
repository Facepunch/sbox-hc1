namespace Facepunch;

public class DeveloperCommandAttribute : Attribute
{
	public string Name { get; set; }
	public string Description { get; set; }

	public DeveloperCommandAttribute( string name, string desc = null )
	{
		Name = name;
		Description = desc;
	}
}
