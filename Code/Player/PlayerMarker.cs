namespace Facepunch;

public partial class PlayerMarker : Component, IMarkerObject
{
	/// <summary>
	/// The player.
	/// </summary>
	[RequireComponent] PlayerController Player { get; set; }

	bool IMarkerObject.ShouldShowMarker => !(Player as IPawn).IsPossessed && Player.HealthComponent.State == LifeState.Alive;

	/// <summary>
	/// Custom marker panel
	/// </summary>
	Type IMarkerObject.MarkerPanelTypeOverride => typeof( UI.PlayerMarkerPanel );

	MarkerFrame IMarkerObject.MarkerFrame
	{
		get => new MarkerFrame()
		{
			Position = Transform.Position + Vector3.Up * 80,
			Rotation = Transform.Rotation,
			DisplayText = Player.GetPlayerName()
		};
	}

	protected override void OnEnabled()
	{
		( this as IMarkerObject ).Register();
	}

	protected override void OnDisabled()
	{
		( this as IMarkerObject ).UnRegister();
	}
}
