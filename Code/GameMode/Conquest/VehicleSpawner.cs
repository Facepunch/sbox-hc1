
namespace Facepunch;

public partial class VehicleSpawner : Component
{
	[Property] public GameObject Prefab { get; set; }

	public GameObject Instance { get; set; }

	protected override void OnStart()
	{
		GetOrCreate();
	}

	public GameObject GetOrCreate()
	{
		Instance = Prefab?.Clone( new CloneConfig()
		{
			StartEnabled = true,
			Transform = Transform.World,
			Name = "Spawned Vehicle",
			Parent = null
		} );

		var handle = Instance.Components.Create<SpawnedVehicleHandle>();
		handle.Spawner = this;

		return Instance;
	}

	/// <summary>
	/// Called when a vehicle is destroyed so we can spawn another
	/// </summary>
	/// <param name="go"></param>
	internal void Notify( GameObject go )
	{
		GetOrCreate();
	}
}
