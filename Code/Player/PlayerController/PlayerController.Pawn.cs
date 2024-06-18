namespace Facepunch;

public partial class PlayerController
{
	/// <summary>
	/// Sync the player's steamid
	/// </summary>
	[Sync] public ulong SteamId { get; set; }

	[RequireComponent] public PlayerState PlayerState { get; set; }

	/// <summary>
	/// A shorthand accessor to say if we're controlling this player.
	/// </summary>
	public bool IsLocallyControlled => IsViewer && PlayerState.IsLocalPlayer;

	/// <summary>
	/// Is this player the currently possessed controller
	/// </summary>
	public bool IsViewer => (this as IPawn).IsPossessed;

	/// <summary>
	/// What are we called?
	/// </summary>
	public string DisplayName => PlayerState.DisplayName;

	/// <summary>
	/// Unique colour or team colour of this player
	/// </summary>
	public Color PlayerColor => PlayerColors.Instance?.GetColor(this) ?? TeamComponent.Team.GetColor();

	/// <summary>
	/// Called when possessed.
	/// </summary>
	public void OnPossess()
	{
		SetupCamera();
	}

	public void TryDePossess()
	{
		if ( !IsLocallyControlled ) return;
		(this as IPawn).DePossess();
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

	public void OnDePossess()
	{
		CameraController.SetActive( false );
	}
}
