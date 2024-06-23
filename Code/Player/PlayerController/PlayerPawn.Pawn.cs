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
	public override string DisplayName => PlayerState.DisplayName;

	/// <summary>
	/// Unique colour or team colour of this player
	/// </summary>
	public Color PlayerColor => PlayerColors.Instance?.GetColor(this) ?? Team.GetColor();

	/// <summary>
	/// Called when possessed.
	/// </summary>
	public override void OnPossess()
	{
		SetupCamera();
	}

	private void SetupCamera()
	{
		// if we're spectating a remote player, use the camera mode preference
		// otherwise: first person for now
		var spectateSystem = SpectateSystem.Instance;
		if ( spectateSystem is not null && (IsProxy || PlayerState.IsBot) )
		{
			CameraController.Mode = spectateSystem.CameraMode;
		}
		else
		{
			CameraController.Mode = CameraMode.FirstPerson;
		}

		CameraController.SetActive( true );
	}

	public override void OnDePossess()
	{
		CameraController.SetActive( false );
	}
}
