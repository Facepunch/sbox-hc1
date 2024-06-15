using Sandbox;

namespace Facepunch;

public sealed class CashPointTracker : Component
{
	public CashPoint Last { get; set; }
	public CashPoint Current { get; set; }
}
