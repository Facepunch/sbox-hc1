namespace Facepunch;

public interface IPingReceiver : IValid
{
	/// <summary>
	/// The position of the ping marker (on the minimap AND the screen)
	/// </summary>
	public Vector3 Position { get; }

	/// <summary>
	/// Should we override the icon on the minimap and screen?
	/// </summary>
	public string Icon => null;

	/// <summary>
	/// Should we override the icon on the minimap and screen?
	/// </summary>
	public string Text => null;

	/// <summary>
	/// What should the colour be?
	/// </summary>
	public Color? Color => null;

	/// <summary>
	/// Called when we start pinging something
	/// </summary>
	public void OnPing() { }

	/// <summary>
	/// Called when this ping gets cancelled by another
	/// </summary>
	public void OnPingCancel() { }

	/// <summary>
	/// Should this show?
	/// </summary>
	/// <returns></returns>
	public bool ShouldShow() => true;
}

/// <summary>
/// A simple component that handles pinging for the player.
/// </summary>
public partial class PlayerPingComponent : Component
{
	/// <summary>
	/// The player
	/// </summary>
	[RequireComponent] PlayerPawn Player { get; set; }

	/// <summary>
	/// How far can we ping?
	/// </summary>
	[Property] public float PingDistance { get; set; } = 10000f;

	/// <summary>
	/// How long do we exist for?
	/// </summary>
	[Property] public float Lifetime { get; set; } = 15f;

	/// <summary>
	/// Store a reference to the last ping this player placed.
	/// </summary>
	WorldPingComponent WorldPing { get; set; }

	/// <summary>
	/// Sends a ping to people on the server, can only be called by the owner of this player.
	/// This gets networked to people on the same team.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="target"></param>
	[Rpc.Owner]
	public void Ping( Vector3 position, Component target = null )
	{
		// Destroy any active pings
		if ( WorldPing.IsValid() )
		{
			WorldPing?.Receiver?.OnPingCancel();
			WorldPing?.GameObject?.Destroy();
		}

		var pingObject = new GameObject();
		pingObject.WorldPosition = position;
		pingObject.Transform.ClearInterpolation();

		var ping = pingObject.Components.Create<WorldPingComponent>();
		ping.Owner = Player.Client;
		pingObject.Name = $"Ping from {ping.Owner.DisplayName}";

		if ( target.IsValid() )
			ping.Target = target;

		WorldPing = ping;
		// trigger the ping to be destroyed at some point
		ping.Trigger( Lifetime );
	}

	[Rpc.Owner]
	public void RemovePing()
	{
		WorldPing?.Receiver?.OnPingCancel();
		WorldPing?.GameObject?.Destroy();
	}

	/// <summary>
	/// Can we ping?
	/// </summary>
	/// <returns></returns>
	private bool CanPing()
	{
		var tr = GetTrace();
		if ( !tr.Hit )
			return false;

		return Player.HealthComponent.State == LifeState.Alive;
	}

	/// <summary>
	/// Trace for the ping
	/// </summary>
	/// <returns></returns>
	private SceneTraceResult GetTrace()
	{
		var tr = Scene.Trace.Ray( Player.AimRay, PingDistance )
			.IgnoreGameObjectHierarchy( Player.GameObject.Root )
			.Run();

		return tr;
	}

	protected override void OnUpdate()
	{
		if ( !Player.IsLocallyControlled )
			return;

		// Are we wanting to ping, can we ping?
		if ( Input.Pressed( "Ping" ) && CanPing() )
		{
			if ( WorldPing.IsValid() )
			{
				var camera = Player.CameraController.Camera;
				Vector3 dir = (WorldPing.WorldPosition - camera.WorldPosition).Normal;
				float dot = Vector3.Dot( dir, camera.WorldRotation.Forward );

				// Are we looking at this marker?
				if ( dot.AlmostEqual( 1f, 0.01f ) )
				{
					using ( NetworkUtils.RpcMyTeam() )
						RemovePing();
					
					return;
				}
			}

			var tr = GetTrace();
			if ( !tr.Hit ) return;

			var target = tr.GameObject.Root.GetComponent<IPingReceiver>() as Component;

			// Send a RPC to my teammates
			using ( NetworkUtils.RpcMyTeam() )
			{
				Ping( tr.EndPosition, target );
			}
		}
	}
}
