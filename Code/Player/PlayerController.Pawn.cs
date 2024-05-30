namespace Facepunch;

public partial class PlayerController
{
	public void OnPossess()
	{
		CreateViewModel();
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
		CameraController.SetActive( true );
	}

	public void OnDePossess()
	{
		CameraController.SetActive( false );
		ClearViewModel();
	}
}
