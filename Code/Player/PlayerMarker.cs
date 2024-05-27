namespace Facepunch;

public partial class PlayerMarker : Component, IMarkerObject
{
	/// <summary>
	/// The player.
	/// </summary>
	[RequireComponent] PlayerController Player { get; set; }

	bool IMarkerObject.ShouldShowMarker => Player.SteamId != Connection.Local.SteamId && Player.HealthComponent.State == LifeState.Alive;

	/// <summary>
	/// Custom marker panel
	/// </summary>
	Type IMarkerObject.MarkerPanelTypeOverride => typeof( UI.PlayerMarkerPanel );

	/// <summary>
	/// The connection (if any)
	/// </summary>
	private Connection Connection => Connection.All.FirstOrDefault( x => x.SteamId == Player.SteamId );

	MarkerFrame IMarkerObject.MarkerFrame
	{
		get => new MarkerFrame()
		{
			Position = Transform.Position + Vector3.Up * 80,
			Rotation = Transform.Rotation,
			DisplayText = $"{Connection?.DisplayName ?? "BOT"}"
		};
	}
}
