namespace Facepunch;

public partial class Crosshair : Component
{
	public static Crosshair Instance { get; set; }

	public float CrosshairGap
	{
		get
		{
			return GameSettingsSystem.Current.CrosshairDistance;
		}
	}

	public bool UseCrosshairDot
	{
		get
		{
			return GameSettingsSystem.Current.ShowCrosshairDot;
		}
	}

	public Color CrosshairColor
	{
		get
		{
			return GameSettingsSystem.Current.CrosshairColor;
		}
	}

	public bool UseDynamicCrosshair
	{
		get
		{
			return GameSettingsSystem.Current.DynamicCrosshair;
		}
	}

	public float CrosshairLength
	{
		get
		{
			return GameSettingsSystem.Current.CrosshairLength;
		}
	}

	protected override void OnStart()
	{
		Instance = this;
	}

	protected override void OnUpdate()
	{
		var hud = Scene.Camera.Hud;
		var center = Screen.Size * 0.5f;
		var pawn = PlayerState.Viewer.Pawn;
		var scale = Screen.Height / 1080.0f;

		var gap = CrosshairGap * scale;
		var len = CrosshairLength * scale;
		var w = 2f * scale;

		Color color = CrosshairColor;

		var player = pawn as PlayerPawn;
		if ( player.IsValid() )
		{
			var equipment = player.CurrentEquipment;

			if ( equipment.IsValid() )
			{
				var recoil = equipment.GetComponentInChildren<RecoilWeaponComponent>( true );
				if ( recoil.IsValid() )
				{
					gap += recoil.Current.AsVector3().Length * 175f * scale;
				}
			}

			if ( UseDynamicCrosshair )
				gap += player.Spread * 150f * scale;

			if ( player.HasEquipmentTag( "aiming" ) )
			{
				color = Color.Transparent;
			}
		}

		hud.SetBlendMode( BlendMode.Lighten );
		hud.DrawLine( center + Vector2.Left * (len + gap), center + Vector2.Left * gap, w, color );
		hud.DrawLine( center - Vector2.Left * (len + gap), center - Vector2.Left * gap, w, color );
		hud.DrawLine( center + Vector2.Up * (len + gap), center + Vector2.Up * gap, w, color );
		hud.DrawLine( center - Vector2.Up * (len + gap), center - Vector2.Up * gap, w, color );

		if ( UseCrosshairDot  )
		{
			hud.DrawCircle( center, 2f * scale, color );
		}
	}
}
