namespace Facepunch;

public partial class SpawnedVehicleHandle : Component
{
	public VehicleSpawner Spawner { get; set; }

	protected override void OnDestroy()
	{
		Spawner?.Notify( GameObject );
	}
}
