using Sandbox;

namespace Facepunch;

public sealed class CashPointTracker : Component
{
	public HashSet<CashPoint> All { get; set; } = new();

	public CashPoint Last { get; set; }
	public CashPoint Current { get; set; }

	protected override void OnStart()
	{
		foreach ( var cashPoint in Scene.GetAllComponents<CashPoint>() )
		{
			All.Add( cashPoint );
		}
	}
}
