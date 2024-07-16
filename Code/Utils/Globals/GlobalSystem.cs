namespace Facepunch;

/// <summary>
/// I wanted gamemodes, and even maps, to be able to override vital global variables.
/// You might say just use convars.. but that doesn't work with 99% of types.
/// 
/// <see cref="GlobalGameNamespace"/> for how we grab them in the scene.
/// 
/// This pretty much lets gamemodes to have their own globals components, to drastically change how the game functions.
/// 
/// I might end up waking up in a few days time and absolutely hating this, so this might disappear soon.
/// 
/// Example: 
/// Player code: How fast should a player move when they're walking?
///		> GetGlobal<PlayerGlobals>().WalkSpeed;
///		
/// Scene: Create a playerglobals component, change the walk speed. There you go, you're done.
/// </summary>
public sealed class GlobalSystem : GameObjectSystem
{
	/// <summary>
	/// A maintained list of all the globals.
	/// </summary>
	private Dictionary<Type, GlobalComponent> Globals { get; set; } = new();

	/// <summary>
	/// The gameobject we're hosting all the globals on.
	/// </summary>
	private GameObject GlobalsGameObject { get; set; }

	public GlobalSystem( Scene scene ) : base( scene )
	{
	}

	/// <summary>
	/// Register an instance of a global component to be cached.
	/// </summary>
	/// <param name="inst"></param>
	public void Register( GlobalComponent inst )
	{
		if ( !GlobalsGameObject.IsValid() )
			GlobalsGameObject = inst.GameObject;

		var type = inst.GetType();
		Globals[type] = inst;
	}

	/// <summary>
	/// Creates a global since it doesn't exist in the scene.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T Create<T>() where T : GlobalComponent
	{
		if ( !GlobalsGameObject.IsValid() )
		{
			GlobalsGameObject = Scene.CreateObject();
			GlobalsGameObject.Name = "Fallback Globals";
		}

		var typeDescription = TypeLibrary.GetType<T>();
		var inst = GlobalsGameObject.Components.Create( typeDescription ) as T;

		Register( inst );

		return inst;
	}

	/// <summary>
	/// Tries to grab a global, if it exists. If it doesn't, we'll make one.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T Get<T>() where T : GlobalComponent
	{
		if ( Globals.TryGetValue( typeof ( T ), out var existing ) && existing is T { IsValid: true } global )
			return global;

		return Create<T>();
	}
}
