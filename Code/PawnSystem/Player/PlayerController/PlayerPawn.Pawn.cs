namespace Facepunch;

public partial class PlayerPawn
{
	/// <summary>
	/// Is this player the currently possessed controller
	/// </summary>
	public bool IsViewer => IsPossessed;

	/// <summary>
	/// What are we called?
	/// </summary>
	public override string DisplayName => PlayerState.IsValid() ? PlayerState.DisplayName : "Invalid Player";
	public override bool IsLocallyControlled => base.IsLocallyControlled && !PlayerState.IsBot;

	/// <summary>
	/// Called when possessed.
	/// </summary>
	public override void OnPossess()
	{
		CameraController.SetActive( true );

		// if we're spectating a remote player, use the camera mode preference
		// otherwise: first person for now
		var spectateSystem = SpectateSystem.Instance;
		if ( spectateSystem.IsValid() && ( IsProxy || ( PlayerState.IsValid() && PlayerState.IsBot ) ) )
		{
			CameraController.Mode = spectateSystem.CameraMode;
		}
		else
		{
			CameraController.Mode = CameraMode.FirstPerson;
		}
	}

	public override void OnDePossess()
	{
		CameraController.SetActive( false );
	}
}
