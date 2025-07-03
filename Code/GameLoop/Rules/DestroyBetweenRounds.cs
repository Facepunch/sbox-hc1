using Facepunch;

/// <summary>
/// Destroy this object when a <see cref="IRoundCleanup"/> is dispatched.
/// </summary>
public sealed class DestroyBetweenRounds : Component, IRoundCleanup
{
	void IRoundCleanup.OnRoundCleanup()
	{
		GameObject.Destroy();
	}
}
