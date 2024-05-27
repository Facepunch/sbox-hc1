namespace Gunfight;

public partial class GameEventSystem : GameObjectSystem
{
	/// <summary>
	/// Called when a player damages something
	/// </summary>
	public Action<GameObject, DamageInfo> OnDamageGivenEvent { get; set; }

	public GameEventSystem( Scene scene ) : base( scene )
	{
	}
}
