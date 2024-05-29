
namespace Facepunch;

partial class PlayerController
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
		_zones.AddRange( Zone.GetAt( Transform.Position ) );
	}
}
