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

	public float CrosshairGap => GameSettingsSystem.Current.CrosshairDistance;
	public bool UseCrosshairDot => GameSettingsSystem.Current.ShowCrosshairDot;
	public Color CrosshairColor => GameSettingsSystem.Current.CrosshairColor;
	public bool UseDynamicCrosshair => GameSettingsSystem.Current.DynamicCrosshair;
	public float CrosshairLength => GameSettingsSystem.Current.CrosshairLength;
	public float CrosshairWidth => GameSettingsSystem.Current.CrosshairWidth;

	public HitmarkerType Hitmarker { get; set; }
	public TimeSince TimeSinceAttacked { get; set; } = 1000;

	private float HitmarkerTime => Hitmarker.HasFlag( HitmarkerType.Kill ) ? 0.5f : 0.2f;

	float mainAlpha = 1f;
	float linesAlpha = 1f;

	protected override void OnStart()
	{
		Instance = this;
	}

	protected override void OnUpdate()
	{
		var center = Screen.Size * 0.5f;

		if ( !Client.Viewer.IsValid() )
			return;

		if ( !Client.Viewer.Pawn.IsValid() )
			return;

		if ( !Client.Viewer.Pawn.Camera.IsValid() )
			return;

		if ( !MainHUD.IsHudEnabled )
			return;

		float alphaTarget = 1f;
		float linesTarget = 1f;

		var pawn = Client.Viewer.Pawn;
		var hud = Client.Viewer.Pawn.Camera.Hud;
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
				if ( !equipment.UseCrosshair ) return;

				var recoil = equipment.GetComponentInChildren<ShootRecoil>( true );
				if ( recoil.IsValid() )
				{
					gap += recoil.Current.AsVector3().Length * 175f * scale;
				}
			}

			hasAmmo = !player.HasEquipmentTag( "no_ammo" );

			if ( UseDynamicCrosshair )
				gap += player.Spread * 150f * scale;

			if ( player.CurrentEquipment.IsValid() && player.CurrentEquipment.HasTag( "aiming" ) && !isThirdPerson )
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
			hud.DrawRect( new Rect( center - ((size / 2f) * scale), size * scale ), Color.Transparent, new( float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue ), new( w, w, w, w ), color );
		}

		if ( type == CrosshairType.Dot )
		{
			hud.DrawCircle( center, w, color );
		}

		if ( TimeSinceAttacked < HitmarkerTime )
		{
			var isKillHitmarker = Hitmarker.HasFlag( HitmarkerType.Kill );
			var isHeadshotHitmarker = Hitmarker.HasFlag( HitmarkerType.Headshot );

			var initialHitmarkerLength = 20f * scale;
			var initialDiagonalOffset = CrosshairGap * scale;
			var translationFactor = isKillHitmarker ? 1.0f : 1.0f - (TimeSinceAttacked / HitmarkerTime);
			translationFactor = translationFactor.Clamp( 0.0f, 1.0f );

			var scaleFactor = isKillHitmarker ? 1.0f : (1.0f - (TimeSinceAttacked / HitmarkerTime)).Clamp( 0.0f, 1.0f );

			var hitmarkerLength = initialHitmarkerLength * scaleFactor;
			var diagonalOffset = initialDiagonalOffset * translationFactor;

			var topLeft = center + new Vector2( -diagonalOffset, -diagonalOffset );
			var topRight = center + new Vector2( diagonalOffset, -diagonalOffset );
			var bottomLeft = center + new Vector2( -diagonalOffset, diagonalOffset );
			var bottomRight = center + new Vector2( diagonalOffset, diagonalOffset );

			var hitColor = isHeadshotHitmarker || isKillHitmarker ? Color.Red : Color.White.WithAlpha( 0.5f );

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
