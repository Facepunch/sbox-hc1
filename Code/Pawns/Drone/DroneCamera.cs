namespace Facepunch;

public partial class DroneCamera : Component
{
	[RequireComponent] public CameraComponent CameraComponent { get; set; }
	[Property] public AudioListener Listener { get; set; }

	public void SetActive( bool active )
	{
		CameraComponent.Enabled = active;
		Listener.Enabled = active;
	}
}
