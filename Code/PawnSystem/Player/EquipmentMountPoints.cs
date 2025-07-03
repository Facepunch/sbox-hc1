using System.Text.Json.Serialization;

namespace Facepunch;

public class MountPoint
{
	[KeyProperty] public EquipmentSlot Slot { get; set; }
	[KeyProperty] public List<GameObject> GameObjects { get; set; } = new();

	/// <summary>
	/// What's currently mounted?
	/// </summary>
	[JsonIgnore]
	public Dictionary<Equipment, GameObject> Mounted { get; set; } = new();

	public bool Mount( Equipment equipment, PlayerPawn player )
	{
		Unmount( equipment, player );

		if ( Mounted.Count < GameObjects.Count )
		{
			var mountSpot = GameObjects.ElementAt( Mounted.Count );
			var inst = equipment.MountedPrefab?.Clone( new CloneConfig()
			{
				Transform = new Transform(),
				Parent = mountSpot,
				StartEnabled = true
			} );

			if ( inst.IsValid() )
			{
				inst.BreakFromPrefab();
				Mounted.Add( equipment, inst );
			}
		}

		return true;
	}

	public bool Unmount( Equipment equipment, PlayerPawn player )
	{
		if ( Mounted.TryGetValue( equipment, out var inst ) )
		{
			Log.Info( $"Found mount point for {equipment}" );
			Mounted.Remove( equipment );
			inst.Destroy();

			return true;
		}
		return false;
	}
}

public sealed class EquipmentMountPoints : Component, IEquipmentEvents
{
	[Property] public PlayerPawn Player { get; set; }

	[Property] public List<MountPoint> MountPoints { get; set; } = new();

	public MountPoint GetMount( Equipment equipment )
	{
		var mount = MountPoints.FirstOrDefault( x => x.Slot == equipment.Resource.Slot );
		return mount;
	}

	void IEquipmentEvents.OnDeployed( Equipment e )
	{
		var mnt = GetMount( e );
		mnt?.Unmount( e, Player );
	}

	void IEquipmentEvents.OnHolstered( Equipment e )
	{
		var mnt = GetMount( e );
		Log.Info( $"Holstering {e}, {mnt}" );
		mnt?.Mount( e, Player );
	}

	void IEquipmentEvents.OnDestroyed( Equipment e )
	{
		var mnt = GetMount( e );
		mnt?.Unmount( e, Player );
	}
}
