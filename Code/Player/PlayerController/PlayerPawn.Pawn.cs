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
	public override bool IsLocallyControlled => base.IsLocallyControlled && !PlayerState.IsBot;

	protected override void OnEnabled()
	{
		if ( !Spottable.IsValid() )
		{
			Log.Warning( $"No spottable on PlayerPawn Enabled - {this}" );
			return;
		}

		Spottable.Team = Team;
	}

	/// <summary>
	/// Called when possessed.
	/// </summary>
	public override void OnPossess()
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
