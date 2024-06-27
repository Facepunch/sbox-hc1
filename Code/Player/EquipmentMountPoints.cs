using Sandbox.Events;
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
				foreach ( var renderer in inst.Components.GetAll<ModelRenderer>() )
				{
					renderer.RenderType = player.IsViewer ? ModelRenderer.ShadowRenderType.ShadowsOnly : ModelRenderer.ShadowRenderType.On;
				}

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

public sealed class EquipmentMountPoints : Component,
	IGameEventHandler<EquipmentDeployedEvent>,
	IGameEventHandler<EquipmentHolsteredEvent>,
	IGameEventHandler<EquipmentDestroyedEvent>
{
	[Property] public PlayerPawn Player { get;  set; }

	[Property] public List<MountPoint> MountPoints { get; set; } = new();

	public MountPoint GetMount( Equipment equipment )
	{
		var mount = MountPoints.FirstOrDefault( x => x.Slot == equipment.Resource.Slot );
		return mount;
	}

	void IGameEventHandler<EquipmentDeployedEvent>.OnGameEvent( EquipmentDeployedEvent eventArgs )
	{
		var mnt = GetMount( eventArgs.Equipment );
		mnt?.Unmount( eventArgs.Equipment, Player );
	}

	void IGameEventHandler<EquipmentHolsteredEvent>.OnGameEvent( EquipmentHolsteredEvent eventArgs )
	{
		var mnt = GetMount( eventArgs.Equipment );
		Log.Info( $"Holstering {eventArgs.Equipment}, {mnt}" );
		mnt?.Mount( eventArgs.Equipment, Player );
	}

	void IGameEventHandler<EquipmentDestroyedEvent>.OnGameEvent( EquipmentDestroyedEvent eventArgs )
	{
		var mnt = GetMount( eventArgs.Equipment );
		mnt?.Unmount( eventArgs.Equipment, Player );
	}
}
