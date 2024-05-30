using Sandbox;
using System.Numerics;

namespace Facepunch;

public sealed class SpectateController : Component, IPawn
{
	public ulong SteamId { get; set; }

	[Property] public float FlySpeed = 10f;
	[Property] public GameObject Camera;

	public Angles ViewAngles { get; set; }

	protected override void OnUpdate()
	{
		ViewAngles += Input.AnalogLook;
		ViewAngles = ViewAngles.WithPitch( ViewAngles.pitch.Clamp( -90, 90 ) );
		Transform.Rotation = ViewAngles.ToRotation();

		Transform.Position += Input.AnalogMove * Transform.Rotation * FlySpeed * Time.Delta;
	}

	void IPawn.OnDePossess()
	{
		Camera.Enabled = false;
	}

	void IPawn.OnPossess()
	{
		Camera.Enabled = true;
	}
}
