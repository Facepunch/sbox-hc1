namespace Facepunch;

public interface IPlayerEvents : ISceneEvent<IPlayerEvents>
{
	/// <summary>
	/// Called when the player is ragdolled
	/// </summary>
	void OnRagdolled( PlayerPawn player, ref float destroyTime ) { }

	/// <summary>
	/// Called when spawning the player
	/// </summary>
	/// <param name="player"></param>
	void OnSpawned( PlayerPawn player ) { }
}
