namespace Facepunch;

public partial class PlayAreaSystem : GameObjectSystem
{
	public PlayAreaSystem( Scene scene ) : base( scene )
	{
		//
	}

	public List<PlayArea> All { get; set; } = new();
}

public partial class PlayArea : Component
{
	[Property] public Zone Zone { get; set; }

	public static int Count { get; set; } = 0;

	protected override void OnEnabled()
	{
		var system = Scene.GetSystem<PlayAreaSystem>();
		system.All.Add( this );
		Count = system.All.Count;
	}
}
