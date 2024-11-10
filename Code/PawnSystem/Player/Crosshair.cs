using Sandbox.Events;

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

		if ( UseCrosshairDot )
		{
			hud.DrawCircle( center, 2f * scale, color );
		}

		if ( TimeSinceAttacked < HitmarkerTime )
		{
			// Check if the hitmarker type includes a Kill, which keeps the lines from scaling down
			bool isKillHitmarker = Hitmarker.HasFlag( HitmarkerType.Kill );
			bool isHeadshotHitmarker = Hitmarker.HasFlag( HitmarkerType.Headshot ); // Check for Headshot flag

			float initialHitmarkerLength = 20f * scale; // Starting length of the hitmarker lines
			float initialDiagonalOffset = CrosshairGap * scale; // Initial offset distance from the center
			float translationFactor = isKillHitmarker ? 1.0f : 1.0f - (TimeSinceAttacked / HitmarkerTime);
			translationFactor = translationFactor.Clamp( 0.0f, 1.0f );

			float scaleFactor = isKillHitmarker ? 1.0f : (1.0f - (TimeSinceAttacked / HitmarkerTime)).Clamp( 0.0f, 1.0f );

			// Apply scaling factor to line length
			float hitmarkerLength = initialHitmarkerLength * scaleFactor;

			// Apply translation factor to offset distance from center
			float diagonalOffset = initialDiagonalOffset * translationFactor;

			// Calculate the start points for the four diagonal lines with translation applied
			var topLeft = center + new Vector2( -diagonalOffset, -diagonalOffset );
			var topRight = center + new Vector2( diagonalOffset, -diagonalOffset );
			var bottomLeft = center + new Vector2( -diagonalOffset, diagonalOffset );
			var bottomRight = center + new Vector2( diagonalOffset, diagonalOffset );

			var hitColor = isHeadshotHitmarker || isKillHitmarker ? Color.Red : Color.White.WithAlpha( 0.5f );

			// Draw the four diagonal lines with adjusted length and position
			hud.DrawLine( topLeft, topLeft + new Vector2( -hitmarkerLength, -hitmarkerLength ), w, hitColor );
			hud.DrawLine( topRight, topRight + new Vector2( hitmarkerLength, -hitmarkerLength ), w, hitColor );
			hud.DrawLine( bottomLeft, bottomLeft + new Vector2( -hitmarkerLength, hitmarkerLength ), w, hitColor );
			hud.DrawLine( bottomRight, bottomRight + new Vector2( hitmarkerLength, hitmarkerLength ), w, hitColor );
		}
	}

	private float HitmarkerTime => Hitmarker.HasFlag( HitmarkerType.Kill ) ? 0.5f : 0.2f;

	[Flags]
	public enum HitmarkerType
	{
		Default = 1,
		Kill = 2,
		Headshot = 4
	}


	public HitmarkerType Hitmarker { get; set; }
	public TimeSince TimeSinceAttacked { get; set; } = 1000;

	public void Trigger( DamageInfo damageInfo )
	{
		if ( damageInfo.Victim is PlayerPawn victim && victim.IsValid() )
		{
			Hitmarker = HitmarkerType.Default;

			var hc = victim.HealthComponent;
			if ( !hc.IsValid() ) return;

			var isKill = hc.Health - damageInfo.Damage <= 0f;

			TimeSinceAttacked = 0;

			if ( isKill )
				Hitmarker |= HitmarkerType.Kill;

			if ( damageInfo.Hitbox.HasFlag( HitboxTags.Head ) )
				Hitmarker |= HitmarkerType.Headshot;
		}
	}
}
