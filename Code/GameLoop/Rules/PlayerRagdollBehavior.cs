namespace Facepunch;

/// <summary>
/// Lets us dictate the behavior of a ragdoll.
/// </summary>
public partial class PlayerRagdollBehavior : Component, IPlayerEvents
{
	/// <summary>
	/// If set to 0, the player ragdoll will only remove after round changes.
	/// </summary>
	[Property] public float DestroyTime { get; set; } = 0f;

	void IPlayerEvents.OnRagdolled( PlayerPawn player, ref float destroyTime )
	{
		destroyTime = DestroyTime;
	}
}
