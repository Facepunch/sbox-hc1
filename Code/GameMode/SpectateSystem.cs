namespace Facepunch;

public sealed class SpectateSystem : SingletonComponent<SpectateSystem>
{
	private PlayerController LocalPlayer => GameUtils.LocalPlayer;

	public CameraMode CameraMode { get; private set; } = CameraMode.FirstPerson;
	public bool IsSpectating => LocalPlayer?.IsSpectating ?? false;
	public bool IsFreecam => (FreecamController as IPawn)?.IsPossessed ?? false;

	[Property] public SpectateController FreecamController { get; set; }
	
	private bool _wasSpectating { get; set; }

	private void OnSpectateBegin()
	{
		if ( GameUtils.Viewer.GameObject.Tags.Has( "invis" ) )
		{
			// Prevent us trying to spectate an invisible/spectator pawn
			// (usually ourselves when we've just joined)
			SpectateNextPlayer( true );
		}
	}

	protected override void OnUpdate()
	{
		var isSpectating = LocalPlayer?.IsSpectating ?? false;

		if ( isSpectating )
			UpdateSpectate();

		_wasSpectating = isSpectating;
	}

	private void UpdateSpectate()
	{
		if ( !_wasSpectating )
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
			if ( IsFreecam || GameUtils.Viewer.IsLocalPlayer )
				return;

			const int max = (int)CameraMode.ThirdPerson + 1;
			CameraMode = (CameraMode)((((int)CameraMode) + 1) % max);

			GameUtils.Viewer.Controller.CameraController.Mode = CameraMode;
		}
	}

	private void SpectateNextPlayer( bool direction )
	{
		var players = Scene.GetAllComponents<PlayerState>();
		
		var idxCur = 0;
		for ( var i = 0; i < players.Count(); i++ )
		{
			if ( players.ElementAt( i ).IsViewer )
				idxCur = i;
		}

		var count = players.Count();
		for ( var i = 1; i <= count; i++ )
		{
			var idx = (idxCur + (direction ? i : -i) + count) % count;
			var playerState = players.ElementAt( idx );

			if ( playerState.Controller is null )
				continue;

			if ( playerState.Controller.IsSpectating )
				continue;

			// Already spectating this guy, no need to reposess (and reset the viewmodel etc)
			if ( idx == idxCur )
				return;

			playerState.Possess();
			return;
		}

		// No players to spectate, fallback to freecam
		SpectateFreecam();
	}

	private void SpectateFreecam()
	{
		if ( GameUtils.Viewer is not null )
		{
			// Entering freecam, position ourselves at the last guy's POV
			var lastTransform = GameUtils.Viewer.GameObject.Transform;
			FreecamController.ViewAngles = GameUtils.Viewer.Controller.EyeAngles.ToRotation();
			FreecamController.Transform.Position = lastTransform.Position - (Transform.Rotation.Forward * 128.0f);
		}

		( FreecamController as IPawn).Possess();
	}
}
