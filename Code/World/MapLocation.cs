using Sandbox;

namespace Facepunch;

public sealed class MapLocation : Component
{
	[RequireComponent]
	public Zone Zone { get; private set; }
}
