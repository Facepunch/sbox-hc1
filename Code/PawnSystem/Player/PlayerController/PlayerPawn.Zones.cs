
namespace Facepunch;

// tony: Not a huge fan of this, can't we be reacting to trigger events instead of tracing against spheres for every player every update?
// or just do this on the local player, and tell the host about what zones they're in?
partial class PlayerPawn
{
	private readonly List<Zone> _zones = new();

	/// <summary>
	/// Which <see cref="Zone"/>s is the player currently standing in.
	/// </summary>
	public IEnumerable<Zone> Zones => _zones;

	/// <summary>
	/// Update which <see cref="Zone"/>s the player is standing in.
	/// </summary>
	private void UpdateZones()
	{
		_zones.Clear();
		_zones.AddRange( Zone.GetAt( WorldPosition ) );
	}

	public T GetZone<T>()
	{
		return Zones.Select( x => x.GetComponent<T>() ).FirstOrDefault( x => x is not null );
	}
}
