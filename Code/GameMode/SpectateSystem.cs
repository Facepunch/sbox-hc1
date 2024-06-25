namespace Facepunch;

public sealed class SpectateSystem : SingletonComponent<SpectateSystem>
{
	public CameraMode CameraMode { get; private set; } = CameraMode.FirstPerson;
	public bool IsSpectating => !GameUtils.LocalPlayerState.Pawn.IsValid() || GameUtils.LocalPlayerState.Pawn.HealthComponent.State != LifeState.Alive;
	public bool IsFreecam => (FreecamController as Pawn)?.IsPossessed ?? false;

	[Property] public SpectateController FreecamController { get; set; }
	
	private bool _wasSpectating { get; set; }

	private void OnSpectateBegin()
	{
		if ( !GameUtils.Viewer.IsValid() )
			return;

		// Possess the freecam by default
		FreecamController.Possess();
		_wasSpectating = true;
	}

	protected override void OnUpdate()
	{
		// Do we have no pawn? Spectate!
		if ( GameUtils.LocalPlayerState.IsValid() && !GameUtils.LocalPlayerState.Pawn.IsValid() )
		{
			UpdateSpectate();
		}
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

			( GameUtils.Viewer.Pawn as PlayerPawn ).CameraController.Mode = CameraMode;
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

			var controller = playerState.Pawn as PlayerPawn;

			if ( !controller.IsValid() )
				continue;

			if ( controller.HealthComponent.State != LifeState.Alive )
				continue;

			// Already spectating this guy, no need to reposess (and reset the viewmodel etc)
			if ( idx == idxCur )
				return;

			playerState.Possess( playerState.Pawn );
			return;
		}

		// No players to spectate, fallback to freecam
		SpectateFreecam();
	}

	private void SpectateFreecam()
	{
		if ( GameUtils.Viewer.IsValid() && GameUtils.Viewer.Pawn.IsValid() )
		{
			// Entering freecam, position ourselves at the last guy's POV
			var lastTransform = GameUtils.Viewer.GameObject.Transform;
			FreecamController.EyeAngles = GameUtils.Viewer.Pawn.EyeAngles.ToRotation();
			FreecamController.Transform.Position = lastTransform.Position - (Transform.Rotation.Forward * 128.0f);
		}

		FreecamController.Possess();
	}
}
