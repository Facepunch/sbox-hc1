namespace Facepunch;

public class DeveloperCommandAttribute : Attribute
{
	public string Name { get; set; }
	public string Group { get; set; }

	public DeveloperCommandAttribute( string name, string group = "" )
	{
		Name = name;
		Group = group;
	}
}
