using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch;

partial class PlayerController
{
	/// <summary>
	/// Is this player in spectate mode
	/// </summary>
	public bool IsSpectating => HealthComponent.State == LifeState.Dead;

	private void SpectateUpdate()
	{
		if ( Input.Pressed( "attack1" ) )
		{
			var players = Scene.GetAllComponents<PlayerController>();
			int idx = 0;
			int idxSelf = 0;
			for ( int i = 0; i < players.Count(); i++ )
			{
				if ( players.ElementAt(i) == GameUtils.Viewer )
					idx = i;

				if ( players.ElementAt( i ) == this )
					idxSelf = i;
			}

			idx = (idx + 1) % players.Count();
			if ( idx == idxSelf )
			{
				Transform.Rotation = GameUtils.Viewer.EyeAngles.ToRotation();
				Transform.Position = GameUtils.Viewer.Transform.Position + (Transform.Rotation.Forward * 3.0f);
			}

			var player = players.ElementAt( idx );
			(player as IPawn).Possess();
		}

		// freecam
		if ( IsViewer )
		{
			Transform.Position += Input.AnalogMove * CameraController.Camera.Transform.Rotation * NoclipSpeed * Time.Delta;
		}
	}
}
