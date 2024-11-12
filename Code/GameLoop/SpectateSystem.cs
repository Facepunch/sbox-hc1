namespace Facepunch;

public sealed class SpectateSystem : SingletonComponent<SpectateSystem>
{
	public CameraMode CameraMode { get; private set; } = CameraMode.FirstPerson;
	// todo: make this not skip deathcams
	public bool IsSpectating => !PlayerState.Local.IsValid() || !PlayerState.Local.PlayerPawn.IsValid() || PlayerState.Local.PlayerPawn != PlayerState.Viewer.Pawn;
	public bool IsFreecam => (FreecamController as Pawn)?.IsPossessed ?? false;

	[Property] public SpectateController FreecamController { get; set; }
	
	private bool _wasSpectating { get; set; }

	private void OnSpectateBegin()
	{
		// try to find someone to spectate
		SpectateNextPlayer( true );
	}

	protected override void OnUpdate()
	{
		// TODO: Fix this, this sucks
		if ( PlayerState.Viewer.PlayerPawn.IsValid() )
		{
			PlayerState.Local.Possess();
		}

		// Do we have no pawn? Spectate!
		if ( IsSpectating )
		{
			UpdateSpectate();
		}

		_wasSpectating = IsSpectating;
	}

	private void UpdateSpectate()
	{
		if ( !_wasSpectating )
			OnSpectateBegin();

		if ( !PlayerState.Viewer.IsValid() )
			return;

		if ( Input.Pressed( "SpectatorNext" ) || !PlayerState.Viewer.Pawn.IsValid() )
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
			if ( IsFreecam || PlayerState.Viewer.IsLocalPlayer )
				return;

			const int max = (int)CameraMode.ThirdPerson + 1;
			CameraMode = (CameraMode)((((int)CameraMode) + 1) % max);

			( PlayerState.Viewer.PlayerPawn ).CameraController.Mode = CameraMode;
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

			var controller = playerState.PlayerPawn;

			if ( !controller.IsValid() )
				continue;

			if ( controller.HealthComponent.State != LifeState.Alive )
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
		if ( !FreecamController.IsValid() )
			return;

		if ( PlayerState.Viewer.IsValid() && PlayerState.Viewer.Pawn.IsValid() )
		{
			// Entering freecam, position ourselves at the last guy's POV
			var rotation = PlayerState.Viewer.Pawn.EyeAngles;
			FreecamController.EyeAngles = rotation;
			FreecamController.WorldPosition = PlayerState.Viewer.Pawn.GameObject.WorldPosition + (rotation.Forward * 8.0f);
		}

		FreecamController.Possess();
	}
}
