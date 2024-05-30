using Facepunch.UI;
using Sandbox;
using Sandbox.Diagnostics;

namespace Facepunch;

public sealed class SpectateSystem : SingletonComponent<SpectateSystem>
{
	private PlayerController LocalPlayer => GameUtils.LocalPlayer;

	public bool IsSpectating => LocalPlayer.IsSpectating;
	public bool IsFreecam => (FreecamController as IPawn).IsPossessed;

	[Property] public SpectateController FreecamController;

	protected override void OnUpdate()
	{
		if ( !LocalPlayer.IsSpectating )
			return;

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
