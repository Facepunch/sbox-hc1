namespace Facepunch;

public partial class PlayerController
{
	public void OnPossess()
	{
		SetupCamera();
	}

	[Broadcast]
	public void TryPossess()
	{
		(this as IPawn).Possess();
	}

	public void TryDePossess()
	{
		if ( !IsLocallyControlled ) return;
		(this as IPawn).DePossess();
	}

	void SetupCamera()
	{
		// if we're spectating a remote player, use the camera mode preference
		// otherwise: first person for now
		var spectateSystem = SpectateSystem.Instance;
		if ( spectateSystem is not null && (IsProxy || IsBot) )
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
