namespace Facepunch;

public partial class PlayAreaSystem : GameObjectSystem
{
	public PlayAreaSystem( Scene scene ) : base( scene )
	{
		//
	}

	public List<PlayArea> All { get; set; } = new();
	public int Count { get; set; } = 0;
}

public partial class PlayArea : Component
{
	[Property] public Zone Zone { get; set; }


	protected override void OnEnabled()
	{
		var system = Scene.GetSystem<PlayAreaSystem>();
		system.All.Add( this );
		system.Count = system.All.Count;
	}
}
