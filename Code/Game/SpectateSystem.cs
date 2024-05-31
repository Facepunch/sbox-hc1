using Facepunch.UI;
using Sandbox;
using Sandbox.Diagnostics;

namespace Facepunch;

public sealed class SpectateSystem : SingletonComponent<SpectateSystem>
{
	private PlayerController LocalPlayer => GameUtils.LocalPlayer;

	public CameraMode CameraMode { get; set; } = CameraMode.FirstPerson;

	private bool wasSpectating = false;
	public bool IsSpectating => LocalPlayer?.IsSpectating ?? false;
	public bool IsFreecam => (FreecamController as IPawn)?.IsPossessed ?? false;

	[Property] public SpectateController FreecamController;

	private void OnSpectateBegin()
	{
		if ( GameUtils.Viewer.GameObject.Tags.Has( "invis" ) )
		{
			// prevent us trying to spectate an invisible/spectator pawn
			// (usually ourselves when we've just joined)
			SpectateNextPlayer( true );
		}
	}

	protected override void OnUpdate()
	{
		if ( LocalPlayer.IsSpectating )
			UpdateSpectate();

		wasSpectating = LocalPlayer.IsSpectating;
	}

	private void UpdateSpectate()
	{
		if ( !wasSpectating )
			OnSpectateBegin();

		if ( Input.Pressed( "SpectatorNext" ) )
		{
			SpectateNextPlayer( true );
		}
		else if ( Input.Pressed( "SpectatorPrev" ) )
		{
			SpectateNextPlayer( false );
		}
		else if ( Input.Pressed( "SpectatorFreeCam" ) )
		{
			SpectateFreecam();
		}
		else if ( Input.Pressed( "SpectatorMode" ) )
		{
			if ( !IsFreecam && GameUtils.Viewer != GameUtils.LocalPlayer )
			{
				int max = (int)CameraMode.ThirdPerson + 1;
				CameraMode = (CameraMode)((((int)CameraMode) + 1) % max);

				GameUtils.Viewer.CameraController.Mode = CameraMode;
			}
		}
	}

	private void SpectateNextPlayer( bool direction )
	{
		var players = Scene.GetAllComponents<PlayerController>();
		int idxCur = 0;
		for ( int i = 0; i < players.Count(); i++ )
		{
			if ( players.ElementAt( i ) == GameUtils.Viewer )
				idxCur = i;
		}

		int count = players.Count();
		for ( int i = 1; i < count; i++ )
		{
			int idx = (idxCur + (direction ? i : -i) + count) % count;

			var element = players.ElementAt( idx );
			if ( !element.IsSpectating )
			{
				(element as IPawn).Possess();
				return;
			}
		}

		// no players to spectate, fallback to freecam
		SpectateFreecam();
		return;
	}

	private void SpectateFreecam()
	{
		if ( GameUtils.Viewer is not null )
		{
			// entering freecam, position ourselves at the last guy's pov
			var lastTransform = GameUtils.Viewer.Transform;
			FreecamController.ViewAngles = GameUtils.Viewer.EyeAngles.ToRotation();
			FreecamController.Transform.Position = lastTransform.Position - (Transform.Rotation.Forward * 128.0f);
		}

		( FreecamController as IPawn).Possess();
	}
}
