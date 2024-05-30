using Sandbox.Diagnostics;

namespace Facepunch;

partial class PlayerController
{
	/// <summary>
	/// Is this player in spectate mode
	/// </summary>
	public bool IsSpectating => HealthComponent.State == LifeState.Dead;

	private void SpectateUpdate()
	{
		if ( Input.Pressed( "SpectatorNext" ) )
		{
			SpectateNextPlayer( true );
		}
		else if ( Input.Pressed( "SpectatorPrev" ) )
		{
			SpectateNextPlayer( false );
		}
		else if( Input.Pressed( "SpectatorFreeCam" ) )
		{
			SpectateFreecam();
		}

		// freecam
		if ( IsViewer )
		{
			Transform.Position += Input.AnalogMove * CameraController.Camera.Transform.Rotation * NoclipSpeed * Time.Delta;
		}
	}

	private void SpectateNextPlayer( bool direction )
	{
		Assert.True( IsSpectating );

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
		Assert.True( IsSpectating );

		if ( IsViewer )
			return;

		// entering freecam, position ourselves at the last guy's pov
		var lastTransform = GameUtils.Viewer.Transform;
		Transform.Rotation = GameUtils.Viewer.EyeAngles.ToRotation();
		Transform.Position = lastTransform.Position - (Transform.Rotation.Forward * 128.0f);
		EyeAngles = Angles.Zero;

		( this as IPawn ).Possess();
	}
}
