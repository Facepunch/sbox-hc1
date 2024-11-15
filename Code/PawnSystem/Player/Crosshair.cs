using Facepunch.UI;

namespace Facepunch;

public enum CrosshairType
{
	/// <summary>
	/// Traditional crosshair, 4 lines, 1 dot. Configurable.
	/// </summary>
	Default,
	
	/// <summary>
	/// Three lines - the default without the top line.
	/// </summary>
	ThreeLines,

	/// <summary>
	/// An arc and a dot
	/// </summary>
	Shotgun,

	/// <summary>
	/// Just the dot, even if it's disabled
	/// </summary>
	Dot
}

[Flags]
public enum HitmarkerType
{
	Default = 1,
	Kill = 2,
	Headshot = 4
}

public partial class Crosshair : Component
{
	public static Crosshair Instance { get; set; }

	float mainAlpha = 1f;
	float linesAlpha = 1f;

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

	public float CrosshairWidth
	{
		get
		{
			return GameSettingsSystem.Current.CrosshairWidth;
		}
	}

	public HitmarkerType Hitmarker { get; set; }
	public TimeSince TimeSinceAttacked { get; set; } = 1000;

	private float HitmarkerTime => Hitmarker.HasFlag( HitmarkerType.Kill ) ? 0.5f : 0.2f;

	protected override void OnStart()
	{
		Instance = this;
	}

	protected override void OnUpdate()
	{
		var center = Screen.Size * 0.5f;

		if ( !PlayerState.Viewer.IsValid() )
			return;

		if ( !PlayerState.Viewer.Pawn.IsValid() )
			return;

		if ( !MainHUD.IsHudEnabled )
			return;

		float alphaTarget = 1f;
		float linesTarget = 1f;

		var pawn = PlayerState.Viewer.Pawn;
		var hud = PlayerState.Viewer.Pawn.Camera.Hud;
		var scale = Screen.Height / 1080.0f;

		var gap = CrosshairGap * scale;
		var len = CrosshairLength * scale;
		var w = CrosshairWidth * scale;

		Color color = CrosshairColor;
		CrosshairType type = CrosshairType.Default;
		bool hasAmmo = true;

		var player = pawn as PlayerPawn;
		if ( player.IsValid() && player.CameraController.IsValid() )
		{
			bool isThirdPerson = player.CameraController.Mode == CameraMode.ThirdPerson;

			if ( isThirdPerson )
			{
				if ( !player.IsLocallyControlled )
					color = Color.Transparent;
			}

			var equipment = player.CurrentEquipment;

			if ( equipment.IsValid() )
			{
				type = equipment.CrosshairType;

				var recoil = equipment.GetComponentInChildren<RecoilWeaponComponent>( true );
				if ( recoil.IsValid() )
				{
					gap += recoil.Current.AsVector3().Length * 175f * scale;
				}
			}

			hasAmmo = !player.HasEquipmentTag( "no_ammo" );

			if ( UseDynamicCrosshair )
				gap += player.Spread * 150f * scale;

			if ( player.CurrentEquipment.IsValid() && player.CurrentEquipment.EquipmentFlags.HasFlag( EquipmentFlags.Aiming ) && !isThirdPerson )
			{
				mainAlpha = 0;
				linesAlpha = 0;
			}
		}

		hud.SetBlendMode( BlendMode.Lighten );

		if ( player.IsValid() && player.HasEquipmentTag( "reloading" ) )
		{
			linesTarget = 0.25f;
		}

		color = color.WithAlpha( mainAlpha );

		var linesCol = color;
		if ( !hasAmmo )
		{
			linesCol = Color.Red;
			linesTarget *= 0.25f;
		}

		if ( player.IsValid() && player.IsSprinting )
		{
			linesTarget = 0;
		}

		linesCol = linesCol.WithAlpha( linesAlpha );

		if ( type == CrosshairType.Default || type == CrosshairType.ThreeLines )
		{
			hud.DrawLine( center + Vector2.Left * (len + gap), center + Vector2.Left * gap, w, linesCol );
			hud.DrawLine( center - Vector2.Left * (len + gap), center - Vector2.Left * gap, w, linesCol );
			hud.DrawLine( center + Vector2.Up * (len + gap), center + Vector2.Up * gap, w, linesCol );

			if ( type != CrosshairType.ThreeLines )
			{
				hud.DrawLine( center - Vector2.Up * (len + gap), center - Vector2.Up * gap, w, linesCol );
			}

			if ( UseCrosshairDot )
			{
				hud.DrawCircle( center, w, color );
			}
		}
		
		if ( type == CrosshairType.Shotgun )
		{
			float scaleFactor = (1.0f - (TimeSinceAttacked / HitmarkerTime)).Clamp( 0.0f, 1.0f );

			var size = 0f;
			size += gap;

			hud.DrawCircle( center, w, color );
			hud.DrawRect( new Rect( center - (( size / 2f ) * scale), size * scale ), Color.Transparent, new( float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue ), new( w, w, w, w ), color );
		}

		if ( type == CrosshairType.Dot )
		{
			hud.DrawCircle( center, w, color );
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

		mainAlpha = mainAlpha.LerpTo( alphaTarget, Time.Delta * 30f );
		linesAlpha = linesAlpha.LerpTo( linesTarget, Time.Delta * 30f );
	}

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
