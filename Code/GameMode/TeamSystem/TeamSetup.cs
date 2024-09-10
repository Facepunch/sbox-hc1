namespace Facepunch;

public partial class TeamSetup : Component
{
	[Property]
	public List<TeamDefinition> Teams { get; set; }

	/// <summary>
	/// Quick accessor for this
	/// </summary>
	public static TeamSetup Instance => GameMode.Instance.Get<TeamSetup>();
}
