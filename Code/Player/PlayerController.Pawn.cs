namespace Facepunch;

public partial class PlayerController
{
	public void OnPossess()
	{
		CreateViewModel();
		SetupCamera();

		HUDGameObject.Enabled = true;
	}

	[Broadcast]
	public void NetPossess()
	{
		// Don't own? Go away
		if ( IsProxy )
			return;

		(this as IPawn ).Possess();
	}

	[Broadcast]
	public void NetDePossess()
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
		HUDGameObject.Enabled = false;
		CameraController.SetActive( false );
		ClearViewModel();
	}
}
